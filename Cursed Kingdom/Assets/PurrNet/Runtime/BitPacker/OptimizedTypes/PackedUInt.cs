using System;
using PurrNet.Modules;
using UnityEngine;

namespace PurrNet.Packing
{
    [Serializable]
    public struct PackedByte
    {
        public byte value;
        
        public PackedByte(byte value)
        {
            this.value = value;
        }
        
        public static implicit operator PackedByte(byte value) => new PackedByte(value);
        
        public static implicit operator byte(PackedByte value) => value.value;
    }
    
    [Serializable]
    public struct PackedSByte
    {
        public sbyte value;
        
        public PackedSByte(sbyte value)
        {
            this.value = value;
        }
        
        public static implicit operator PackedSByte(sbyte value) => new PackedSByte(value);
        
        public static implicit operator sbyte(PackedSByte value) => value.value;
    }
    
    [Serializable]
    public struct PackedULong
    {
        public ulong value;
        
        public PackedULong(ulong value)
        {
            this.value = value;
        }
        
        public static implicit operator PackedULong(ulong value) => new PackedULong(value);
        
        public static implicit operator ulong(PackedULong value) => value.value;
    }
    
    [Serializable]
    public struct PackedLong
    {
        public long value;
        
        public PackedLong(long value)
        {
            this.value = value;
        }
        
        public static implicit operator PackedLong(long value) => new PackedLong(value);
        
        public static implicit operator long(PackedLong value) => value.value;
    }
    
    [Serializable]
    public struct PackedUInt : IEquatable<PackedUInt>
    {
        public uint value;
        
        public PackedUInt(uint value)
        {
            this.value = value;
        }
        
        public static implicit operator PackedUInt(uint value) => new PackedUInt(value);
        
        public static implicit operator uint(PackedUInt value) => value.value;

        public bool Equals(PackedUInt other)
        {
            return value == other.value;
        }

