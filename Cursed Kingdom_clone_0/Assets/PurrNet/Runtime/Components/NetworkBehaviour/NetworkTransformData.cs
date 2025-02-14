using System;
using PurrNet.Packing;
using UnityEngine;

namespace PurrNet
{
    public struct NetworkTransformData : IEquatable<NetworkTransformData>
    {
        public readonly ushort id;
        public Vector3 position;
        public Quaternion rotation;
        public HalfVector3 scale;
        
        public NetworkTransformData(ushort id, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            this.id = id;
        }

        public bool Equals(NetworkTransformData other, Tolerances tolerance)
        {
            return Vector3.Distance(position, other.position) < tolerance.positionTolerance &&
                   Quaternion.Angle(rotation, other.rotation) < tolerance.rotationAngleTolerance &&
                   Vector3.Distance(scale, other.scale) < tolerance.scaleTolerance;
        }

        public override bool Equals(object obj)
        {
            return obj is NetworkTransformData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(position, rotation, scale);
        }

        public bool Equals(NetworkTransformData other)
        {
            return position.Equals(other.position) && rotation.Equals(other.rotation) && scale.Equals(other.scale);
        }
    }
}