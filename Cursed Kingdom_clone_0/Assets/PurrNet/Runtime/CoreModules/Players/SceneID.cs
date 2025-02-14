using System;

namespace PurrNet
{
    public readonly struct SceneID : IEquatable<SceneID>
    {
        private ushort _id { get; }

        public ushort id => _id;

        public SceneID(ushort id)
        {
            _id = id;
        }

        public override string ToString()
        {
            return _id.ToString("000");
        }

        public override int GetHashCode()
        {
            return _id;
        }

        public bool Equals(SceneID other)
        {
            return _id == other._id;
        }

        public override bool Equals(object obj)
        {
            return obj is SceneID other && Equals(other);
        }
        
        public static bool operator ==(SceneID a, SceneID b)
        {
            return a._id == b._id;
        }

        public static bool operator !=(SceneID a, SceneID b)
        {
            return a._id != b._id;
        }
    }
}