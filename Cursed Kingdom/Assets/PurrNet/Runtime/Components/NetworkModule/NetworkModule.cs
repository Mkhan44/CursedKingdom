using System;
using System.Reflection;
using JetBrains.Annotations;
using PurrNet.Logging;
using PurrNet.Modules;
using PurrNet.Packing;
using PurrNet.Transports;

namespace PurrNet
{
    public class NetworkModule
    {
        public NetworkIdentity parent { get; private set; }

        [UsedImplicitly] public string name { get; private set; }

        [UsedImplicitly] public byte index { get; private set; } = 255;

        [UsedImplicitly] public NetworkManager networkManager => parent ? parent.networkManager : null;

        [UsedImplicitly] public bool isSceneObject => parent && parent.isSceneObject;

        [UsedImplicitly] public bool isOwner => parent && parent.isOwner;

        [UsedImplicitly] public bool isClient => parent && parent.isClient;

        [UsedImplicitly] public bool isServer => parent && parent.isServer;

        [UsedImplicitly] public bool isServerOnly => parent && parent.isServerOnly;

        [UsedImplicitly] public bool isHost => parent && parent.isHost;

        [UsedImplicitly] public bool isSpawned => parent && parent.isSpawned;

        public bool hasOwner => parent.hasOwner;

        public bool hasConnectedOwner => parent && parent.hasConnectedOwner;

        [UsedImplicitly] public PlayerID? localPlayer => parent ? parent.localPlayer : null;

        [UsedByIL] protected PlayerID localPlayerForced => parent ? parent.localPlayerForced : default;

        public PlayerID? owner => parent ? parent.owner : null;

        public bool isController => parent && parent.isController;

        [UsedImplicitly]
        public bool IsController(bool ownerHasAuthority) => parent && parent.IsController(ownerHasAuthority);

        [UsedImplicitly]
        public bool IsController(bool ownerHasAuthority, bool asServer) =>
            parent && parent.IsController(asServer, ownerHasAuthority);

        [UsedByIL]
        public void Error(string message)
        {
            PurrLogger.LogWarning($"Module in {parent.GetType().Name} is null: <i>{message}</i>\n" +
                                  $"You can initialize it on Awake or override OnInitializeModules.", parent);
        }

        public virtual void OnSpawn()
        {
        }

        public virtual void OnSpawn(bool asServer)
        {
        }

        public virtual void OnDespawned()
        {
        }

        public virtual void OnDespawned(bool asServer)
        {
        }

        /// <summary>
        /// Called when an observer is added.
        /// Server only.
        /// </summary>
        /// <param name="player"></param>
        public virtual void OnObserverAdded(PlayerID player)
        {
        }

        /// <summary>
        /// Called when an observer is removed.
        /// Server only.
        /// </summary>
        /// <param name="player"></param>
        public virtual void OnObserverRemoved(PlayerID player)
        {
        }

        public virtual void OnOwnerChanged(PlayerID? oldOwner, PlayerID? newOwner, bool asServer)
        {
        }

        public virtual void OnOwnerDisconnected(PlayerID ownerId)
        {
        }

        public virtual void OnOwnerReconnected(PlayerID ownerId)
        {
        }

        public void SetComponentParent(NetworkIdentity p, byte i, string moduleName)
        {
            parent = p;
            index = i;
            name = moduleName;
        }

        [UsedByIL]
        public void RegisterModuleInternal(string moduleName, string type, NetworkModule module, bool isNetworkIdentity)
        {
            var parentRef = this.parent;

            if (parentRef)
                parentRef.RegisterModuleInternal(moduleName, type, module, isNetworkIdentity);
            else PurrLogger.LogError($"Registering module '{moduleName}' failed since it is not spawned.");
        }

