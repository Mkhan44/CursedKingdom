using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using PurrNet.Modules;
using PurrNet.Transports;

namespace PurrNet.Packing
{
    [UsedImplicitly]
    public partial class BitPacker : IDisposable
    {
        private byte[] _buffer;
        private bool _isReading;
        
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
        
        private void EnsureBitsExist(int bits)
        {
            int targetPos = positionInBits + bits;
            var bufferBitSize = _buffer.Length * 8;
            
            if (targetPos > bufferBitSize)
            {
                if (_isReading)
                    throw new IndexOutOfRangeException("Not enough bits in the buffer. | " + targetPos + " > " + bufferBitSize);
                Array.Resize(ref _buffer, _buffer.Length * 2);
            }
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
            int excess = count % 8;     // Remaining bytes after full chunks

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
    }
}
