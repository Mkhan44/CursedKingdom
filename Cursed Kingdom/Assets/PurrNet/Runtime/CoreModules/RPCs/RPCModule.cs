using System;
using System.Collections.Generic;
using System.Reflection;
using K4os.Compression.LZ4;
using PurrNet.Logging;
using PurrNet.Packing;
using PurrNet.Transports;
using PurrNet.Utils;
using UnityEngine;

namespace PurrNet.Modules
{
    public class RPCModule : INetworkModule
    {
        readonly HierarchyFactory _hierarchyModule;
        readonly PlayersManager _playersManager;
        readonly ScenesModule _scenes;
        readonly GlobalOwnershipModule _ownership;
        readonly NetworkManager _manager;

        public RPCModule(NetworkManager manager, PlayersManager playersManager, HierarchyFactory hierarchyModule,
            GlobalOwnershipModule ownerships, ScenesModule scenes)
        {
            _manager = manager;
            _playersManager = playersManager;
            _hierarchyModule = hierarchyModule;
            _scenes = scenes;
            _ownership = ownerships;
        }

        public void Enable(bool asServer)
        {
            _playersManager.Subscribe<RPCPacket>(ReceiveRPC);
            _playersManager.Subscribe<StaticRPCPacket>(ReceiveStaticRPC);
            _playersManager.Subscribe<ChildRPCPacket>(ReceiveChildRPC);

            _playersManager.onPlayerJoined += OnPlayerJoined;
            _scenes.onSceneUnloaded += OnSceneUnloaded;

            _hierarchyModule.onEarlyObserverAdded += OnObserverAdded;
            _hierarchyModule.onIdentityRemoved += OnIdentityRemoved;
        }

        public void Disable(bool asServer)
        {
            _playersManager.Unsubscribe<RPCPacket>(ReceiveRPC);
            _playersManager.Unsubscribe<StaticRPCPacket>(ReceiveStaticRPC);
            _playersManager.Unsubscribe<ChildRPCPacket>(ReceiveChildRPC);

            _playersManager.onPlayerJoined -= OnPlayerJoined;
            _scenes.onSceneUnloaded -= OnSceneUnloaded;

            _hierarchyModule.onEarlyObserverAdded -= OnObserverAdded;
            _hierarchyModule.onIdentityRemoved -= OnIdentityRemoved;
        }

        private void OnObserverAdded(PlayerID player, NetworkIdentity identity)
        {
            SendAnyInstanceRPCs(player, identity);
            SendAnyChildRPCs(player, identity);
        }

        // Clean up buffered RPCs when an identity is removed
        private void OnIdentityRemoved(NetworkIdentity identity)
        {
            for (int i = 0; i < _bufferedRpcsDatas.Count; i++)
            {
                var data = _bufferedRpcsDatas[i];

                if (data.rpcid.sceneId != identity.sceneId) continue;
                if (data.rpcid.networkId != identity.id) continue;

                FreeStream(data.stream);

                _bufferedRpcsKeys.Remove(data.rpcid);
                _bufferedRpcsDatas.RemoveAt(i--);
            }

            for (int i = 0; i < _bufferedChildRpcsDatas.Count; i++)
            {
                var data = _bufferedChildRpcsDatas[i];

                if (data.rpcid.sceneId != identity.sceneId) continue;
                if (data.rpcid.networkId != identity.id) continue;

                FreeStream(data.stream);

                _bufferedChildRpcsKeys.Remove(data.rpcid);
                _bufferedChildRpcsDatas.RemoveAt(i--);
            }
        }

        // Clean up buffered RPCs when a scene is unloaded
        private void OnSceneUnloaded(SceneID scene, bool asServer)
        {
            for (int i = 0; i < _bufferedRpcsDatas.Count; i++)
            {
                var data = _bufferedRpcsDatas[i];

                if (data.rpcid.sceneId != scene) continue;

                var key = data.rpcid;
                FreeStream(data.stream);

                _bufferedRpcsKeys.Remove(key);
                _bufferedRpcsDatas.RemoveAt(i--);
            }

            for (int i = 0; i < _bufferedChildRpcsDatas.Count; i++)
            {
                var data = _bufferedChildRpcsDatas[i];

                if (data.rpcid.sceneId != scene) continue;

                var key = data.rpcid;
                FreeStream(data.stream);

                _bufferedChildRpcsKeys.Remove(key);
                _bufferedChildRpcsDatas.RemoveAt(i--);
            }
        }

