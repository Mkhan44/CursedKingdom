using System;
using System.Collections.Generic;
using System.Reflection;
using PurrNet.Logging;
using PurrNet.Modules;
using PurrNet.Utils;

namespace PurrNet.Packing
{
    public delegate bool DeltaWriteFunc<in T>(BitPacker packer, T oldValue, T newValue);

    public delegate void DeltaReadFunc<T>(BitPacker packer, T oldValue, ref T value);

    public delegate void WriteFunc<in T>(BitPacker packer, T value);

    public delegate void ReadFunc<T>(BitPacker packer, ref T value);

    public static class DeltaPacker<T>
    {
        static DeltaWriteFunc<T> _write;
        static DeltaReadFunc<T> _read;

        public static void Register(DeltaWriteFunc<T> write, DeltaReadFunc<T> read)
        {
            RegisterWriter(write);
            RegisterReader(read);
        }

        public static void RegisterWriter(DeltaWriteFunc<T> a)
        {
            if (_write != null)
                return;
            _write = a;
        }

        public static void RegisterReader(DeltaReadFunc<T> b)
        {
            if (_read != null)
                return;
            _read = b;
        }

        public static bool Write(BitPacker packer, T oldValue, T newValue)
        {
            try
            {
                if (_write == null)
                {
                    PurrLogger.LogError($"No delta writer for type '{typeof(T)}' is registered.");
                    return false;
                }

                return _write(packer, oldValue, newValue);
            }
            catch (Exception e)
            {
                PurrLogger.LogError($"Failed to delta write value of type '{typeof(T)}'.\n{e.Message}\n{e.StackTrace}");
                return false;
            }
        }

        public static void Read(BitPacker packer, T oldValue, ref T value)
        {
            try
            {
                if (_read == null)
                {
                    PurrLogger.LogError($"No delta reader for type '{typeof(T)}' is registered.");
                    return;
                }

                _read(packer, oldValue, ref value);
            }
            catch (Exception e)
            {
                PurrLogger.LogError($"Failed to delta read value of type '{typeof(T)}'.\n{e.Message}\n{e.StackTrace}");
            }
        }

        public static void Serialize(BitPacker packer, T oldValue, ref T value)
        {
            if (packer.isWriting)
                Write(packer, oldValue, value);
            else Read(packer, oldValue, ref value);
        }
    }

    public static class Packer<T>
    {
        static WriteFunc<T> _write;
        static ReadFunc<T> _read;

        public static void RegisterWriter(WriteFunc<T> a)
        {
            Packer.RegisterWriter(typeof(T), a.Method);
            _write ??= a;
        }

        public static void RegisterReader(ReadFunc<T> b)
        {
            Packer.RegisterReader(typeof(T), b.Method);
            _read ??= b;
        }

        public static void Write(BitPacker packer, T value)
        {
            try
            {
                if (_write == null)
                {
                    Packer.FallbackWriter(packer, value);
                    return;
                }

                _write(packer, value);
            }
            catch (Exception e)
            {
                PurrLogger.LogError($"Failed to write value of type '{typeof(T)}'.\n{e.Message}\n{e.StackTrace}");
            }
        }

        public static void Read(BitPacker packer, ref T value)
        {
            try
            {
                if (_read == null)
                {
                    Packer.FallbackReader(packer, ref value);
                    return;
                }

                _read(packer, ref value);
            }
            catch (Exception e)
            {
                PurrLogger.LogError($"Failed to read value of type '{typeof(T)}'.\n{e.Message}\n{e.StackTrace}");
            }
        }

        public static void Serialize(BitPacker packer, ref T value)
        {
            if (packer.isWriting)
                Write(packer, value);
            else Read(packer, ref value);
        }
    }

    public static class Packer
    {
        [UsedByIL]
        public static bool AreEqual<T>(T a, T b)
        {
            using var packerA = BitPackerPool.Get();
            using var packerB = BitPackerPool.Get();

            Write(packerA, a);
            Write(packerB, b);

            return packerA.ToByteData().span.SequenceEqual(packerB.ToByteData().span);
        }

        [UsedByIL]
        public static bool AreEqualRef<T>(ref T a, ref T b)
        {
            using var packerA = BitPackerPool.Get();
            using var packerB = BitPackerPool.Get();

            Write(packerA, a);
            Write(packerB, b);

            return packerA.ToByteData().span.SequenceEqual(packerB.ToByteData().span);
        }

        static readonly Dictionary<Type, MethodInfo> _writeMethods = new Dictionary<Type, MethodInfo>();
        static readonly Dictionary<Type, MethodInfo> _readMethods = new Dictionary<Type, MethodInfo>();

        public static void RegisterWriter(Type type, MethodInfo method)
        {
            _writeMethods.TryAdd(type, method);
        }

        public static void RegisterReader(Type type, MethodInfo method)
        {
            Hasher.PrepareType(type);
            _readMethods.TryAdd(type, method);
        }

        static readonly object[] _args = new object[2];

        public static void FallbackWriter<T>(BitPacker packer, T value)
        {
            try
            {
                bool hasValue = value != null;
                Packer<bool>.Write(packer, hasValue);

                if (!hasValue) return;

                object obj = value;
                uint typeHash = Hasher.GetStableHashU32(obj.GetType());

                Packer<uint>.Write(packer, typeHash);
                Write(packer, obj);
            }
            catch (Exception e)
            {
                PurrLogger.LogError(
                    $"Failed to write value of type '{typeof(T)}' when using fallback writer.\n{e.Message}\n{e.StackTrace}");
            }
        }

        public static void FallbackReader<T>(BitPacker packer, ref T value)
        {
            try
            {
                bool hasValue = default;
                Packer<bool>.Read(packer, ref hasValue);

                if (!hasValue)
                {
                    value = default;
                    return;
                }

                uint typeHash = default;
                Packer<uint>.Read(packer, ref typeHash);

                var type = Hasher.ResolveType(typeHash);

                object obj = null;
                Read(packer, type, ref obj);

                if (obj is T entity)
                    value = entity;
                else value = default;
            }
            catch (Exception e)
            {
                PurrLogger.LogError(
                    $"Failed to read value of type '{typeof(T)}' when using fallback reader.\n{e.Message}\n{e.StackTrace}");
            }
        }

        public static void Write(BitPacker packer, object value)
        {
            var type = value.GetType();

            if (!_writeMethods.TryGetValue(type, out var method))
            {
                PurrLogger.LogError($"No writer for type '{type}' is registered.");
                return;
            }

            try
            {
                _args[0] = packer;
                _args[1] = value;
                method.Invoke(null, _args);
            }
            catch (Exception e)
            {
                PurrLogger.LogError($"Failed to write value of type '{type}'.\n{e.Message}\n{e.StackTrace}");
            }
        }

        public static void Read(BitPacker packer, Type type, ref object value)
        {
            if (!_readMethods.TryGetValue(type, out var method))
            {
                PurrLogger.LogError($"No reader for type '{type}' is registered.");
                return;
            }

            try
            {
                _args[0] = packer;
                _args[1] = value;
                method.Invoke(null, _args);
                value = _args[1];
            }
            catch (Exception e)
            {
                PurrLogger.LogError($"Failed to read value of type '{type}'.\n{e.Message}\n{e.StackTrace}");
            }
        }

        public static void Serialize(BitPacker packer, Type type, ref object value)
        {
            if (packer.isWriting)
                Write(packer, value);
            else Read(packer, type, ref value);
        }
    }
}
