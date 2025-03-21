using PurrNet.Modules;

namespace PurrNet.Packing
{
    public static class PackUIntegers
    {
        [UsedByIL]
        public static void Write(this BitPacker packer, ulong value)
        {
            packer.WriteBits(value, 64);
        }

        [UsedByIL]
        public static void Read(this BitPacker packer, ref ulong value)
        {
            value = packer.ReadBits(64);
        }

        [UsedByIL]
        public static void Write(this BitPacker packer, uint value)
        {
            packer.WriteBits(value, 32);
        }

        [UsedByIL]
        public static void Read(this BitPacker packer, ref uint value)
        {
            value = (uint)packer.ReadBits(32);
        }

        [UsedByIL]
        public static void Write(this BitPacker packer, ushort value)
        {
            packer.WriteBits(value, 16);
        }

        [UsedByIL]
        public static void Read(this BitPacker packer, ref ushort value)
        {
            value = (ushort)packer.ReadBits(16);
        }

        [UsedByIL]
        public static void Write(this BitPacker packer, byte value)
        {
            packer.WriteBits(value, 8);
        }

        [UsedByIL]
        public static void Read(this BitPacker packer, ref byte value)
        {
            value = (byte)packer.ReadBits(8);
        }
    }
}