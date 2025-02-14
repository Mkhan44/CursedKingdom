using PurrNet.Modules;
using UnityEngine;

namespace PurrNet.Packing
{
    public static class PackUnityAnimatorTypesExtension
    {
        [UsedByIL]
        public static void Write(this BitPacker packer, AvatarIKHint value) 
            => packer.WriteBits((ulong)value, 2);

        [UsedByIL]
        public static void Read(this BitPacker packer, ref AvatarIKHint value) 
            => value = (AvatarIKHint)packer.ReadBits(2);

        [UsedByIL]
        public static void Write(this BitPacker packer, AvatarIKGoal value) 
            => packer.WriteBits((ulong)value, 2);

        [UsedByIL]
        public static void Read(this BitPacker packer, ref AvatarIKGoal value) 
            => value = (AvatarIKGoal)packer.ReadBits(2);
        
        [UsedByIL]
        public static void Write(this BitPacker packer, HumanBodyBones value) 
            => packer.WriteBits((ulong)value, 6);

        [UsedByIL]
        public static void Read(this BitPacker packer, ref HumanBodyBones value) 
            => value = (HumanBodyBones)packer.ReadBits(6);
        
        [UsedByIL]
        public static void Write(this BitPacker packer, AnimatorCullingMode value) 
            => packer.WriteBits((ulong)value, 2);

        [UsedByIL]
        public static void Read(this BitPacker packer, ref AnimatorCullingMode value) 
            => value = (AnimatorCullingMode)packer.ReadBits(2);
        
        [UsedByIL]
        public static void Write(this BitPacker packer, AnimatorUpdateMode value) 
            => packer.WriteBits((ulong)value, 2);

        [UsedByIL]
        public static void Read(this BitPacker packer, ref AnimatorUpdateMode value) 
            => value = (AnimatorUpdateMode)packer.ReadBits(2);
        
        [UsedByIL]
        public static void Write(this BitPacker packer, AvatarTarget value) 
            => packer.WriteBits((ulong)value, 3);

        [UsedByIL]
        public static void Read(this BitPacker packer, ref AvatarTarget value) 
            => value = (AvatarTarget)packer.ReadBits(3);
        
        [UsedByIL]
        public static void Write(this BitPacker packer, MatchTargetWeightMask value)
        {
            Packer<Half>.Write(packer, new Half(value.rotationWeight));
            Packer<HalfVector3>.Write(packer, value.positionXYZWeight);
        }

        [UsedByIL]
        public static void Read(this BitPacker packer, ref MatchTargetWeightMask value)
        {
            Half rotationWeight = default;
            Packer<Half>.Read(packer, ref rotationWeight);
            
            HalfVector3 positionXYZWeight = default;
            Packer<HalfVector3>.Read(packer, ref positionXYZWeight);
            
            value = new MatchTargetWeightMask(positionXYZWeight, rotationWeight);
        }
    }
}
