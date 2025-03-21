using PurrNet.Packing;

namespace PurrNet.Modules
{
    public struct FinishSpawnPacket : IPackedAuto
    {
        public SceneID sceneId;
        public SpawnID packetIdx;

        public override string ToString()
        {
            return $"FinishSpawnPacket: {{ sceneId: {sceneId}, packetIdx: {packetIdx} }}";
        }
    }
}