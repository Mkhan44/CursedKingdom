using System;

namespace PurrNet.Modules
{
    public readonly struct SpawnID : IEquatable<SpawnID>
    {
        readonly int packetIdx;
        public readonly PlayerID player;

        public SpawnID(int packetIdx, PlayerID player)
        {
            this.packetIdx = packetIdx;
            this.player = player;
        }

        public bool Equals(SpawnID other)
        {
            return packetIdx == other.packetIdx && player.Equals(other.player);
        }

        public override bool Equals(object obj)
        {
            return obj is SpawnID other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(packetIdx, player);
        }
    }
}