using JetBrains.Annotations;
using K4os.Compression.LZ4;
using PurrNet.Modules;
using PurrNet.Transports;

namespace PurrNet
{
    public struct RPCInfo
    {
        public NetworkManager manager;
        public PlayerID sender;
        public bool asServer;

        [UsedByIL] public RPCSignature compileTimeSignature;
    }

    public enum RPCType
    {
        ServerRPC,
        ObserversRPC,
        TargetRPC
    }

    public struct RPCSignature
    {
        public RPCType type;
        public Channel channel;
        public bool isStatic;
        public bool runLocally;
        public bool requireOwnership;
        public bool bufferLast;
        public bool requireServer;
        public bool excludeOwner;
        public bool excludeSender;
        public string rpcName;
        public float asyncTimeoutInSec;
        public CompressionLevel compressionLevel;
        public PlayerID? targetPlayer;

        [UsedImplicitly]
        public static RPCSignature Make(RPCType type, Channel channel, bool runLocally, bool requireOwnership,
            bool bufferLast, bool requireServer, bool excludeOwner, string name, bool isStatic, float asyncTimoutInSec,
            CompressionLevel compressionLevel, bool excludeSender)
        {
            return new RPCSignature
            {
                type = type,
                channel = channel,
                runLocally = runLocally,
                requireOwnership = requireOwnership,
                bufferLast = bufferLast,
                requireServer = requireServer,
                excludeOwner = excludeOwner,
                excludeSender = excludeSender,
                targetPlayer = null,
                isStatic = isStatic,
                rpcName = name,
                asyncTimeoutInSec = asyncTimoutInSec,
                compressionLevel = compressionLevel
            };
        }

        [UsedImplicitly]
        public static RPCSignature MakeWithTarget(RPCType type, Channel channel, bool runLocally, bool requireOwnership,
            bool bufferLast, bool requireServer, bool excludeOwner, string name, bool isStatic, float asyncTimoutInSec,
            CompressionLevel compressionLevel, bool excludeSender, PlayerID playerID)
        {
            var rpc = Make(type, channel, runLocally, requireOwnership, bufferLast, requireServer, excludeOwner, name,
                isStatic, asyncTimoutInSec, compressionLevel, excludeSender);
            rpc.targetPlayer = playerID;
            return rpc;
        }
    }
}