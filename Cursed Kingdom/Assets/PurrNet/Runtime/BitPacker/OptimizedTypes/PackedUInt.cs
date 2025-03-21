using System;
using PurrNet.Logging;
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

        public static ushort ZigzagEncode(short i) => (ushort)(((ulong)i >> 15) ^ ((ulong)i << 1));

        public static short ZigzagDecode(ushort i) => (short)((i >> 1) ^ -(i & 1));

        public static uint ZigzagEncode(int i) => (uint)(((ulong)i >> 31) ^ ((ulong)i << 1));

        public static ulong ZigzagEncode(long i) => (ulong)((i >> 63) ^ (i << 1));

        public static int ZigzagDecode(uint i) => (int)(((long)i >> 1) ^ -((long)i & 1));

        public static long ZigzagDecode(ulong i) => ((long)(i >> 1) & 0x7FFFFFFFFFFFFFFFL) ^ ((long)(i << 63) >> 63);

        static int CountLeadingZeroBits(ulong value)
        {
            if (value == 0) return 64; // Special case for zero

            int count = 0;
            if ((value & 0xFFFFFFFF00000000) == 0)
            {
                count += 32;
                value <<= 32;
            }

            if ((value & 0xFFFF000000000000) == 0)
            {
                count += 16;
                value <<= 16;
            }

            if ((value & 0xFF00000000000000) == 0)
            {
                count += 8;
                value <<= 8;
            }

            if ((value & 0xF000000000000000) == 0)
            {
                count += 4;
                value <<= 4;
            }

            if ((value & 0xC000000000000000) == 0)
            {
                count += 2;
                value <<= 2;
            }

            if ((value & 0x8000000000000000) == 0)
            {
                count += 1;
            }

            return count;
        }

        [UsedByIL]
        public static void Write(BitPacker packer, PackedUInt value)
        {
            Write(packer, new PackedULong(value.value));
        }

        [UsedByIL]
        public static void Read(BitPacker packer, ref PackedUInt value)
        {
            PackedULong packed = default;
            Read(packer, ref packed);
            value = new PackedUInt((uint)packed.value);
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

        [UsedByIL]
        public static void Write(BitPacker packer, PackedUShort value)
        {
            var packed = new PackedULong(value.value);
            Write(packer, packed);
        }

        [UsedByIL]
        public static void Read(BitPacker packer, ref PackedUShort value)
        {
            PackedULong packed = default;
            Read(packer, ref packed);
            value = new PackedUShort((ushort)packed.value);
        }

        [UsedByIL]
        public static void Write(BitPacker packer, PackedShort value)
        {
            Write(packer, new PackedULong(ZigzagEncode(value.value)));
        }

        [UsedByIL]
        public static void Read(BitPacker packer, ref PackedShort value)
        {
            PackedULong packed = default;
            Read(packer, ref packed);
            value = new PackedShort((short)ZigzagDecode(packed.value));
        }

        private const int SEGMENTS = 16;
        const int TOTAL_BITS = 64;
        const int CHUNK = TOTAL_BITS / SEGMENTS;

        [UsedByIL]
        public static void Write(BitPacker packer, PackedULong value)
        {
            int trailingZeroes = CountLeadingZeroBits(value.value);
            int emptyChunks = trailingZeroes / CHUNK;
            int segmentCount = SEGMENTS - emptyChunks;
            int pointer = 0;

            if (segmentCount == 0)
            {
                packer.WriteBits(0, 1);
                return;
            }

            packer.WriteBits(1, 1);

            const ulong mask = (ulong.MaxValue >> (TOTAL_BITS - CHUNK));
            while (segmentCount > 0 && pointer < TOTAL_BITS)
            {
                ulong isolated = (value.value >> pointer) & mask;
                packer.WriteBits(isolated, CHUNK);
                pointer += CHUNK;

                --segmentCount;
                packer.WriteBits(segmentCount == 0 ? 0u : 1u, 1);
            }
        }

        [UsedByIL]
        public static void Read(BitPacker packer, ref PackedULong value)
        {
            if (packer.ReadBits(1) == 0)
            {
                value.value = 0;
                return;
            }

            ulong result = 0;
            int pointer = 0;
            bool continueReading;

            do
            {
                ulong chunk = packer.ReadBits(CHUNK);
                result |= chunk << pointer;
                pointer += CHUNK;

                continueReading = packer.ReadBits(1) == 1;
            } while (continueReading && pointer < TOTAL_BITS);

            value.value = result;
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

        [UsedByIL]
        public static void Write(BitPacker packer, PackedByte value)
        {
            Write(packer, new PackedULong(value.value));
        }

        [UsedByIL]
        public static void Read(BitPacker packer, ref PackedByte value)
        {
            PackedULong packed = default;
            Read(packer, ref packed);
            value = new PackedByte((byte)packed.value);
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
