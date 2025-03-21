using PurrNet.Modules;
using UnityEngine;

namespace PurrNet.Packing
{
    public static class DeltaPackVectors
    {
        [UsedByIL]
        private static bool WriteVector2(BitPacker packer, Vector2 oldvalue, Vector2 newvalue)
        {
            int flagPos = packer.AdvanceBits(1);
            bool wasChanged;

            wasChanged = DeltaPacker<float>.Write(packer, oldvalue.x, newvalue.x);
            wasChanged = DeltaPacker<float>.Write(packer, oldvalue.y, newvalue.y) || wasChanged;

            packer.WriteAt(flagPos, wasChanged);

            if (!wasChanged)
                packer.SetBitPosition(flagPos + 1);

            return wasChanged;
        }

        [UsedByIL]
        private static void ReadVector2(BitPacker packer, Vector2 oldvalue, ref Vector2 value)
        {
            bool hasChanged = default;
            Packer<bool>.Read(packer, ref hasChanged);

            if (hasChanged)
            {
                DeltaPacker<float>.Read(packer, oldvalue.x, ref value.x);
                DeltaPacker<float>.Read(packer, oldvalue.y, ref value.y);
            }
            else value = oldvalue;
        }

        [UsedByIL]
        private static bool WriteVector3(BitPacker packer, Vector3 oldvalue, Vector3 newvalue)
        {
            int flagPos = packer.AdvanceBits(1);
            bool hasChanged;

            hasChanged = DeltaPacker<float>.Write(packer, oldvalue.x, newvalue.x);
            hasChanged = DeltaPacker<float>.Write(packer, oldvalue.y, newvalue.y) || hasChanged;
            hasChanged = DeltaPacker<float>.Write(packer, oldvalue.z, newvalue.z) || hasChanged;

            packer.WriteAt(flagPos, hasChanged);

            if (!hasChanged)
                packer.SetBitPosition(flagPos + 1);
            return hasChanged;
        }

        [UsedByIL]
        private static void ReadVector3(BitPacker packer, Vector3 oldvalue, ref Vector3 value)
        {
            bool hasChanged = default;
            Packer<bool>.Read(packer, ref hasChanged);

            if (hasChanged)
            {
                DeltaPacker<float>.Read(packer, oldvalue.x, ref value.x);
                DeltaPacker<float>.Read(packer, oldvalue.y, ref value.y);
                DeltaPacker<float>.Read(packer, oldvalue.z, ref value.z);
            }
            else value = oldvalue;
        }

        [UsedByIL]
        private static bool WriteVector4(BitPacker packer, Vector4 oldvalue, Vector4 newvalue)
        {
            int flagPos = packer.AdvanceBits(1);
            bool isEqual;

            isEqual = DeltaPacker<float>.Write(packer, oldvalue.x, newvalue.x);
            isEqual = DeltaPacker<float>.Write(packer, oldvalue.y, newvalue.y) || isEqual;
            isEqual = DeltaPacker<float>.Write(packer, oldvalue.z, newvalue.z) || isEqual;
            isEqual = DeltaPacker<float>.Write(packer, oldvalue.w, newvalue.w) || isEqual;

            packer.WriteAt(flagPos, isEqual);
            if (!isEqual)
                packer.SetBitPosition(flagPos + 1);
            return isEqual;
        }

        [UsedByIL]
        private static void ReadVector4(BitPacker packer, Vector4 oldvalue, ref Vector4 value)
        {
            bool hasChanged = default;
            Packer<bool>.Read(packer, ref hasChanged);

            if (hasChanged)
            {
                DeltaPacker<float>.Read(packer, oldvalue.x, ref value.x);
                DeltaPacker<float>.Read(packer, oldvalue.y, ref value.y);
                DeltaPacker<float>.Read(packer, oldvalue.z, ref value.z);
                DeltaPacker<float>.Read(packer, oldvalue.w, ref value.w);
            }
            else value = oldvalue;
        }

