using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using PurrNet.Logging;
using PurrNet.Modules;
using PurrNet.Packing;
using PurrNet.Transports;
using PurrNet.Utils;
using UnityEngine.Scripting;
using Channel = PurrNet.Transports.Channel;

namespace PurrNet
{
    public partial class NetworkIdentity
    {
        internal readonly struct InstanceGenericKey : IEquatable<InstanceGenericKey>
        {
            readonly string _methodName;
            readonly int _typesHash;
            readonly int _callerHash;

            public InstanceGenericKey(string methodName, Type caller, Type[] types)
            {
                _methodName = methodName;
                _typesHash = 0;

                _callerHash = caller.GetHashCode();

                for (int i = 0; i < types.Length; i++)
                    _typesHash ^= types[i].GetHashCode();
            }

            public override int GetHashCode() => _methodName.GetHashCode() ^ _typesHash ^ _callerHash;

            public bool Equals(InstanceGenericKey other)
            {
                return _methodName == other._methodName && _typesHash == other._typesHash &&
                       _callerHash == other._callerHash;
            }

            public override bool Equals(object obj)
            {
                return obj is InstanceGenericKey other && Equals(other);
            }
        }

        internal static readonly Dictionary<InstanceGenericKey, MethodInfo> genericMethods =
            new Dictionary<InstanceGenericKey, MethodInfo>();

        [UsedByIL]
        public static void ReadGenericHeader(BitPacker stream, RPCInfo info, int genericCount, int paramCount,
            out GenericRPCHeader rpcHeader)
        {
            uint hash = 0;

            rpcHeader = new GenericRPCHeader
            {
                stream = stream,
                types = new Type[genericCount],
                values = new object[paramCount],
                info = info
            };

            for (int i = 0; i < genericCount; i++)
            {
                Packer<uint>.Read(stream, ref hash);
                var type = Hasher.ResolveType(hash);

                rpcHeader.types[i] = type;
            }
        }

        [UsedByIL]
        protected object CallGeneric(string methodName, GenericRPCHeader rpcHeader)
        {
            var key = new InstanceGenericKey(methodName, GetType(), rpcHeader.types);

            if (!genericMethods.TryGetValue(key, out var gmethod))
            {
                var method = GetType().GetMethod(methodName,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                gmethod = method?.MakeGenericMethod(rpcHeader.types);

                genericMethods.Add(key, gmethod);
            }

            if (gmethod == null)
            {
                PurrLogger.LogError($"Calling generic RPC failed. Method '{methodName}' not found.");
                return null;
            }

            return gmethod.Invoke(this, rpcHeader.values);
        }

        /// <summary>
        /// Used internally to get next RPC id.
        /// Do not use this method directly.
        /// </summary>
        [UsedByIL]
        public Task<T> GetNextId<T>(RPCType rpcType, PlayerID? target, float timeout, out RpcRequest request)
        {
            request = default;

            if (!networkManager)
            {
                return Task.FromException<T>(new InvalidOperationException(
                    "NetworkIdentity is not spawned."));
            }

            bool asServer = rpcType switch
            {
                RPCType.ServerRPC => !networkManager.isClient,
                RPCType.TargetRPC => networkManager.isServer,
                RPCType.ObserversRPC => networkManager.isServer,
                _ => throw new ArgumentOutOfRangeException(nameof(rpcType), rpcType, null)
            };

            if (!networkManager.TryGetModule<RpcRequestResponseModule>(asServer, out var module))
            {
                return Task.FromException<T>(new InvalidOperationException(
                    "RpcRequestResponseModule module is missing."));
            }

            return module.GetNextId<T>(target, timeout, out request);
        }

        [UsedByIL]
        public UniTask GetNextIdUniTask(RPCType rpcType, PlayerID? target, float timeout, out RpcRequest request)
        {
            request = default;

            if (!networkManager)
            {
                return UniTask.FromException(new InvalidOperationException(
                    "NetworkIdentity is not spawned."));
            }

            bool asServer = rpcType switch
            {
                RPCType.ServerRPC => !networkManager.isClient,
                RPCType.TargetRPC => networkManager.isServer,
                RPCType.ObserversRPC => networkManager.isServer,
                _ => throw new ArgumentOutOfRangeException(nameof(rpcType), rpcType, null)
            };

            if (!networkManager.TryGetModule<RpcRequestResponseModule>(asServer, out var module))
            {
                return UniTask.FromException(new InvalidOperationException(
                    "RpcRequestResponseModule module is missing."));
            }

            return module.GetNextIdUniTask(target, timeout, out request);
        }