        [UsedByIL]
        protected void SendRPC(ChildRPCPacket packet, RPCSignature signature)
        {
            if (!parent)
            {
                if (signature.channel is Channel.ReliableOrdered)
                    PurrLogger.LogError($"Trying to send RPC from '{GetType().Name}' which is not initialized.");
                return;
            }

            if (!parent.isSpawned)
            {
                if (signature.channel is Channel.ReliableOrdered)
                    PurrLogger.LogError($"Trying to send RPC from '{parent.name}' which is not spawned.", parent);
                return;
            }

            var nm = parent.networkManager;

            if (!nm.TryGetModule<RPCModule>(nm.isServer, out var module))
            {
                PurrLogger.LogError("Failed to get RPC module.", parent);
                return;
            }

            var rules = networkManager.networkRules;
            bool shouldIgnoreOwnership = rules && rules.ShouldIgnoreRequireOwner();

            if (!shouldIgnoreOwnership && signature.requireOwnership && !isOwner)
            {
                if (!signature.runLocally)
                    PurrLogger.LogError(
                        $"Trying to send RPC '{signature.rpcName}' from '{GetType().Name}' without ownership.", parent);
                return;
            }

            bool shouldIgnore = rules && rules.ShouldIgnoreRequireServer();

            if (!shouldIgnore && signature.requireServer && !networkManager.isServer)
            {
                if (!signature.runLocally)
                    PurrLogger.LogError(
                        $"Trying to send RPC '{signature.rpcName}' from '{GetType().Name}' without server.", parent);
                return;
            }

            module.AppendToBufferedRPCs(packet, signature);

            switch (signature.type)
            {
                case RPCType.ServerRPC: parent.SendToServer(packet, signature.channel); break;
                case RPCType.ObserversRPC:
                {
                    if (isServer)
                        parent.SendToObservers(packet, ShouldSend, signature.channel);
                    else parent.SendToServer(packet, signature.channel);
                    break;
                }
                case RPCType.TargetRPC:
                    if (isServer)
                        parent.SendToTarget(signature.targetPlayer!.Value, packet, signature.channel);
                    else parent.SendToServer(packet, signature.channel);
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

                return !signature.excludeOwner || parent.IsNotOwnerPredicate(player);
            }
        }

        [UsedByIL]
        protected bool ValidateReceivingRPC(RPCInfo info, RPCSignature signature, IRpc data, bool asServer)
        {
            return parent && parent.ValidateReceivingRPC(info, signature, data, asServer);
        }

        [UsedByIL]
        protected object CallGeneric(string methodName, GenericRPCHeader rpcHeader)
        {
            var key = new NetworkIdentity.InstanceGenericKey(methodName, GetType(), rpcHeader.types);

            if (!NetworkIdentity.genericMethods.TryGetValue(key, out var gmethod))
            {
                var method = GetType().GetMethod(methodName,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                gmethod = method?.MakeGenericMethod(rpcHeader.types);

                NetworkIdentity.genericMethods.Add(key, gmethod);
            }

            if (gmethod == null)
            {
                PurrLogger.LogError($"Calling generic RPC failed. Method '{methodName}' not found.");
                return null;
            }

            return gmethod.Invoke(this, rpcHeader.values);
        }

        [UsedByIL]
        protected ChildRPCPacket BuildRPC(byte rpcId, BitPacker data)
        {
            if (!parent)
                throw new InvalidOperationException(
                    $"Trying to send RPC from '{GetType().Name}' which is not spawned.");

            var rpc = new ChildRPCPacket
            {
                networkId = parent.id!.Value,
                sceneId = parent.sceneId,
                childId = index,
                rpcId = rpcId,
                data = data.ToByteData(),
                senderId = RPCModule.GetLocalPlayer(networkManager)
            };

            return rpc;
        }

        public virtual void OnInitializeModules()
        {
        }

        /// <summary>
        /// Called when this object is spawned but before any other data is received.
        /// At this point you might be missing ownership data, module data, etc.
        /// This is only called once even if in host mode.
        /// </summary>
        public virtual void OnEarlySpawn()
        {
        }

        /// <summary>
        /// Called when this object is spawned but before any other data is received.
        /// At this point you might be missing ownership data, module data, etc.
        /// This is called twice in host mode, once for the server and once for the client.
        /// </summary>
        public virtual void OnEarlySpawn(bool asServer)
        {
        }

        /// <summary>
        /// Called when this object is put back into the pool.
        /// Use this to reset any values for the next spawn.
        /// </summary>
        public virtual void OnPoolReset()
        {
        }
    }
}