using UnityEngine;

namespace PurrNet.Packing
{
    public struct HalfVector4
    {
        public Half x;
        public Half y;
        public Half z;
        public Half w;
        
        public static implicit operator Vector4(HalfVector4 value)
        {
            return new Vector4(value.x, value.y, value.z, value.w);
        }
        
        public static implicit operator HalfVector4(Vector4 value)
        {
            return new HalfVector4
            {
                x = new Half(value.x),
                y = new Half(value.y),
                z = new Half(value.z),
                w = new Half(value.w)
            };
        }
    }
}