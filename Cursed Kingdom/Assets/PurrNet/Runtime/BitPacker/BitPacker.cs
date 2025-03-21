using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using K4os.Compression.LZ4;
using PurrNet.Modules;
using PurrNet.Transports;

namespace PurrNet.Packing
{
    public readonly struct BitPackerWithLength : IDisposable
    {
        public readonly int originalLength;
        public readonly BitPacker packer;

        public BitPackerWithLength(int ogLength, BitPacker packer)
        {
            originalLength = ogLength;
            this.packer = packer;
        }

        public void Dispose()
        {
            packer.Dispose();
        }
    }

    public readonly struct BitPackerWrapper : IBufferWriter<byte>, IDisposable
    {
        public readonly BitPacker packer;

        public BitPackerWrapper(BitPacker packer)
        {
            this.packer = packer;
        }

        public void Advance(int count)
        {
            packer.AdvanceBits(count * 9);
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            packer.EnsureBitsExist(sizeHint * 8);
            return new Memory<byte>(packer.buffer, packer.positionInBytes, sizeHint);
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            packer.EnsureBitsExist(sizeHint * 8);
            return new Span<byte>(packer.buffer, packer.positionInBytes, sizeHint);
        }

        public void Dispose()
        {
            packer?.Dispose();
        }
    }

    [UsedImplicitly]
    public partial class BitPacker : IDisposable
    {
        private byte[] _buffer;
        private bool _isReading;
        public byte[] buffer => _buffer;

        public bool isWrapper { get; private set; }

        public int positionInBits { get; private set; }

        public int positionInBytes
        {
            get
            {
                int pos = positionInBits / 8;
                int len = pos + (positionInBits % 8 == 0 ? 0 : 1);
                return len;
            }
        }

        public int length
        {
            get
            {
                if (isWrapper)
                    return _buffer.Length;
                return positionInBytes;
            }
        }

        public bool isReading => _isReading;

        public bool isWriting => !_isReading;

        /// <summary>
        /// Pickles the current buffer into the provided BitPacker.
        /// </summary>
        public void PickleInto(BitPacker packer, LZ4Level level = LZ4Level.L00_FAST)
        {
            LZ4Pickler.Pickle(ToByteData().span, new BitPackerWrapper(packer), level);
        }

        /// <summary>
        /// Unpickles the provided ByteData into the current BitPacker.
        /// </summary>
        public void UnpickleFrom(ByteData data)
        {
            LZ4Pickler.Unpickle(data.span, new BitPackerWrapper(this));
        }

        /// <summary>
        /// Unpickles the provided BitPacker into the current BitPacker.
        /// </summary>
        public void UnpickleFrom(BitPacker data)
        {
            LZ4Pickler.Unpickle(data.ToByteData().span, new BitPackerWrapper(this));
        }

        /// <summary>
        /// Pickles the current buffer into a new BitPacker.
        /// Don't forget to dispose of the returned BitPacker.
        /// </summary>
        public BitPacker Pickle(LZ4Level level = LZ4Level.L00_FAST)
        {
            var packer = BitPackerPool.Get();
            PickleInto(packer, level);
            return packer;
        }

        public void Advance(int count)
        {
            EnsureBitsExist(count * 8);
            positionInBits += count * 8;
        }

        public int AdvanceBits(int bitCount)
        {
            EnsureBitsExist(bitCount);
            var old = positionInBits;
            positionInBits += bitCount;
            return old;
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            EnsureBitsExist(sizeHint * 8);
            return new Memory<byte>(_buffer, positionInBytes, sizeHint);
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            EnsureBitsExist(sizeHint * 8);
            return new Span<byte>(_buffer, positionInBytes, sizeHint);
        }

        public BitPacker(int initialSize = 1024)
        {
            _buffer = new byte[initialSize];
        }

        public void MakeWrapper(ByteData data)
        {
            _buffer = data.data;
            positionInBits = data.offset * 8;
            isWrapper = true;
        }

        public void Dispose()
        {
            BitPackerPool.Free(this);
        }

        public ByteData ToByteData()
        {
            return new ByteData(_buffer, 0, length);
        }

        public void ResetPosition()
        {
            positionInBits = 0;
        }

        public void ResetMode(bool readMode)
        {
            _isReading = readMode;
        }

