using PurrNet.Modules;

namespace PurrNet.Packing
{
    public static class PackNetworkID
    {
        [UsedByIL]
        public static void Write(BitPacker packer, SceneID value)
        {
            Packer<PackedUShort>.Write(packer, new PackedUShort(value.id));
        }

        [UsedByIL]
        public static void Read(BitPacker packer, ref SceneID value)
        {
            PackedUShort id = default;
            Packer<PackedUShort>.Read(packer, ref id);
            value = new SceneID(id);
        }
        
        [UsedByIL]
        public static void Write(BitPacker packer, NetworkID value)
        {
            Packer<PackedUInt>.Write(packer, value.id);
            Packer<PlayerID>.Write(packer, value.scope);
        }

        [UsedByIL]
        public static void Read(BitPacker packer, ref NetworkID value)
        {
            PackedUInt id = default;
            PlayerID scope = default;
            
            Packer<PackedUInt>.Read(packer, ref id);
            Packer<PlayerID>.Read(packer, ref scope);
            
            value = new NetworkID(id, scope);
        }
        
        [UsedByIL]
        public static void Write(BitPacker packer, PlayerID value)
        {
            Packer<PackedUShort>.Write(packer, new PackedUShort(value.id));
            Packer<bool>.Write(packer, value.isBot);
        }

        [UsedByIL]
        public static void Read(BitPacker packer, ref PlayerID value)
        {
            PackedUShort id = default;
            bool isBot = default;
            
            Packer<PackedUShort>.Read(packer, ref id);
            Packer<bool>.Read(packer, ref isBot);
            
            value = new PlayerID(id, isBot);
        }
    }
}
