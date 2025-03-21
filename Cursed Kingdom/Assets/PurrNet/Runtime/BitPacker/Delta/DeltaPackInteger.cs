using PurrNet.Modules;

namespace PurrNet.Packing
{
    public static class DeltaPackInteger
    {
        [UsedByIL]
        private static bool WriteBool(BitPacker packer, bool oldvalue, bool newvalue)
        {
            bool hasChanged = oldvalue != newvalue;
            Packer<bool>.Write(packer, hasChanged);
            return hasChanged;
        }

        [UsedByIL]
        private static void ReadBool(BitPacker packer, bool oldvalue, ref bool value)
        {
            bool hasChanged = default;
            Packer<bool>.Read(packer, ref hasChanged);
            value = hasChanged ? !oldvalue : oldvalue;
        }

        [UsedByIL]
        private static bool WriteInt8(BitPacker packer, sbyte oldvalue, sbyte newvalue)
        {
            bool hasChanged = oldvalue != newvalue;
            Packer<bool>.Write(packer, hasChanged);

            if (hasChanged)
            {
                short diff = (short)(newvalue - oldvalue);
                Packer<PackedShort>.Write(packer, diff);
            }

            return hasChanged;
        }

        [UsedByIL]
        private static void ReadInt8(BitPacker packer, sbyte oldvalue, ref sbyte value)
        {
            bool hasChanged = default;
            Packer<bool>.Read(packer, ref hasChanged);

            if (hasChanged)
            {
                PackedShort packed = default;
                Packer<PackedShort>.Read(packer, ref packed);
                value = (sbyte)(oldvalue + packed.value);
            }
            else value = oldvalue;
        }

        [UsedByIL]
        private static bool WriteUInt8(BitPacker packer, byte oldvalue, byte newvalue)
        {
            bool hasChanged = oldvalue != newvalue;
            Packer<bool>.Write(packer, hasChanged);

            if (hasChanged)
            {
                short diff = (short)(newvalue - oldvalue);
                Packer<PackedShort>.Write(packer, diff);
            }

            return hasChanged;
        }

        [UsedByIL]
        private static void ReadUInt8(BitPacker packer, byte oldvalue, ref byte value)
        {
            bool hasChanged = default;
            Packer<bool>.Read(packer, ref hasChanged);

            if (hasChanged)
            {
                PackedShort packed = default;
                Packer<PackedShort>.Read(packer, ref packed);
                value = (byte)(oldvalue + packed.value);
            }
            else value = oldvalue;
        }

        [UsedByIL]
        private static bool WriteInt16(BitPacker packer, short oldvalue, short newvalue)
        {
            bool hasChanged = oldvalue != newvalue;
            Packer<bool>.Write(packer, hasChanged);

            if (hasChanged)
            {
                int diff = newvalue - oldvalue;
                Packer<PackedInt>.Write(packer, diff);
            }

            return hasChanged;
        }

        [UsedByIL]
        private static void ReadInt16(BitPacker packer, short oldvalue, ref short value)
        {
            bool hasChanged = default;
            Packer<bool>.Read(packer, ref hasChanged);

            if (hasChanged)
            {
                PackedInt packed = default;
                Packer<PackedInt>.Read(packer, ref packed);
                value = (short)(oldvalue + packed.value);
            }
            else value = oldvalue;
        }

        [UsedByIL]
        private static bool WriteUInt16(BitPacker packer, ushort oldvalue, ushort newvalue)
        {
            bool hasChanged = oldvalue != newvalue;
            Packer<bool>.Write(packer, hasChanged);

            if (hasChanged)
            {
                int diff = (int)((uint)newvalue - oldvalue);
                Packer<PackedInt>.Write(packer, diff);
            }

            return hasChanged;
        }

        [UsedByIL]
        private static void ReadUInt16(BitPacker packer, ushort oldvalue, ref ushort value)
        {
            bool hasChanged = default;
            Packer<bool>.Read(packer, ref hasChanged);

            if (hasChanged)
            {
                PackedInt packed = default;
                Packer<PackedInt>.Read(packer, ref packed);
                value = (ushort)(oldvalue + packed.value);
            }
            else value = oldvalue;
        }

        [UsedByIL]
        private static bool WriteUInt32(BitPacker packer, uint oldvalue, uint newvalue)
        {
            bool hasChanged = oldvalue != newvalue;
            Packer<bool>.Write(packer, hasChanged);

            if (hasChanged)
            {
                long diff = newvalue - (long)oldvalue;
                Packer<PackedLong>.Write(packer, diff);
            }

            return hasChanged;
        }

        [UsedByIL]
        private static void ReadUInt32(BitPacker packer, uint oldvalue, ref uint value)
        {
            bool hasChanged = default;
            Packer<bool>.Read(packer, ref hasChanged);

            if (hasChanged)
            {
                PackedLong packed = default;
                Packer<PackedLong>.Read(packer, ref packed);
                value = (uint)(oldvalue + packed.value);
            }
            else value = oldvalue;
        }

        [UsedByIL]
        private static bool WriteUInt32(BitPacker packer, PackedUInt oldvalue, PackedUInt newvalue)
        {
            bool hasChanged = oldvalue != newvalue;
            Packer<bool>.Write(packer, hasChanged);

            if (hasChanged)
            {
                long diff = newvalue - (long)oldvalue;
                Packer<PackedLong>.Write(packer, diff);
            }

            return hasChanged;
        }

        [UsedByIL]
        private static void ReadUInt32(BitPacker packer, PackedUInt oldvalue, ref PackedUInt value)
        {
            bool hasChanged = default;
            Packer<bool>.Read(packer, ref hasChanged);

            if (hasChanged)
            {
                PackedLong packed = default;
                Packer<PackedLong>.Read(packer, ref packed);
                value = (uint)(oldvalue + packed.value);
            }
            else value = oldvalue;
        }

        [UsedByIL]
        private static bool WriteInt32(BitPacker packer, int oldvalue, int newvalue)
        {
            bool hasChanged = oldvalue != newvalue;
            Packer<bool>.Write(packer, hasChanged);

            if (hasChanged)
            {
                long diff = newvalue - (long)oldvalue;
                Packer<PackedLong>.Write(packer, diff);
            }

            return hasChanged;
        }

        [UsedByIL]
        private static void ReadInt32(BitPacker packer, int oldvalue, ref int value)
        {
            bool hasChanged = default;
            Packer<bool>.Read(packer, ref hasChanged);

            if (hasChanged)
            {
                PackedLong packed = default;
                Packer<PackedLong>.Read(packer, ref packed);
                value = (int)(oldvalue + packed.value);
            }
            else value = oldvalue;
        }
    }
}