        public void SetBitPosition(int bitPosition)
        {
            positionInBits = bitPosition;
        }

        public void SkipBytes(int skip)
        {
            positionInBits += skip * 8;
        }

        public void SkipBytes(uint skip)
        {
            positionInBits += (int)skip * 8;
        }

        public void ResetPositionAndMode(bool readMode)
        {
            positionInBits = 0;
            _isReading = readMode;
        }

        public void EnsureBitsExist(int bits)
        {
            int targetPos = positionInBits + bits;
            var bufferBitSize = _buffer.Length * 8;

            if (targetPos > bufferBitSize)
            {
                if (_isReading)
                    throw new IndexOutOfRangeException("Not enough bits in the buffer. | " + targetPos + " > " +
                                                       bufferBitSize);
                Array.Resize(ref _buffer, _buffer.Length * 2);
            }
        }

        private void EnsureBitsExist(int positionInBits, int bits)
        {
            int targetPos = positionInBits + bits;
            var bufferBitSize = _buffer.Length * 8;

            if (targetPos > bufferBitSize)
            {
                if (_isReading)
                    throw new IndexOutOfRangeException("Not enough bits in the buffer. | " + targetPos + " > " +
                                                       bufferBitSize);
                Array.Resize(ref _buffer, _buffer.Length * 2);
            }
        }

        [UsedByIL]
        public bool HandleNullScenarios<T>(T oldValue, T newValue, ref bool areEqual) where T : class
        {
            if (oldValue == null)
            {
                if (newValue == null)
                {
                    areEqual = true;
                    return false;
                }

                areEqual = false;
                Packer<T>.Write(this, newValue);
                return false;
            }

            if (newValue == null)
            {
                areEqual = false;
                Packer<T>.Write(this, null);
                return false;
            }

            return true;
        }

        [UsedByIL]
        public bool WriteIsNull<T>(T value) where T : class
        {
            if (value == null)
            {
                WriteBits(1, 1);
                return false;
            }

            WriteBits(0, 1);
            return true;
        }

        [UsedByIL]
        public bool ReadIsNull<T>(ref T value) where T : class
        {
            if (ReadBits(1) == 1)
            {
                value = default;
                return false;
            }

            value = Activator.CreateInstance<T>();
            return true;
        }

        public void WriteBits(ulong data, byte bits)
        {
            EnsureBitsExist(bits);

            if (bits > 64)
                throw new ArgumentOutOfRangeException(nameof(bits), "Cannot write more than 64 bits at a time.");

            int bitsLeft = bits;

            while (bitsLeft > 0)
            {
                int bytePos = positionInBits / 8;
                int bitOffset = positionInBits % 8;
                int bitsToWrite = Math.Min(bitsLeft, 8 - bitOffset);

                byte mask = (byte)((1 << bitsToWrite) - 1);
                byte value = (byte)((data >> (bits - bitsLeft)) & mask);

                _buffer[bytePos] &= (byte)~(mask << bitOffset); // Clear the bits to be written
                _buffer[bytePos] |= (byte)(value << bitOffset); // Set the bits

                bitsLeft -= bitsToWrite;
                positionInBits += bitsToWrite;
            }
        }

        public ulong ReadBits(byte bits)
        {
            if (bits > 64)
                throw new ArgumentOutOfRangeException(nameof(bits), "Cannot read more than 64 bits at a time.");

            ulong result = 0;
            int bitsLeft = bits;

            while (bitsLeft > 0)
            {
                int bytePos = positionInBits / 8;
                int bitOffset = positionInBits % 8;
                int bitsToRead = Math.Min(bitsLeft, 8 - bitOffset);

                byte mask = (byte)((1 << bitsToRead) - 1);
                byte value = (byte)((_buffer[bytePos] >> bitOffset) & mask);

                result |= (ulong)value << (bits - bitsLeft);

                bitsLeft -= bitsToRead;
                positionInBits += bitsToRead;
            }

            return result;
        }

        public void ReadBytes(BitPacker target, int count)
        {
            EnsureBitsExist(count * 8);

            int excess = count % 8;
            int fullChunks = count / 8;

            // Process excess bytes (remaining bytes before full 64-bit chunks)
            for (int i = 0; i < excess; i++)
            {
                target.WriteBits(ReadBits(8), 8);
            }

            // Process full 64-bit chunks
            for (int i = 0; i < fullChunks; i++)
                target.WriteBits(ReadBits(64), 64);
        }

