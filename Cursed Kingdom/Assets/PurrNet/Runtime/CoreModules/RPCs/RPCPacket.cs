using System;
using System.Net.Sockets;
using PurrNet.Packing;
using PurrNet.Transports;

namespace PurrNet
{
    public interface IRpc
    {
        PlayerID senderPlayerId { get; }
    }
    
    public struct RPCPacket : IPackedAuto, IRpc
    {
        public NetworkID networkId;
        public SceneID sceneId;
        public PlayerID senderId;
        public byte rpcId;
        public ByteData data;
        
        public PlayerID senderPlayerId => senderId;
    }
    
    public struct ChildRPCPacket : IPackedAuto, IRpc
    {
        public NetworkID networkId;
        public SceneID sceneId;
        public PlayerID senderId;
        public byte rpcId;
        public byte childId;
        public ByteData data;
        
        public PlayerID senderPlayerId => senderId;
    }
    
    public struct StaticRPCPacket : IPackedAuto, IRpc
    {
        public uint typeHash;
        public byte rpcId;
        public PlayerID senderId;
        public ByteData data;
        
        public PlayerID senderPlayerId => senderId;
    }
    
    internal readonly struct RPC_ID : IEquatable<RPC_ID>
    {
        public readonly uint typeHash;
        public readonly SceneID sceneId;
        public readonly NetworkID networkId;
        public readonly PlayerID senderId;
        private readonly byte rpcId;
        private readonly byte childId;

        public RPC_ID(RPCPacket packet)
        {
            sceneId = packet.sceneId;
            networkId = packet.networkId;
            rpcId = packet.rpcId;
            senderId = packet.senderId;
            typeHash = default;
            childId = default;
        }

        public RPC_ID(StaticRPCPacket packet)
        {
            sceneId = default;
            networkId = default;
            rpcId = packet.rpcId;
            typeHash = packet.typeHash;
            senderId = packet.senderId;
            childId = default;
        }

        public RPC_ID(ChildRPCPacket packet)
        {
            sceneId = packet.sceneId;
            networkId = packet.networkId;
            rpcId = packet.rpcId;
            typeHash = default;
            childId = packet.childId;
            senderId = packet.senderId;
        }

        public override int GetHashCode()
        {
            return sceneId.GetHashCode() ^ 
                   networkId.GetHashCode() ^ 
                   rpcId.GetHashCode() ^ 
                   typeHash.GetHashCode() ^ 
                   childId.GetHashCode();
        }

        public bool Equals(RPC_ID other)
        {
            return typeHash == other.typeHash && 
                   sceneId.Equals(other.sceneId) && 
                   networkId.Equals(other.networkId) && 
                   senderId.Equals(other.senderId) && 
                   rpcId == other.rpcId && 
                   childId == other.childId;
        }

        public override bool Equals(object obj)
        {
            return obj is RPC_ID other && Equals(other);
        }
    }

    internal class RPC_DATA
    {
        public RPC_ID rpcid;
        public RPCPacket packet;
        public RPCSignature sig;
        public BitPacker stream;
    }

    internal class CHILD_RPC_DATA
    {
        public RPC_ID rpcid;
        public ChildRPCPacket packet;
        public RPCSignature sig;
        public BitPacker stream;
    }

    internal class STATIC_RPC_DATA
    {
        public RPC_ID rpcid;
        public StaticRPCPacket packet;
        public RPCSignature sig;
        public BitPacker stream;
    }
}