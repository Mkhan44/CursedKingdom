using PurrNet.Modules;
using UnityEngine;

namespace PurrNet.Packing
{
    public static class DeltaPackVectors
    {
        [UsedByIL]
        private static void WriteVector2(BitPacker packer, Vector2 oldvalue, Vector2 newvalue)
        {
            bool hasChanged = oldvalue != newvalue;
            Packer<bool>.Write(packer, hasChanged);

            if (hasChanged)
            {
                DeltaPacker<float>.Write(packer, oldvalue.x, newvalue.x);
                DeltaPacker<float>.Write(packer, oldvalue.y, newvalue.y);
            }
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
        }
        
        [UsedByIL]
        private static void WriteVector3(BitPacker packer, Vector3 oldvalue, Vector3 newvalue)
        {
            bool hasChanged = oldvalue != newvalue;
            Packer<bool>.Write(packer, hasChanged);

            if (hasChanged)
            {
                DeltaPacker<float>.Write(packer, oldvalue.x, newvalue.x);
                DeltaPacker<float>.Write(packer, oldvalue.y, newvalue.y);
                DeltaPacker<float>.Write(packer, oldvalue.z, newvalue.z);
            }
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
        }
        
        [UsedByIL]
        private static void WriteVector4(BitPacker packer, Vector4 oldvalue, Vector4 newvalue)
        {
            bool hasChanged = oldvalue != newvalue;
            Packer<bool>.Write(packer, hasChanged);

            if (hasChanged)
            {
                DeltaPacker<float>.Write(packer, oldvalue.x, newvalue.x);
                DeltaPacker<float>.Write(packer, oldvalue.y, newvalue.y);
                DeltaPacker<float>.Write(packer, oldvalue.z, newvalue.z);
                DeltaPacker<float>.Write(packer, oldvalue.w, newvalue.w);
            }
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
        }
        
        [UsedByIL]
        private static void WriteQuaternion(BitPacker packer, Quaternion oldvalue, Quaternion newvalue)
        {
            bool hasChanged = oldvalue != newvalue;
            Packer<bool>.Write(packer, hasChanged);

            if (hasChanged)
            {
                DeltaPacker<float>.Write(packer, oldvalue.x, newvalue.x);
                DeltaPacker<float>.Write(packer, oldvalue.y, newvalue.y);
                DeltaPacker<float>.Write(packer, oldvalue.z, newvalue.z);
                DeltaPacker<float>.Write(packer, oldvalue.w, newvalue.w);
            }
        }
        
        [UsedByIL]
        private static void ReadQuaternion(BitPacker packer, Quaternion oldvalue, ref Quaternion value)
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
        }
    }
}
