using System;
using PurrNet.Modules;

namespace PurrNet.Packing
{
    public static class PackFloats
    {
        [UsedByIL]
        public static void Write(this BitPacker packer, float data)
        {
            packer.WriteBits((ulong)BitConverter.SingleToInt32Bits(data), 32);
        }

        [UsedByIL]
        public static void Read(this BitPacker packer, ref float data)
        {
            data = BitConverter.Int32BitsToSingle((int)packer.ReadBits(32));
        }

        [UsedByIL]
        public static void Write(this BitPacker packer, double data)
        {
            packer.WriteBits((ulong)BitConverter.DoubleToInt64Bits(data), 64);
        }

        [UsedByIL]
        public static void Read(this BitPacker packer, ref double data)
        {
            data = BitConverter.Int64BitsToDouble((long)packer.ReadBits(64));
        }
    }
}