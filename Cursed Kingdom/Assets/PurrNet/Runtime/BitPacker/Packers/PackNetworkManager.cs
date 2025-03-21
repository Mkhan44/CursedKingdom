using PurrNet.Modules;
using PurrNet.Packing;

namespace PurrNet
{
    [UsedByIL]
    public static class PackNetworkManager
    {
        [UsedByIL]
        public static void WriteNetworkManager(this BitPacker packer, NetworkManager manager)
        {
        }

        [UsedByIL]
        public static void ReadNetworkManager(this BitPacker packer, ref NetworkManager manager)
        {
            manager = NetworkManager.main;
        }
    }
}