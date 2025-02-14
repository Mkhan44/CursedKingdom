using System;
using UnityEngine;

namespace PurrNet.Packing
{
    public struct HalfVector3 : IEquatable<HalfVector3>
    {
        public Half x;
        public Half y;
        public Half z;
        
        public static implicit operator Vector3(HalfVector3 value)
        {
            return new Vector3(value.x, value.y, value.z);
        }
        
        public static implicit operator HalfVector3(Vector3 value)
        {
            return new HalfVector3
            {
                x = new Half(value.x),
                y = new Half(value.y),
                z = new Half(value.z)
            };
        }

        public bool Equals(HalfVector3 other)
        {
            return x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z);
        }

        public override bool Equals(object obj)
        {
            return obj is HalfVector3 other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y, z);
        }
    }
}