        [UsedByIL]
        private static bool WriteQuaternion(BitPacker packer, Quaternion oldvalue, Quaternion newvalue)
        {
            newvalue.Normalize();

            int flagPos = packer.AdvanceBits(1);
            bool isEqual;

            isEqual = DeltaPacker<Angle>.Write(packer, oldvalue.x, newvalue.x);
            isEqual = DeltaPacker<Angle>.Write(packer, oldvalue.y, newvalue.y) || isEqual;
            isEqual = DeltaPacker<Angle>.Write(packer, oldvalue.z, newvalue.z) || isEqual;
            isEqual = DeltaPacker<bool>.Write(packer, oldvalue.w < 0, newvalue.w < 0) || isEqual;

            packer.WriteAt(flagPos, isEqual);
            if (!isEqual)
                packer.SetBitPosition(flagPos + 1);
            return isEqual;
        }

        [UsedByIL]
        private static void ReadQuaternion(BitPacker packer, Quaternion oldvalue, ref Quaternion value)
        {
            bool hasChanged = default;
            Packer<bool>.Read(packer, ref hasChanged);

            if (hasChanged)
            {
                Angle x = default, y = default, z = default;

                DeltaPacker<Angle>.Read(packer, oldvalue.x, ref x);
                DeltaPacker<Angle>.Read(packer, oldvalue.y, ref y);
                DeltaPacker<Angle>.Read(packer, oldvalue.z, ref z);


                bool oldW = oldvalue.w < 0;
                DeltaPacker<bool>.Read(packer, oldW, ref oldW);

                var w = Mathf.Sqrt(Mathf.Max(0, 1 - x * x - y * y - z * z));
                if (oldW) w = -w;

                value = new Quaternion(x, y, z, w);
            }
            else value = oldvalue;
        }

        [UsedByIL]
        public static void Write(this BitPacker packer, HalfQuaternion value)
        {
            value.Normalize();

            Packer<Half>.Write(packer, value.x);
            Packer<Half>.Write(packer, value.y);
            Packer<Half>.Write(packer, value.z);

            packer.Write(value.w < 0);
        }

        [UsedByIL]
        public static void Read(this BitPacker packer, ref HalfQuaternion value)
        {
            Half x = default;
            Half y = default;
            Half z = default;

            Packer<Half>.Read(packer, ref x);
            Packer<Half>.Read(packer, ref y);
            Packer<Half>.Read(packer, ref z);

            bool wSign = false;
            packer.Read(ref wSign);

            var w = (Half)Mathf.Sqrt(Mathf.Max(0, 1 - x * x - y * y - z * z));

            if (wSign)
                w = -w;

            value = new HalfQuaternion(x, y, z, w);
        }

        [UsedByIL]
        private static bool WriteQuaternion(BitPacker packer, HalfQuaternion oldvalue, HalfQuaternion newvalue)
        {
            var flagPos = packer.AdvanceBits(1);
            bool hasChanged;

            hasChanged = DeltaPacker<Half>.Write(packer, oldvalue.x, newvalue.x);
            hasChanged = DeltaPacker<Half>.Write(packer, oldvalue.y, newvalue.y) || hasChanged;
            hasChanged = DeltaPacker<Half>.Write(packer, oldvalue.z, newvalue.z) || hasChanged;
            hasChanged = DeltaPacker<Half>.Write(packer, oldvalue.w, newvalue.w) || hasChanged;

            packer.WriteAt(flagPos, hasChanged);
            if (!hasChanged)
                packer.SetBitPosition(flagPos + 1);
            return hasChanged;
        }

        [UsedByIL]
        private static void ReadQuaternion(BitPacker packer, HalfQuaternion oldvalue, ref HalfQuaternion value)
        {
            bool hasChanged = default;
            Packer<bool>.Read(packer, ref hasChanged);

            if (hasChanged)
            {
                DeltaPacker<Half>.Read(packer, oldvalue.x, ref value.x);
                DeltaPacker<Half>.Read(packer, oldvalue.y, ref value.y);
                DeltaPacker<Half>.Read(packer, oldvalue.z, ref value.z);
                DeltaPacker<Half>.Read(packer, oldvalue.w, ref value.w);
            }
            else value = oldvalue;
        }
    }
}
