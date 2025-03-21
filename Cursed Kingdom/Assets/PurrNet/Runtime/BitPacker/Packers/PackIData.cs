using System;
using PurrNet.Modules;
using PurrNet.Utils;

namespace PurrNet.Packing
{
    public static class PackIData
    {
        [UsedByIL]
        public static void Write(this BitPacker packer, IPacked value)
        {
            var hash = Hasher.GetStableHashU32(value.GetType());
            packer.WriteBits(hash, 32);
            value.Write(packer);
        }

        [UsedByIL]
        public static void Read(this BitPacker packer, ref IPacked value)
        {
            uint hash = default;
            packer.Read(ref hash);

            var type = Hasher.ResolveType(hash);
            object obj = value;
            Packer.Read(packer, type, ref obj);

            if (obj is IPacked packed)
            {
                value = packed;
            }
            else
                throw new InvalidCastException(
                    $"Failed to cast object of type '{obj.GetType()}' to '{typeof(IPacked)}'.");
        }

        [UsedByIL]
        public static void Write(this BitPacker packer, IPackedSimple value)
        {
            var hash = Hasher.GetStableHashU32(value.GetType());
            packer.WriteBits(hash, 32);
            Packer.Write(packer, value);
        }

        [UsedByIL]
        public static void Read(this BitPacker packer, ref IPackedSimple value)
        {
            uint hash = default;
            packer.Read(ref hash);

            var type = Hasher.ResolveType(hash);

            object obj = value;
            Packer.Read(packer, type, ref obj);

            if (obj is IPackedSimple packed)
            {
                value = packed;
            }
            else
                throw new InvalidCastException(
                    $"Failed to cast object of type '{obj.GetType()}' to '{typeof(IPackedSimple)}'.");
        }
    }
}