        public override bool Equals(object obj)
        {
            return obj is PackedUInt other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int)value;
        }
    }
    
    [Serializable]
    public struct PackedInt
    {
        public int value;
        
        public PackedInt(int value)
        {
            this.value = value;
        }
        
        public static implicit operator PackedInt(int value) => new PackedInt(value);
        
        public static implicit operator int(PackedInt value) => value.value;
    }

    [Serializable]
    public struct PackedUShort
    {
        public ushort value;
        
        public PackedUShort(ushort value)
        {
            this.value = value;
        }
        
        public static implicit operator PackedUShort(ushort value) => new PackedUShort(value);
        
        public static implicit operator ushort(PackedUShort value) => value.value;
    }
    
    [Serializable]
    public struct PackedShort
    {
        public short value;
        
        public PackedShort(short value)
        {
            this.value = value;
        }
        
        public static implicit operator PackedShort(short value) => new PackedShort(value);
        
        public static implicit operator short(PackedShort value) => value.value;
    }

    public static class PackedUintSerializer
    {
        public static byte ZigzagEncode(sbyte i) => (byte)(((uint)i >> 7) ^ ((uint)i << 1));
        
        public static sbyte ZigzagDecode(byte i) => (sbyte)((i >> 1) ^ -(i & 1));
        
        public static ushort ZigzagEncode (short i) => (ushort)(((ulong)i >> 15) ^ ((ulong)i << 1));
        
        public static short ZigzagDecode (ushort i) => (short)((i >> 1) ^ -(i & 1));
        
        public static uint ZigzagEncode (int i) => (uint)(((ulong)i >> 31) ^ ((ulong)i << 1));
        
        public static ulong ZigzagEncode(long i) => (ulong)((i >> 63) ^ (i << 1));
        
        public static int ZigzagDecode (uint i) => (int)(((long)i >> 1) ^ -((long)i & 1));
        
        public static long ZigzagDecode(ulong i) => ((long)(i >> 1) & 0x7FFFFFFFFFFFFFFFL) ^ ((long)(i << 63) >> 63);
        
        static int CountLeadingZeroBits(uint value)
        {
            if (value == 0) return 32; // Special case for zero

            int count = 0;
            if ((value & 0xFFFF0000) == 0) { count += 16; value <<= 16; }
            if ((value & 0xFF000000) == 0) { count += 8; value <<= 8; }
            if ((value & 0xF0000000) == 0) { count += 4; value <<= 4; }
            if ((value & 0xC0000000) == 0) { count += 2; value <<= 2; }
            if ((value & 0x80000000) == 0) { count += 1; }

            return count;
        }
        
        static int CountLeadingZeroBits(ulong value)
        {
            if (value == 0) return 64; // Special case for zero

            int count = 0;
            if ((value & 0xFFFFFFFF00000000) == 0) { count += 32; value <<= 32; }
            if ((value & 0xFFFF000000000000) == 0) { count += 16; value <<= 16; }
            if ((value & 0xFF00000000000000) == 0) { count += 8; value <<= 8; }
            if ((value & 0xF000000000000000) == 0) { count += 4; value <<= 4; }
            if ((value & 0xC000000000000000) == 0) { count += 2; value <<= 2; }
            if ((value & 0x8000000000000000) == 0) { count += 1; }

            return count;
        }
        
        const int PREFIX_BITS = 4;
        const int TOTAL_BITS = 32;
        const int MAX_COUNT = 1 << PREFIX_BITS;
        const int CHUNK = TOTAL_BITS / MAX_COUNT;
        
        [UsedByIL]
        public static void Write(BitPacker packer, PackedUInt value)
        {
            int trailingZeroes = CountLeadingZeroBits(value.value);
            int emptyChunks = trailingZeroes / CHUNK;
            int fullBytes = Mathf.Clamp(MAX_COUNT - emptyChunks, 1, MAX_COUNT);
            packer.WriteBits((ulong)(fullBytes - 1), PREFIX_BITS);
            byte numberBits = (byte)(fullBytes * CHUNK);
            packer.WriteBits(value.value, numberBits);
        }

        [UsedByIL]
        public static void Read(BitPacker packer, ref PackedUInt value)
        {
            var fullBytes = packer.ReadBits(PREFIX_BITS) + 1;
            int emptyChunks = MAX_COUNT - (int)fullBytes;
            byte numberBits = (byte)(TOTAL_BITS - emptyChunks * CHUNK);
            value = new PackedUInt((uint)packer.ReadBits(numberBits));
        }
        
        [UsedByIL]
        public static void Write(BitPacker packer, PackedInt value)
        {
            var packed = new PackedUInt(ZigzagEncode(value.value));
            Write(packer, packed);
        }

        [UsedByIL]
        public static void Read(BitPacker packer, ref PackedInt value)
        {
            PackedUInt packed = default;
            Read(packer, ref packed);
            value = new PackedInt(ZigzagDecode(packed.value));
        }
        
        const int PREFIX_BITS_2 = 3;
        const int TOTAL_BITS_2 = 16;
        const int MAX_COUNT_2 = 1 << PREFIX_BITS_2;
        const int CHUNK_2 = TOTAL_BITS_2 / MAX_COUNT_2;
        
        [UsedByIL]
        public static void Write(BitPacker packer, PackedUShort value)
        {
            int trailingZeroes = CountLeadingZeroBits(value.value) - TOTAL_BITS_2;
            int emptyChunks = trailingZeroes / CHUNK_2;
            int fullBytes = Mathf.Clamp(MAX_COUNT_2 - emptyChunks, 1, MAX_COUNT_2);
            packer.WriteBits((ulong)(fullBytes - 1), PREFIX_BITS_2);
            byte numberBits = (byte)(fullBytes * CHUNK_2);
            packer.WriteBits(value.value, numberBits);
        }

        [UsedByIL]
        public static void Read(BitPacker packer, ref PackedUShort value)
        {
            var fullBytes = packer.ReadBits(PREFIX_BITS_2) + 1;
            int emptyChunks = MAX_COUNT_2 - (int)fullBytes;
            byte numberBits = (byte)(TOTAL_BITS_2 - emptyChunks * CHUNK_2);
            value = new PackedUShort((ushort)packer.ReadBits(numberBits));
        }
        
        [UsedByIL]
        public static void Write(BitPacker packer, PackedShort value)
        {
            Write(packer, new PackedUShort(ZigzagEncode(value.value)));
        }
        
        [UsedByIL]
        public static void Read(BitPacker packer, ref PackedShort value)
        {
            PackedUShort packed = default;
            Read(packer, ref packed);
            value = new PackedShort(ZigzagDecode(packed.value));
        }
        
        const int PREFIX_BITS_3 = 5;
        const int TOTAL_BITS_3 = 64;
        const int MAX_COUNT_3 = 1 << PREFIX_BITS_3;
        const int CHUNK_3 = TOTAL_BITS_3 / MAX_COUNT_3;
        
        [UsedByIL]
        public static void Write(BitPacker packer, PackedULong value)
        {
            int trailingZeroes = CountLeadingZeroBits(value.value);
            int emptyChunks = trailingZeroes / CHUNK_3;
            int fullBytes = Mathf.Clamp(MAX_COUNT_3 - emptyChunks, 1, MAX_COUNT_3);
            packer.WriteBits((ulong)(fullBytes - 1), PREFIX_BITS_3);
            byte numberBits = (byte)(fullBytes * CHUNK_3);
            packer.WriteBits(value.value, numberBits);
        }

        [UsedByIL]
        public static void Read(BitPacker packer, ref PackedULong value)
        {
            var fullBytes = packer.ReadBits(PREFIX_BITS_3) + 1;
            int emptyChunks = MAX_COUNT_3 - (int)fullBytes;
            byte numberBits = (byte)(TOTAL_BITS_3 - emptyChunks * CHUNK_3);
            value = new PackedULong(packer.ReadBits(numberBits));
        }
        
        [UsedByIL]
        public static void Write(BitPacker packer, PackedLong value)
        {
            Write(packer, new PackedULong(ZigzagEncode(value.value)));
        }

        [UsedByIL]
        public static void Read(BitPacker packer, ref PackedLong value)
        {
            PackedULong packed = default;
            Read(packer, ref packed);
            value = new PackedLong(ZigzagDecode(packed.value));
        }
        
        const int PREFIX_BITS_4 = 3;
        const int TOTAL_BITS_4 = 8;
        const int MAX_COUNT_4 = 1 << PREFIX_BITS_4;
        const int CHUNK_4 = TOTAL_BITS_4 / MAX_COUNT_4;
        
        [UsedByIL]
        public static void Write(BitPacker packer, PackedByte value)
        {
            int trailingZeroes = CountLeadingZeroBits(value.value) - (32 - TOTAL_BITS_4);
            int emptyChunks = trailingZeroes / CHUNK_4;
            int fullBytes = Mathf.Clamp(MAX_COUNT_4 - emptyChunks, 1, MAX_COUNT_4);
            packer.WriteBits((ulong)(fullBytes - 1), PREFIX_BITS_4);
            byte numberBits = (byte)(fullBytes * CHUNK_4);
            packer.WriteBits(value.value, numberBits);
        }

        [UsedByIL]
        public static void Read(BitPacker packer, ref PackedByte value)
        {
            var fullBytes = packer.ReadBits(PREFIX_BITS_4) + 1;
            int emptyChunks = MAX_COUNT_4 - (int)fullBytes;
            byte numberBits = (byte)(TOTAL_BITS_4 - emptyChunks * CHUNK_4);
            value = new PackedByte((byte)packer.ReadBits(numberBits));
        }
        
        [UsedByIL]
        public static void Write(BitPacker packer, PackedSByte value)
        {
            Write(packer, new PackedByte(ZigzagEncode(value.value)));
        }

        [UsedByIL]
        public static void Read(BitPacker packer, ref PackedSByte value)
        {
            PackedByte packed = default;
            Read(packer, ref packed);
            value = new PackedSByte(ZigzagDecode(packed.value));
        }
    }
}