        private void OnPlayerJoined(PlayerID player, bool isReconnect, bool asServer)
        {
            SendAnyStaticRPCs(player);
        }

        [UsedByIL]
        public static PlayerID GetLocalPlayer()
        {
            var nm = NetworkManager.main;

            if (!nm) return default;

            if (!nm.TryGetModule<PlayersManager>(false, out var players))
                return default;

            return players.localPlayerId ?? default;
        }

        public static PlayerID GetLocalPlayer(NetworkManager nm)
        {
            if (!nm) return default;

            if (!nm.TryGetModule<PlayersManager>(false, out var players))
                return default;

            return players.localPlayerId ?? default;
        }

        [UsedByIL]
        public static void SendStaticRPC(StaticRPCPacket packet, RPCSignature signature)
        {
            var nm = NetworkManager.main;

            if (!nm)
            {
                PurrLogger.LogError($"Can't send static RPC '{signature.rpcName}'. NetworkManager not found.");
                return;
            }

            if (!nm.TryGetModule<RPCModule>(nm.isServer, out var module))
            {
                PurrLogger.LogError("Failed to get RPC module while sending static RPC.", nm);
                return;
            }

            var rules = nm.networkRules;
            bool shouldIgnore = rules && rules.ShouldIgnoreRequireServer();

            if (!shouldIgnore && signature.requireServer && !nm.isServer)
            {
                PurrLogger.LogError(
                    $"Trying to send static RPC '{signature.rpcName}' of type {signature.type} without server.");
                return;
            }

            module.AppendToBufferedRPCs(packet, signature);

            switch (signature.type)
            {
                case RPCType.ServerRPC:
                {
                    if (nm.TryGetModule<PlayersManager>(false, out var players))
                        players.SendToServer(packet, signature.channel);
                    break;
                }
                case RPCType.ObserversRPC:
                {
                    if (nm.isServer)
                    {
                        var players = nm.GetModule<PlayersManager>(true);
                        _observers.Clear();
                        _observers.AddRange(players.players);

                        if (signature.excludeSender && nm.isClient)
                            _observers.Remove(GetLocalPlayer(nm));

                        players.Send(_observers, packet, signature.channel);
                    }
                    else nm.GetModule<PlayersManager>(false).SendToServer(packet, signature.channel);

                    break;
                }
                case RPCType.TargetRPC:
                {
                    if (nm.isServer)
                        nm.GetModule<PlayersManager>(true)
                            .Send(signature.targetPlayer!.Value, packet, signature.channel);
                    else nm.GetModule<PlayersManager>(false).SendToServer(packet, signature.channel);
                    break;
                }
                default: throw new ArgumentOutOfRangeException();
            }
        }

        static readonly List<PlayerID> _observers = new List<PlayerID>();

        static IEnumerable<PlayerID> GetImmediateExcept(PlayersManager players, PlayerID except)
        {
            _observers.Clear();
            _observers.AddRange(players.players);
            _observers.Remove(except);
            return _observers;
        }