        [UsedByIL]
        public UniTask<T> GetNextIdUniTask<T>(RPCType rpcType, PlayerID? target, float timeout, out RpcRequest request)
        {
            request = default;

            if (!networkManager)
            {
                return UniTask.FromException<T>(new InvalidOperationException(
                    "NetworkIdentity is not spawned."));
            }

            bool asServer = rpcType switch
            {
                RPCType.ServerRPC => !networkManager.isClient,
                RPCType.TargetRPC => networkManager.isServer,
                RPCType.ObserversRPC => networkManager.isServer,
                _ => throw new ArgumentOutOfRangeException(nameof(rpcType), rpcType, null)
            };

            if (!networkManager.TryGetModule<RpcRequestResponseModule>(asServer, out var module))
            {
                return UniTask.FromException<T>(new InvalidOperationException(
                    "RpcRequestResponseModule module is missing."));
            }

            return module.GetNextIdUniTask<T>(target, timeout, out request);
        }

        /// <summary>
        /// Used internally to get next RPC id.
        /// Do not use this method directly.
        /// </summary>
        [UsedByIL]
        public Task GetNextId(RPCType rpcType, PlayerID? target, float timeout, out RpcRequest request)
        {
            request = default;

            if (!networkManager)
            {
                return Task.FromException(new InvalidOperationException(
                    "NetworkIdentity is not spawned."));
            }

            bool asServer = rpcType switch
            {
                RPCType.ServerRPC => !networkManager.isClient,
                RPCType.TargetRPC => networkManager.isServer,
                RPCType.ObserversRPC => networkManager.isServer,
                _ => throw new ArgumentOutOfRangeException(nameof(rpcType), rpcType, null)
            };

            if (!networkManager.TryGetModule<RpcRequestResponseModule>(asServer, out var module))
            {
                return Task.FromException(new InvalidOperationException(
                    "RpcRequestResponseModule module is missing."));
            }

            return module.GetNextId(target, timeout, out request);
        }

        [UsedByIL]
        protected void SendRPC(RPCPacket packet, RPCSignature signature)
        {
            if (!isSpawned)
            {
                if (!signature.runLocally)
                    PurrLogger.LogError($"Trying to send RPC from '{GetType().Name}' which is not spawned.", this);
                return;
            }

            if (!networkManager.TryGetModule<RPCModule>(networkManager.isServer, out var module))
            {
                if (!signature.runLocally)
                    PurrLogger.LogError("Failed to get RPC module.", this);
                return;
            }

            var rules = networkManager.networkRules;
            bool shouldIgnoreOwnership = rules && rules.ShouldIgnoreRequireOwner();

            if (!shouldIgnoreOwnership && signature.requireOwnership && !isOwner)
            {
                if (!signature.runLocally)
                    PurrLogger.LogError(
                        $"Trying to send RPC '{signature.rpcName}' from '{GetType().Name}' without ownership.", this);
                return;
            }

            bool shouldIgnore = rules && rules.ShouldIgnoreRequireServer();

            if (!shouldIgnore && signature.requireServer && !networkManager.isServer)
            {
                if (!signature.runLocally)
                    PurrLogger.LogError(
                        $"Trying to send RPC '{signature.rpcName}' from '{GetType().Name}' without server.", this);
                return;
            }

            module.AppendToBufferedRPCs(packet, signature);

            switch (signature.type)
            {
                case RPCType.ServerRPC:
                    if (networkManager.isServerOnly)
                        break;

                    if (signature.runLocally && isServer)
                        break;

                    SendToServer(packet, signature.channel);
                    break;
                case RPCType.ObserversRPC:
                {
                    if (isServer)
                        SendToObservers(packet, ShouldSend, signature.channel);
                    else SendToServer(packet, signature.channel);
                    break;
                }
                case RPCType.TargetRPC:
                    if (isServer)
                        SendToTarget(signature.targetPlayer!.Value, packet, signature.channel);
                    else SendToServer(packet, signature.channel);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }

            return;

            bool ShouldSend(PlayerID player)
            {
                bool isLocalPlayer = player == networkManager.localPlayer;

                if (signature.runLocally && isLocalPlayer)
                    return false;

                if (signature.excludeSender && isLocalPlayer)
                    return false;

                return !signature.excludeOwner || IsNotOwnerPredicate(player);
            }
        }

