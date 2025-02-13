﻿using System;

namespace PurrNet.Modules
{
    public readonly struct PrefabPieceID : IEquatable<PrefabPieceID>
    {
        public readonly int prefabId;
        public readonly int componentIndex;

        public PrefabPieceID(int prefabId, int componentIndex)
        {
            this.prefabId = prefabId;
            this.componentIndex = componentIndex;
        }

        public override string ToString()
        {
            return $"PrefabPieceID: {{ prefabId: {prefabId}, componentIndex: {componentIndex} }}";
        }

        public bool Equals(PrefabPieceID other)
        {
            return prefabId == other.prefabId && componentIndex == other.componentIndex;
        }

        public override bool Equals(object obj)
        {
            return obj is PrefabPieceID other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(prefabId, componentIndex);
        }
    }
}