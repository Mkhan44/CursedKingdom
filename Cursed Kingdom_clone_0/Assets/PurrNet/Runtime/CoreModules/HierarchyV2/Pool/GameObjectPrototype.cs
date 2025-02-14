using System;
using System.Text;
using PurrNet.Pooling;
using UnityEngine;

namespace PurrNet.Modules
{
    public struct GameObjectPrototype : IDisposable
    {
        public Vector3 position;
        public Quaternion rotation;
        public NetworkID? parentID;
        public readonly int[] path;
        public readonly int? defaultParentSiblingIndex;
        public DisposableList<GameObjectFrameworkPiece> framework;
        
        public GameObjectPrototype(
            Vector3 position, 
            Quaternion rotation,
            NetworkID? parentID, 
            int[] path,
            DisposableList<GameObjectFrameworkPiece> framework,
            int? defaultParentSiblingIndex)
        {
            this.position = position;
            this.rotation = rotation;
            this.framework = framework;
            this.parentID = parentID;
            this.path = path;
            this.defaultParentSiblingIndex = defaultParentSiblingIndex;
        }

        public void Dispose()
        {
            framework.Dispose();
        }

        public override string ToString()
        {
            StringBuilder builder = new();
            builder.Append($"GameObjectPrototype: {{\n    ");
            for (int i = 0; i < framework.Count; i++)
            {
                builder.Append(framework[i]);
                if (i < framework.Count - 1)
                    builder.Append("\n    ");
            }
            builder.Append("\n}");
            return builder.ToString();
        }
    }
}