        [UsedByIL]
        public bool ValidateReceivingRPC(RPCInfo info, RPCSignature signature, IRpc data, bool asServer)
        {
            var rules = networkManager.networkRules;
            bool shouldIgnoreOwnership = rules && rules.ShouldIgnoreRequireOwner();

            if (!shouldIgnoreOwnership && signature.requireOwnership && info.sender != owner)
                return false;

            if (signature.excludeOwner && isOwner)
                return false;

            if (signature.type == RPCType.ServerRPC)
            {
                if (!asServer)
                {
                    PurrLogger.LogError(
                        $"Trying to receive server RPC '{signature.rpcName}' from '{name}' on client. Aborting RPC call.",
                        this);
                    return false;
                }

                var idObservers = observers;

                if (idObservers == null)
                {
                    PurrLogger.LogError(
                        $"Trying to receive server RPC '{signature.rpcName}' from '{name}' but failed to get observers.",
                        this);
                    return false;
                }

                if (!idObservers.Contains(info.sender) && signature.channel == Channel.ReliableOrdered)
                {
                    PurrLogger.LogError(
                        $"Trying to receive server RPC '{signature.rpcName}' from '{name}' by player '{info.sender}' which is not an observer. Aborting RPC call.",
                        this);
                    return false;
                }

                return true;
            }

            if (!asServer)
            {
                return true;
            }

            bool shouldIgnore = rules && rules.ShouldIgnoreRequireServer();

            if (!shouldIgnore && signature.requireServer)
            {
                PurrLogger.LogError(
                    $"Trying to receive client RPC '{signature.rpcName}' from '{name}' on server. " +
                    "If you want automatic forwarding use 'requireServer: false'.", this);
                return false;
            }

            Func<PlayerID, bool> predicate = ShouldSend;

            switch (signature.type)
            {
                case RPCType.ServerRPC: throw new InvalidOperationException("ServerRPC should be handled by server.");

                case RPCType.ObserversRPC:
                {
                    var rawData = BroadcastModule.GetImmediateData(data);
                    SendToObservers(rawData, predicate, signature.channel);
                    return !isClient;
                }
                case RPCType.TargetRPC:
                {
                    var rawData = BroadcastModule.GetImmediateData(data);
                    SendToTarget(data.senderPlayerId, rawData, signature.channel);
                    return false;
                }
                default: throw new ArgumentOutOfRangeException(nameof(signature.type));
            }

            bool ShouldSend(PlayerID player)
            {
                if (player == info.sender && (signature.excludeSender || signature.runLocally))
                    return false;

                return !signature.excludeOwner || IsNotOwnerPredicate(player);
            }
        }

        static readonly List<PlayerID> _players = new List<PlayerID>();

        public void SendToObservers(ByteData packet, [CanBeNull] Func<PlayerID, bool> predicate,
            Channel method = Channel.ReliableOrdered)
        {
            if (predicate != null)
            {
                _players.Clear();
                _players.AddRange(observers);

                for (int i = 0; i < _players.Count; i++)
                {
                    if (!predicate(_players[i]))
                        _players.RemoveAt(i--);
                }

                Send(_players, packet, method);
            }
            else Send(observers, packet, method);
        }

        public void SendToObservers<T>(T packet, [CanBeNull] Func<PlayerID, bool> predicate,
            Channel method = Channel.ReliableOrdered)
        {
            if (predicate != null)
            {
                _players.Clear();
                _players.AddRange(observers);

                for (int i = 0; i < _players.Count; i++)
                {
                    if (!predicate(_players[i]))
                        _players.RemoveAt(i--);
                }

                Send(_players, packet, method);
            }
            else Send(observers, packet, method);
        }

        public void Send<T>(PlayerID player, T packet, Channel method = Channel.ReliableOrdered)
        {
            if (networkManager.isServer)
                networkManager.GetModule<PlayersManager>(true).Send(player, packet, method);
        }

        public void Send(PlayerID player, ByteData data, Channel method = Channel.ReliableOrdered)
        {
            if (networkManager.isServer)
                networkManager.GetModule<PlayersManager>(true).SendRaw(player, data, method);
        }

        [Preserve]
        public void SendToTarget(PlayerID player, ByteData data, Channel method = Channel.ReliableOrdered)
        {
            if (!observers.Contains(player))
            {
                PurrLogger.LogError($"Trying to send TargetRPC to player '{player}' which is not observing '{name}'.",
                    this);
                return;
            }

            Send(player, data, method);
        }

        [Preserve]
        public void SendToTarget<T>(PlayerID player, T packet, Channel method = Channel.ReliableOrdered)
        {
            if (!observers.Contains(player))
            {
                PurrLogger.LogError($"Trying to send TargetRPC to player '{player}' which is not observing '{name}'.",
                    this);
                return;
            }

            Send(player, packet, method);
        }

        public void Send<T>(IEnumerable<PlayerID> players, T data, Channel method = Channel.ReliableOrdered)
        {
            if (networkManager.isServer)
                networkManager.GetModule<PlayersManager>(true).Send(players, data, method);
        }

        public void Send(IEnumerable<PlayerID> players, ByteData data, Channel method = Channel.ReliableOrdered)
        {
            if (networkManager.isServer)
                networkManager.GetModule<PlayersManager>(true).SendRaw(players, data, method);
        }

        public void SendToServer<T>(T packet, Channel method = Channel.ReliableOrdered)
        {
            if (networkManager.isClient)
                networkManager.GetModule<PlayersManager>(false).SendToServer(packet, method);
        }
    }
}