using System;
using PurrNet.Pooling;
using UnityEngine;

namespace PurrNet.Modules
{
    public struct GameObjectRuntimePair : IDisposable
    {
        public readonly Transform parent;
        public readonly NetworkIdentity identity;
        public DisposableList<TransformIdentityPair> children;

        public GameObjectRuntimePair(Transform parent, NetworkIdentity identity,
            DisposableList<TransformIdentityPair> children)
        {
            this.parent = parent;
            this.identity = identity;
            this.children = children;
        }

        public void Dispose()
        {
            children.Dispose();
        }
    }
}