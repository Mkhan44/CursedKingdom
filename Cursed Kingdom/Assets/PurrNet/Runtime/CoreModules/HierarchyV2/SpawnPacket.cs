using System;
using PurrNet.Packing;

namespace PurrNet.Modules
{
    public struct SpawnPacket : IPackedAuto, IDisposable
    {
        public SceneID sceneId;
        public SpawnID packetIdx;
        public GameObjectPrototype prototype;
        
        public override string ToString()
        {
            return $"SpawnPacket: {{ sceneId: {sceneId}, packetIdx: {packetIdx}, prototype: {prototype} }}";
        }

        public void Dispose()
        {
            prototype.Dispose();
        }
    }
}