        public void ReadBytes(IList<byte> bytes)
        {
            int count = bytes.Count;

            EnsureBitsExist(count * 8);

            int excess = count % 8;
            int fullChunks = count / 8;

            int index = 0;

            // Process excess bytes (remaining bytes before full 64-bit chunks)
            for (int i = 0; i < excess; i++)
            {
                bytes[index++] = (byte)ReadBits(8);
            }

            // Process full 64-bit chunks
            for (int i = 0; i < fullChunks; i++)
            {
                var longValue = ReadBits(64);

                for (int j = 0; j < 8; j++)
                {
                    if (index < count)
                    {
                        bytes[index++] = (byte)(longValue >> (j * 8));
                    }
                }
            }
        }

        public void WriteBytes(ByteData byteData)
        {
            WriteBytes(byteData.span);
        }

        public void WriteBytes(BitPacker other, int count)
        {
            EnsureBitsExist(count * 8);

            int fullChunks = count / 8;
            int excess = count % 8;

            // Process full 64-bit chunks
            for (int i = 0; i < fullChunks; i++)
                WriteBits(other.ReadBits(64), 64);

            // Process excess bytes (remaining bytes before full 64-bit chunks)
            for (int i = 0; i < excess; i++)
                WriteBits(other.ReadBits(8), 8);
        }

        public void WriteBytes(ReadOnlySpan<byte> bytes)
        {
            EnsureBitsExist(bytes.Length * 8);

            int count = bytes.Length;
            int fullChunks = count / 8; // Number of full 64-bit chunks
            int excess = count % 8; // Remaining bytes after full chunks

            int index = 0;

            // Process full 64-bit chunks
            for (int i = 0; i < fullChunks; i++)
            {
                ulong longValue = 0;

                // Combine 8 bytes into a single 64-bit value
                for (int j = 0; j < 8; j++)
                    longValue |= (ulong)bytes[index++] << (j * 8);

                // Write the 64-bit chunk
                WriteBits(longValue, 64);
            }

            // Process remaining excess bytes
            for (int i = 0; i < excess; i++)
            {
                WriteBits(bytes[index++], 8);
            }
        }

        public void SkipBits(int skip)
        {
            positionInBits += skip;
        }

        public void WriteString(Encoding utf8, string valueValue)
        {
            if (valueValue == null)
            {
                WriteBits(0, 1);
                return;
            }

            WriteBits(1, 1);

            byte[] bytes = utf8.GetBytes(valueValue);
            WriteBits((ulong)bytes.Length, 31);
            WriteBytes(bytes);
        }

        public string ReadString(Encoding utf8)
        {
            if (ReadBits(1) == 0)
                return null;

            int byteCount = (int)ReadBits(31);
            byte[] bytes = new byte[byteCount];

            ReadBytes(bytes);
            return utf8.GetString(bytes);
        }

        public char ReadChar()
        {
            return (char)ReadBits(8);
        }

        public void WriteAt(int positionInBits, bool data)
        {
            WriteBitsAt(positionInBits, data ? 1UL : 0UL, 1);
        }

        public void WriteBitsAt(int positionInBits, ulong data, byte bits)
        {
            EnsureBitsExist(positionInBits, bits);

            if (bits > 64)
                throw new ArgumentOutOfRangeException(nameof(bits), "Cannot write more than 64 bits at a time.");

            int bitsLeft = bits;

            while (bitsLeft > 0)
            {
                int bytePos = positionInBits / 8;
                int bitOffset = positionInBits % 8;
                int bitsToWrite = Math.Min(bitsLeft, 8 - bitOffset);

                byte mask = (byte)((1 << bitsToWrite) - 1);
                byte value = (byte)((data >> (bits - bitsLeft)) & mask);

                _buffer[bytePos] &= (byte)~(mask << bitOffset); // Clear the bits to be written
                _buffer[bytePos] |= (byte)(value << bitOffset); // Set the bits

                bitsLeft -= bitsToWrite;
                positionInBits += bitsToWrite;
            }
        }
    }
}
