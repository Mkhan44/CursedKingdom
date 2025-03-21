using PurrNet.Modules;
using UnityEngine;

namespace PurrNet.Packing
{
    public struct Angle
    {
        public float value;

        public Angle(float value)
        {
            this.value = value;
        }

        public static implicit operator Angle(float value) => new Angle(value);
        public static implicit operator float(Angle angle) => angle.value;
    }

    public static class DeltaPackFloats
    {
        [UsedByIL]
        private static void WriteHalf(BitPacker packer, Half value)
        {
            Packer<ushort>.Write(packer, value.rawValue);
        }

        [UsedByIL]
        private static void ReadHalf(BitPacker packer, ref Half value)
        {
            ushort rawValue = default;
            Packer<ushort>.Read(packer, ref rawValue);
            value = Half.FromRawValue(rawValue);
        }

        [UsedByIL]
        private static void WriteHalf(BitPacker packer, Angle value)
        {
            Packer<float>.Write(packer, value.value);
        }

        [UsedByIL]
        private static void ReadHalf(BitPacker packer, ref Angle value)
        {
            Packer<float>.Read(packer, ref value.value);
        }

        [UsedByIL]
        private static bool WriteHalf(BitPacker packer, Half oldvalue, Half newvalue)
        {
            return DeltaPacker<ushort>.Write(packer, oldvalue.rawValue, newvalue.rawValue);
        }

        [UsedByIL]
        private static void ReadHalf(BitPacker packer, Half oldvalue, ref Half value)
        {
            ushort newValue = default;
            DeltaPacker<ushort>.Read(packer, oldvalue.rawValue, ref newValue);
            value = Half.FromRawValue(newValue);
        }

        [UsedByIL]
        private static unsafe bool WriteDouble(BitPacker packer, double oldvalue, double newvalue)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            bool hasChanged = oldvalue != newvalue;

            Packer<bool>.Write(packer, hasChanged);

            if (hasChanged)
            {
                ulong oldBits = *(ulong*)&oldvalue;
                ulong newBits = *(ulong*)&newvalue;
                long diff = (long)(newBits - oldBits);
                Packer<PackedLong>.Write(packer, diff);
            }

            return hasChanged;
        }

        [UsedByIL]
        private static unsafe void ReadDouble(BitPacker packer, double oldvalue, ref double value)
        {
            bool hasChanged = default;
            Packer<bool>.Read(packer, ref hasChanged);

            if (hasChanged)
            {
                PackedLong packed = default;
                Packer<PackedLong>.Read(packer, ref packed);
                ulong oldBits = *(ulong*)&oldvalue;
                ulong newBits = (ulong)((long)oldBits + packed.value);
                value = *(double*)&newBits;
            }
            else value = oldvalue;
        }

        public const float PRECISION = 0.001f;

        [UsedByIL]
        private static bool WriteSingle(BitPacker packer, float oldvalue, float newvalue)
        {
            float delta = newvalue - oldvalue;

            if (System.Math.Abs(delta) < PRECISION)
            {
                Packer<bool>.Write(packer, false);
                return false;
            }

            Packer<bool>.Write(packer, true);

            var deltaAsInt = Mathf.RoundToInt(delta / PRECISION);
            var estimatedNewValue = oldvalue + deltaAsInt * PRECISION;
            bool isCorrect = Mathf.Abs(newvalue - estimatedNewValue) < PRECISION * 2;

            Packer<bool>.Write(packer, isCorrect);

            if (isCorrect)
            {
                Packer<PackedInt>.Write(packer, deltaAsInt);
            }
            else
            {
                Packer<float>.Write(packer, newvalue);
            }

            return true;
        }

        [UsedByIL]
        private static void ReadSingle(BitPacker packer, float oldvalue, ref float value)
        {
            bool hasChanged = default;
            Packer<bool>.Read(packer, ref hasChanged);

            if (hasChanged)
            {
                bool isCorrect = default;
                Packer<bool>.Read(packer, ref isCorrect);

                if (isCorrect)
                {
                    PackedInt packed = default;
                    Packer<PackedInt>.Read(packer, ref packed);
                    value = oldvalue + packed.value * PRECISION;
                }
                else
                {
                    Packer<float>.Read(packer, ref value);
                }
            }
            else value = oldvalue;
        }

        public const float ANGLE_PRECISION = 0.0005f;

        [UsedByIL]
        private static bool WriteAngle(BitPacker packer, Angle oldvalue, Angle newvalue)
        {
            float delta = newvalue - oldvalue;

            if (System.Math.Abs(delta) < ANGLE_PRECISION)
            {
                Packer<bool>.Write(packer, false);
                return false;
            }

            Packer<bool>.Write(packer, true);

            var deltaAsInt = Mathf.RoundToInt(delta / ANGLE_PRECISION);
            var estimatedNewValue = oldvalue + deltaAsInt * ANGLE_PRECISION;
            bool isCorrect = Mathf.Abs(newvalue - estimatedNewValue) < ANGLE_PRECISION * 2;

            Packer<bool>.Write(packer, isCorrect);

            if (isCorrect)
            {
                Packer<PackedInt>.Write(packer, deltaAsInt);
            }
            else
            {
                Packer<float>.Write(packer, newvalue.value);
            }

            return true;
        }

        [UsedByIL]
        private static void ReadAngle(BitPacker packer, Angle oldvalue, ref Angle value)
        {
            bool hasChanged = default;
            Packer<bool>.Read(packer, ref hasChanged);

            if (hasChanged)
            {
                bool isCorrect = default;
                Packer<bool>.Read(packer, ref isCorrect);

                if (isCorrect)
                {
                    PackedInt packed = default;
                    Packer<PackedInt>.Read(packer, ref packed);
                    value.value = oldvalue + packed.value * ANGLE_PRECISION;
                }
                else
                {
                    Packer<float>.Read(packer, ref value.value);
                }
            }
            else value = oldvalue;
        }

        /*[UsedByIL]
        private static unsafe void WriteSingle(BitPacker packer, float oldvalue, float newvalue)
        {
            bool hasChanged = !Mathf.Approximately(oldvalue, newvalue);
            Packer<bool>.Write(packer, hasChanged);

            if (hasChanged)
            {
                uint oldBits = *(uint*)&oldvalue;
                uint newBits = *(uint*)&newvalue;
                long diff = checked((long)newBits - oldBits);
                Packer<PackedLong>.Write(packer, diff);
            }
        }

        [UsedByIL]
        private static unsafe void ReadSingle(BitPacker packer, float oldvalue, ref float value)
        {
            bool hasChanged = default;
            Packer<bool>.Read(packer, ref hasChanged);

            if (hasChanged)
            {
                PackedLong packed = default;
                Packer<PackedLong>.Read(packer, ref packed);
                uint oldBits = *(uint*)&oldvalue;
                uint newBits = (uint)(oldBits + packed.value);
                value = *(float*)&newBits;
            }
            else value = oldvalue;
        }*/
    }
}