        [UsedByIL]
        public static bool ValidateReceivingStaticRPC(RPCInfo info, RPCSignature signature, IRpc data, bool asServer)
        {
            var nm = NetworkManager.main;

            if (!nm)
            {
                PurrLogger.LogError($"Aborted RPC '{signature.rpcName}'. NetworkManager not found.");
                return false;
            }

            if (!asServer)
            {
                if (signature.type == RPCType.ServerRPC)
                {
                    PurrLogger.LogError(
                        $"Aborted RPC {signature.type} '{signature.rpcName}' on client. ServerRpc are meant for server only.");
                    return false;
                }

                return true;
            }

            var rules = nm.networkRules;
            bool shouldIgnore = rules && rules.ShouldIgnoreRequireServer();

            if (!shouldIgnore && signature.requireServer)
            {
                PurrLogger.LogError(
                    $"Aborted RPC {signature.type} '{signature.rpcName}' which requires server from client.");
                return false;
            }

            switch (signature.type)
            {
                case RPCType.ServerRPC: return true;
                case RPCType.ObserversRPC:
                {
                    var players = nm.GetModule<PlayersManager>(true);
                    var rawData = BroadcastModule.GetImmediateData(data);
                    var collection = signature.excludeSender
                        ? GetImmediateExcept(players, info.sender)
                        : players.players;
                    players.SendRaw(collection, rawData, signature.channel);
                    return !nm.isClient;
                }
                case RPCType.TargetRPC:
                {
                    var players = nm.GetModule<PlayersManager>(true);
                    var rawData = BroadcastModule.GetImmediateData(data);
                    players.SendRaw(data.senderPlayerId, rawData, signature.channel);
                    return false;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        readonly struct StaticGenericKey : IEquatable<StaticGenericKey>
        {
            readonly IntPtr _type;
            readonly string _methodName;
            readonly int _typesHash;

            public StaticGenericKey(IntPtr type, string methodName, Type[] types)
            {
                _type = type;
                _methodName = methodName;

                _typesHash = 0;

                for (int i = 0; i < types.Length; i++)
                    _typesHash ^= types[i].GetHashCode();
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(_type, _methodName, _typesHash);
            }

            public bool Equals(StaticGenericKey other)
            {
                return _type.Equals(other._type) && _methodName == other._methodName && _typesHash == other._typesHash;
            }

            public override bool Equals(object obj)
            {
                return obj is StaticGenericKey other && Equals(other);
            }
        }

        static readonly Dictionary<StaticGenericKey, MethodInfo> _staticGenericHandlers =
            new Dictionary<StaticGenericKey, MethodInfo>();

        [UsedByIL]
        public static object CallStaticGeneric(RuntimeTypeHandle type, string methodName, GenericRPCHeader rpcHeader)
        {
            var targetType = Type.GetTypeFromHandle(type);
            var key = new StaticGenericKey(type.Value, methodName, rpcHeader.types);

            if (!_staticGenericHandlers.TryGetValue(key, out var gmethod))
            {
                var method = targetType.GetMethod(methodName,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                gmethod = method?.MakeGenericMethod(rpcHeader.types);

                _staticGenericHandlers[key] = gmethod;
            }

            if (gmethod == null)
            {
                PurrLogger.LogError($"Calling generic static RPC failed. Method '{methodName}' not found.");
                return null;
            }

            return gmethod.Invoke(null, rpcHeader.values);
        }

        private void SendAnyChildRPCs(PlayerID player, NetworkIdentity identity)
        {
            for (int i = 0; i < _bufferedChildRpcsDatas.Count; i++)
            {
                var data = _bufferedChildRpcsDatas[i];

                if (data.rpcid.sceneId != identity.sceneId)
                    continue;

                if (data.rpcid.networkId != identity.id)
                    continue;

                if (data.sig.excludeOwner && _ownership.TryGetOwner(identity, out var owner) && owner == player)
                    continue;

                switch (data.sig.type)
                {
                    case RPCType.ObserversRPC:
                    {
                        var packet = data.packet;
                        packet.data = data.stream.ToByteData();
                        _playersManager.Send(player, packet);

                        break;
                    }

                    case RPCType.TargetRPC:
                    {
                        if (data.sig.targetPlayer == player)
                        {
                            var packet = data.packet;
                            packet.data = data.stream.ToByteData();
                            _playersManager.Send(player, packet);
                        }

                        break;
                    }
                    case RPCType.ServerRPC:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void SendAnyInstanceRPCs(PlayerID player, NetworkIdentity identity)
        {
            for (int i = 0; i < _bufferedRpcsDatas.Count; i++)
            {
                var data = _bufferedRpcsDatas[i];

                if (data.rpcid.sceneId != identity.sceneId)
                    continue;

                if (data.rpcid.networkId != identity.id)
                    continue;

                if (data.sig.excludeOwner && _ownership.TryGetOwner(identity, out var owner) && owner == player)
                    continue;

                switch (data.sig.type)
                {
                    case RPCType.ObserversRPC:
                    {
                        var packet = data.packet;
                        packet.data = data.stream.ToByteData();
                        _playersManager.Send(player, packet);

                        break;
                    }

                    case RPCType.TargetRPC:
                    {
                        if (data.sig.targetPlayer == player)
                        {
                            var packet = data.packet;
                            packet.data = data.stream.ToByteData();
                            _playersManager.Send(player, packet);
                        }

                        break;
                    }
                }
            }
        }

        private void SendAnyStaticRPCs(PlayerID player)
        {
            for (int i = 0; i < _bufferedStaticRpcsDatas.Count; i++)
            {
                var data = _bufferedStaticRpcsDatas[i];

                switch (data.sig.type)
                {
                    case RPCType.ObserversRPC:
                    {
                        var packet = data.packet;
                        packet.data = data.stream.ToByteData();
                        _playersManager.Send(player, packet);

                        break;
                    }

                    case RPCType.TargetRPC:
                    {
                        if (data.sig.targetPlayer == player)
                        {
                            var packet = data.packet;
                            packet.data = data.stream.ToByteData();
                            _playersManager.Send(player, packet);
                        }

                        break;
                    }
                }
            }
        }

        [UsedByIL]
        public static BitPacker AllocStream(bool reading)
        {
            return BitPackerPool.Get(reading);
        }

        [UsedByIL]
        public static void FreeStream(BitPacker stream)
        {
            stream.Dispose();
        }

        readonly Dictionary<RPC_ID, RPC_DATA> _bufferedRpcsKeys = new Dictionary<RPC_ID, RPC_DATA>();

        readonly Dictionary<RPC_ID, STATIC_RPC_DATA>
            _bufferedStaticRpcsKeys = new Dictionary<RPC_ID, STATIC_RPC_DATA>();

        readonly Dictionary<RPC_ID, CHILD_RPC_DATA> _bufferedChildRpcsKeys = new Dictionary<RPC_ID, CHILD_RPC_DATA>();

        readonly List<RPC_DATA> _bufferedRpcsDatas = new List<RPC_DATA>();
        readonly List<STATIC_RPC_DATA> _bufferedStaticRpcsDatas = new List<STATIC_RPC_DATA>();
        readonly List<CHILD_RPC_DATA> _bufferedChildRpcsDatas = new List<CHILD_RPC_DATA>();

        private void AppendToBufferedRPCs(StaticRPCPacket packet, RPCSignature signature)
        {
            if (!signature.bufferLast) return;

            var rpcid = new RPC_ID(packet);

            if (_bufferedStaticRpcsKeys.TryGetValue(rpcid, out var data))
            {
                data.stream.ResetPosition();
                data.stream.WriteBytes(packet.data);
            }
            else
            {
                var newStream = AllocStream(false);
                newStream.WriteBytes(packet.data);

                var newdata = new STATIC_RPC_DATA
                {
                    rpcid = rpcid,
                    packet = packet,
                    sig = signature,
                    stream = newStream
                };

                _bufferedStaticRpcsKeys.Add(rpcid, newdata);
                _bufferedStaticRpcsDatas.Add(newdata);
            }
        }

        public void AppendToBufferedRPCs(ChildRPCPacket packet, RPCSignature signature)
        {
            if (!signature.bufferLast) return;

            var rpcid = new RPC_ID(packet);

            if (_bufferedChildRpcsKeys.TryGetValue(rpcid, out var data))
            {
                data.stream.ResetPosition();
                data.stream.WriteBytes(packet.data);
            }
            else
            {
                var newStream = AllocStream(false);
                newStream.WriteBytes(packet.data);

                var newdata = new CHILD_RPC_DATA
                {
                    rpcid = rpcid,
                    packet = packet,
                    sig = signature,
                    stream = newStream
                };

                _bufferedChildRpcsKeys.Add(rpcid, newdata);
                _bufferedChildRpcsDatas.Add(newdata);
            }
        }

        public void AppendToBufferedRPCs(RPCPacket packet, RPCSignature signature)
        {
            if (!signature.bufferLast) return;

            var rpcid = new RPC_ID(packet);

            if (_bufferedRpcsKeys.TryGetValue(rpcid, out var data))
            {
                data.stream.ResetPosition();
                data.stream.WriteBytes(packet.data);
            }
            else
            {
                var newStream = AllocStream(false);
                newStream.WriteBytes(packet.data);

                var newdata = new RPC_DATA
                {
                    rpcid = rpcid,
                    packet = packet,
                    sig = signature,
                    stream = newStream
                };

                _bufferedRpcsKeys.Add(rpcid, newdata);
                _bufferedRpcsDatas.Add(newdata);
            }
        }

        [UsedByIL]
        public static RPCPacket BuildRawRPC(NetworkID? networkId, SceneID id, byte rpcId, BitPacker data)
        {
            var rpc = new RPCPacket
            {
                networkId = networkId ?? default,
                rpcId = rpcId,
                sceneId = id,
                data = data.ToByteData(),
                senderId = GetLocalPlayer()
            };

            return rpc;
        }

        [UsedByIL]
        public static StaticRPCPacket BuildStaticRawRPC<T>(byte rpcId, BitPacker data)
        {
            var hash = Hasher.GetStableHashU32<T>();

            var rpc = new StaticRPCPacket
            {
                rpcId = rpcId,
                data = data.ToByteData(),
                typeHash = hash,
                senderId = GetLocalPlayer()
            };

            return rpc;
        }

        readonly struct RPCKey : IEquatable<RPCKey>
        {
            private readonly IReflect type;
            private readonly byte rpcId;

            public override int GetHashCode()
            {
                return type.GetHashCode() ^ rpcId.GetHashCode();
            }

            public RPCKey(IReflect type, byte rpcId)
            {
                this.type = type;
                this.rpcId = rpcId;
            }

            public bool Equals(RPCKey other)
            {
                return Equals(type, other.type) && rpcId == other.rpcId;
            }

            public override bool Equals(object obj)
            {
                return obj is RPCKey other && Equals(other);
            }
        }

        static readonly Dictionary<RPCKey, IntPtr> _rpcHandlers = new Dictionary<RPCKey, IntPtr>();

        static IntPtr GetRPCHandler(IReflect type, byte rpcId)
        {
            var rpcKey = new RPCKey(type, rpcId);

            if (_rpcHandlers.TryGetValue(rpcKey, out var handler))
                return handler;

            string methodName = $"HandleRPCGenerated_{rpcId}";
            var method = type.GetMethod(methodName,
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            var ptr = method != null ? method.MethodHandle.GetFunctionPointer() : IntPtr.Zero;

            if (ptr != IntPtr.Zero)
                _rpcHandlers[rpcKey] = ptr;

            return ptr;
        }

        unsafe void ReceiveStaticRPC(PlayerID player, StaticRPCPacket data, bool asServer)
        {
            if (!Hasher.TryGetType(data.typeHash, out var type))
            {
                PurrLogger.LogError($"Failed to resolve type with hash {data.typeHash}.");
                return;
            }

            using var stream = BitPackerPool.Get(data.data);

            var rpcHandlerPtr = GetRPCHandler(type, data.rpcId);
            var info = new RPCInfo
            {
                manager = _manager,
                sender = player,
                asServer = asServer
            };

            if (rpcHandlerPtr != IntPtr.Zero)
            {
                try
                {
                    // Call the RPC handler
                    ((delegate* managed<BitPacker, StaticRPCPacket, RPCInfo, bool, void>)
                        rpcHandlerPtr)(stream, data, info, asServer);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            else PurrLogger.LogError($"Can't find RPC handler for id {data.rpcId} on '{type.Name}'.");
        }

        unsafe void ReceiveChildRPC(PlayerID player, ChildRPCPacket packet, bool asServer)
        {
            using var stream = BitPackerPool.Get(packet.data);

            var info = new RPCInfo
            {
                manager = _manager,
                sender = player,
                asServer = asServer
            };

            if (_hierarchyModule.TryGetIdentity(packet.sceneId, packet.networkId, out var identity) && identity)
            {
                if (!identity.enabled && !identity.ShouldPlayRPCsWhenDisabled())
                    return;

                if (!identity.TryGetModule(packet.childId, out var networkClass))
                {
                    PurrLogger.LogError(
                        $"Can't find child with id {packet.childId} in identity {identity.GetType().Name}.", identity);
                }
                else
                {
                    var rpcHandlerPtr = GetRPCHandler(networkClass.GetType(), packet.rpcId);

                    if (rpcHandlerPtr != IntPtr.Zero)
                    {
                        try
                        {
                            // Call the RPC handler
                            ((delegate* managed<NetworkModule, BitPacker, ChildRPCPacket, RPCInfo, bool, void>)
                                rpcHandlerPtr)(networkClass, stream, packet, info, asServer);
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }
                    }
                    else
                        PurrLogger.LogError(
                            $"Can't find RPC handler for id {packet.rpcId} in identity {identity.GetType().Name}.");
                }
            }
        }

        unsafe void ReceiveRPC(PlayerID player, RPCPacket packet, bool asServer)
        {
            using var stream = BitPackerPool.Get(packet.data);

            var info = new RPCInfo
            {
                manager = _manager,
                sender = packet.senderId,
                asServer = asServer
            };

            if (_hierarchyModule.TryGetIdentity(packet.sceneId, packet.networkId, out var identity) && identity)
            {
                if (!identity.enabled && !identity.ShouldPlayRPCsWhenDisabled())
                {
                    return;
                }

                var rpcHandlerPtr = GetRPCHandler(identity.GetType(), packet.rpcId);
                if (rpcHandlerPtr != IntPtr.Zero)
                {
                    try
                    {
                        // Call the RPC handler
                        ((delegate* managed<NetworkIdentity, BitPacker, RPCPacket, RPCInfo, bool, void>)
                            rpcHandlerPtr)(identity, stream, packet, info, asServer);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(
                            new Exception(
                                $"Failed to call RPC handler for id {packet.rpcId} on identity {identity.GetType().Name}.",
                                e), identity);
                    }
                }
                else
                    PurrLogger.LogError(
                        $"Can't find RPC handler for id {packet.rpcId} in identity {identity.GetType().Name}.");
            }
            // else PurrLogger.LogError($"Can't find identity with id {packet.networkId} in scene {packet.sceneId}.");
        }

        static void Test()
        {
            var packer = BitPackerPool.Get();
            var packet = new RPCPacket();
            PreProcessRpc(ref packet.data, default, ref packer);
        }

        [UsedByIL]
        public static void PreProcessRpc(ref ByteData rpcData, RPCSignature signature, ref BitPacker packer)
        {
            if (signature.compressionLevel == CompressionLevel.None)
                return;

            var level = signature.compressionLevel switch
            {
                CompressionLevel.None => LZ4Level.L00_FAST,
                CompressionLevel.Fast => LZ4Level.L00_FAST,
                CompressionLevel.Balanced => LZ4Level.L06_HC,
                CompressionLevel.Best => LZ4Level.L12_MAX,
                _ => throw new ArgumentOutOfRangeException()
            };

            var newPacker = packer.Pickle(level);
            rpcData = newPacker.ToByteData();

            packer.Dispose();
            packer = newPacker;
        }

        [UsedByIL]
        public static void PostProcessRpc(ByteData rpcData, RPCInfo info, ref BitPacker packer)
        {
            if (info.compileTimeSignature.compressionLevel == CompressionLevel.None)
                return;

            var newPacker = BitPackerPool.Get();
            newPacker.UnpickleFrom(rpcData);
            newPacker.ResetPositionAndMode(true);

            packer.Dispose();
            packer = newPacker;
        }
    }
}