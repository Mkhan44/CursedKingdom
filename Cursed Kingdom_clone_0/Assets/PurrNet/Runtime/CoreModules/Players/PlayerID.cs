using System;
using PurrNet.Packing;

namespace PurrNet
{
    public readonly struct PlayerID : IPackedAuto, IEquatable<PlayerID>
    {
        private ushort _id { get; }

        public bool isBot { get; }
        
        public ushort id => _id;
        
        public static readonly PlayerID Server = new PlayerID(0, false);
        
        public bool isServer => _id == 0;

        public PlayerID(ushort id, bool isBot)
        {
            _id = id;
            this.isBot = isBot;
        }

        public override string ToString()
        {
            return _id == 0 ? "Server" : _id.ToString("000");
        }

        public override int GetHashCode()
        {
            return _id;
        }

        public bool Equals(PlayerID other)
        {
            return _id == other._id;
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerID other && Equals(other);
        }
        
        public static bool operator ==(PlayerID a, PlayerID b)
        {
            return a._id == b._id;
        }

        public static bool operator !=(PlayerID a, PlayerID b)
        {
            return a._id != b._id;
        }
    }
}
