using PurrNet.Modules;
using PurrNet.Transports;

namespace PurrNet.Packing
{
    public static class PackByteData
    {
        [UsedByIL]
        public static void Write(this BitPacker packer, ByteData data)
        {
            Packer<PackedInt>.Write(packer, data.length);
            packer.WriteBytes(data.span);
        }

        [UsedByIL]
        public static void Read(this BitPacker packer, ref ByteData data)
        {
            // TODO: use BitData instead of ByteData and map BitData directly to BitPacker underlaying buffer

            PackedInt length = default;
            Packer<PackedInt>.Read(packer, ref length);

            byte[] buffer = new byte[length];
            packer.ReadBytes(buffer);
            data = new ByteData(buffer, 0, length);
        }

        [UsedByIL]
        public static void Write(this BitPacker packer, BitPacker data)
        {
            Write(packer, data.ToByteData());
        }

        [UsedByIL]
        public static void Read(this BitPacker packer, ref BitPacker data)
        {
            PackedInt length = default;
            Packer<PackedInt>.Read(packer, ref length);

            data = BitPackerPool.Get();
            packer.ReadBytes(data, length);
            data.ResetPosition();
        }

        [UsedByIL]
        public static void Write(this BitPacker packer, BitPackerWithLength data)
        {
            Write(packer, data.packer.ToByteData());
        }

        [UsedByIL]
        public static void Read(this BitPacker packer, ref BitPackerWithLength data)
        {
            PackedInt length = default;
            Packer<PackedInt>.Read(packer, ref length);

            var dataPacker = BitPackerPool.Get();
            packer.ReadBytes(dataPacker, length);
            dataPacker.ResetPosition();

            data = new BitPackerWithLength(length, dataPacker);
        }
    }
}