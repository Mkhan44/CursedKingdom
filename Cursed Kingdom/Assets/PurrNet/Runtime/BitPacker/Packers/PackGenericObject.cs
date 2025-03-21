using PurrNet.Modules;
using PurrNet.Packing;
using PurrNet.Utils;

namespace PurrNet
{
    public static class PackGenericObject
    {
        [UsedByIL]
        public static void WriteObject(this BitPacker packer, object value)
        {
            bool isNull = value == null;

            packer.Write(isNull);

            if (isNull)
                return;

            var hash = Hasher.GetStableHashU32(value.GetType());

            packer.Write(hash);

            Packer.Write(packer, value);
        }

        [UsedByIL]
        public static void ReadObject(this BitPacker packer, ref object value)
        {
            bool isNull = false;

            packer.Read(ref isNull);

            if (isNull)
            {
                value = null;
                return;
            }

            uint hash = 0;

            packer.Read(ref hash);

            if (!Hasher.TryGetType(hash, out var type))
                return;

            Packer.Read(packer, type, ref value);
        }
    }
}