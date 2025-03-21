using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using PurrNet.Collections;
using PurrNet.Logging;
using PurrNet.Modules;
using PurrNet.Pooling;
using PurrNet.Utils;
using UnityEngine;

namespace PurrNet
{
    public delegate void NidParentChanged(NetworkIdentity oldParent, NetworkIdentity newParent);

    [DefaultExecutionOrder(-1000)]
    public partial class NetworkIdentity : MonoBehaviour
    {
        [SerializeField, HideInInspector] private bool _isSetup;

        [SerializeField, HideInInspector] private int _prefabId = int.MinValue;

        [SerializeField, HideInInspector] private int _componentIndex = int.MinValue;

        [SerializeField, HideInInspector] private bool _shouldBePooled;

        [SerializeField, HideInInspector] private NetworkIdentity _parent;

        [SerializeField, HideInInspector] private int[] _invertedPathToNearestParent;

        [SerializeField, HideInInspector] private List<NetworkIdentity> _directChildren;

        public event Action<PlayerID> onObserverAdded;

        public event Action<PlayerID> onObserverRemoved;

        internal Transform defaultParent { get; private set; }

        public RollbackModule rollbackModule
        {
            get
            {
                if (!networkManager)
                    return null;

                if (networkManager.TryGetModule<ColliderRollbackFactory>(isServer, out var factory) &&
                    factory.TryGetModule(sceneId, out var module))
                    return module;

                return null;
            }
        }

        public double rollbackTick => networkManager ? networkManager.tickModule.rollbackTick : 0;

        public int[] invertedPathToNearestParent
        {
            get => _invertedPathToNearestParent;
            internal set => _invertedPathToNearestParent = value;
        }

        public IReadOnlyList<NetworkIdentity> directChildren => _directChildren;

        /// <summary>
        /// The nearest network parent of this object.
        /// This can differ from transform.parent.
        /// Especially if syncing the parent is disabled.
        /// </summary>
        public NetworkIdentity parent
        {
            get => _parent;
            internal set
            {
                if (_parent == value)
                    return;

                var oldParent = _parent;
                _parent = value;
                onParentChanged?.Invoke(oldParent, value);
            }
        }

        /// <summary>
        /// Called when the network parent of this object changes.
        /// This will be the closest parent with a NetworkIdentity.
        /// </summary>
        public event NidParentChanged onParentChanged;

        public int prefabId => _prefabId;

        public int componentIndex => _componentIndex;

        public bool shouldBePooled => _shouldBePooled;

        public bool isSetup => _isSetup;

        public void PreparePrefabInfo(int prefabId, int componentIndex, bool shouldBePooled, bool isSceneObject)
        {
            _isSetup = true;

            if (isSceneObject)
                defaultParent = transform.parent;

            this.isSceneObject = isSceneObject;

            this._prefabId = prefabId;
            this._componentIndex = componentIndex;
            this._shouldBePooled = shouldBePooled;

            parent = GetNearestParent();

            RecalculateNearestPath();

            var firstIdentity = GetComponent<NetworkIdentity>();

            if (firstIdentity != this)
                _directChildren = new List<NetworkIdentity>();
            else RecalculateDirectChildren();
        }

        internal void RecalculateDirectChildren()
        {
            using var dChildren = new DisposableList<TransformIdentityPair>(16);
            HierarchyPool.GetDirectChildren(transform, dChildren);

            _directChildren ??= new List<NetworkIdentity>(dChildren.Count);
            _directChildren.Clear();
            for (int i = 0; i < dChildren.Count; i++)
                _directChildren.Add(dChildren[i].identity);
        }

        internal void ClearDirectChildren()
        {
            _directChildren.Clear();
        }

        internal void AddDirectChild(NetworkIdentity identity)
        {
            if (_directChildren.Contains(identity))
                return;
            _directChildren.Add(identity);
        }

        internal void RemoveDirectChild(NetworkIdentity identity)
        {
            _directChildren.Remove(identity);
        }

        internal void RecalculateNearestPath()
        {
            if (_parent)
            {
                using var invPath = HierarchyPool.GetInvPath(_parent.transform, transform);

                if (_invertedPathToNearestParent == null)
                    _invertedPathToNearestParent = new int[invPath.Count];
                else if (_invertedPathToNearestParent.Length != invPath.Count)
                    _invertedPathToNearestParent = new int[invPath.Count];

                for (int i = 0; i < invPath.Count; i++)
                    _invertedPathToNearestParent[i] = invPath[i];
            }
            else
            {
                _invertedPathToNearestParent = Array.Empty<int>();
            }
        }

        /// <summary>
        /// Navigates the hierarchy to find the nearest parent with a NetworkIdentity.
        /// </summary>
        /// <returns>Nearest parent with a NetworkIdentity</returns>
        public NetworkIdentity GetNearestParent()
        {
            var current = transform.parent;
            while (current)
            {
                if (current.TryGetComponent(out NetworkIdentity identity))
                    return identity;

                current = current.parent;
            }

            return null;
        }

        /// <summary>
        /// Network id of this object. Holds more information than the ObjectId
        /// </summary>
        public NetworkID? id => _idServer ?? _idClient;

        public NetworkID? GetNetworkID(bool asServer) => asServer ? _idServer : _idClient;

        /// <summary>
        /// Unique ObjectId of this object
        /// </summary>
        public uint objectId => id?.id ?? 0;

        /// <summary>
        /// Scene id of this object.
        /// </summary>
        public SceneID sceneId { get; private set; }

        /// <summary>
        /// Is spawned over the network.
        /// </summary>
        public bool isSpawned => _isSpawnedServer || _isSpawnedClient;

        public bool isSceneObject { get; private set; }

        public bool isServer => isSpawned && networkManager.isServer;

        [UsedByIL] public bool isServerOnly => isSpawned && networkManager.isServerOnly;

        public bool isClient => isSpawned && networkManager.isClient;

        public bool isHost => isSpawned && networkManager.isHost;

        public bool isOwner => isSpawned && localPlayer.HasValue && owner == localPlayer;

        public bool hasOwner => owner.HasValue;

        Queue<Action> _onSpawnedQueue;

        /// <summary>
        /// Returns if you can control this object.
        /// If the object has an owner, it will return if you are the owner.
        /// If the object doesn't have an owner, it will return if you are the server.
        /// </summary>
        [UsedImplicitly]
        public bool isController => isSpawned && (hasConnectedOwner ? isOwner : isServer);

        /// <summary>
        /// Returns if you can control this object.
        /// If ownerHasAuthority is true, it will return true if you are the owner.
        /// If ownerHasAuthority is false, it will return true if you are the server.
        /// Otherwise, similar to isController.
        /// </summary>
        /// <param name="ownerHasAuthority">Should owner be controller or is it server only</param>
        /// <returns>Can you control this identity</returns>
        [UsedImplicitly]
        public bool IsController(bool ownerHasAuthority) => ownerHasAuthority ? isController : isServer;

        public bool IsController(bool ownerHasAuthority, bool asServer) => ownerHasAuthority ? isController : asServer;

        public bool IsController(PlayerID player, bool ownerHasAuthority, bool asServer)
        {
            if (!ownerHasAuthority)
                return asServer;

            if (!hasConnectedOwner)
                return asServer;

            if (player == owner)
                return true;

            return asServer;
        }

        public bool hasConnectedOwner => networkManager && owner.HasValue &&
                                         networkManager.TryGetModule<PlayersManager>(isServer, out var module) &&
                                         module.IsPlayerConnected(owner.Value);

        internal PlayerID? internalOwnerServer;
        internal PlayerID? internalOwnerClient;

        private TickManager _serverTickManager;
        private TickManager _clientTickManager;

        private bool _isSpawnedClient;
        private bool _isSpawnedServer;

        private NetworkID? _idServer;
        private NetworkID? _idClient;

        /// <summary>
        /// Returns the owner of this object.
        /// It will return the owner of the closest parent object.
        /// If you can, cache this value for performance.
        /// </summary>
        public PlayerID? owner => internalOwnerServer ?? internalOwnerClient;

        public NetworkManager networkManager { get; private set; }

        private HierarchyV2 _clientHierarchy;
        private HierarchyV2 _serverHierarchy;

        private PlayerID? _localPlayer;

        /// <summary>
        /// The cached value of the local player.
        /// </summary>
        public PlayerID? localPlayer
        {
            get
            {
                if (_localPlayer.HasValue)
                    return _localPlayer;

                if (networkManager.TryGetModule<PlayersManager>(false, out var players))
                {
                    _localPlayer = players.localPlayerId;
                    return _localPlayer;
                }

                return null;
            }
        }

        /// <summary>
        /// Returns the local player if it exists.
        /// Defaults to default(PlayerID) if it doesn't exist.
        /// </summary>
        [UsedByIL]
        public PlayerID localPlayerForced => localPlayer ?? default;

        private readonly PurrHashSet<PlayerID> _observers = new PurrHashSet<PlayerID>(4);

        public IReadonlyHashSet<PlayerID> observers => _observers;

        [UsedImplicitly]
        public void QueueOnSpawned(Action action)
        {
            _onSpawnedQueue ??= new Queue<Action>();
            _onSpawnedQueue.Enqueue(action);
        }

        public NetworkIdentity GetRootIdentity()
        {
            var lastKnown = gameObject.GetComponent<NetworkIdentity>();
            var currentParent = parent;

            while (currentParent)
            {
                lastKnown = currentParent;
                currentParent = currentParent.parent;
            }

            return lastKnown;
        }

        private IServerSceneEvents _serverSceneEvents;
        private int onTickCount;
        private ITick _ticker;

        private readonly List<ITick> _tickables = new List<ITick>();

        [ContextMenu("PurrNet/Take Ownership")]
        private void TakeOwnership()
        {
            GiveOwnership(localPlayer);
        }

        [ContextMenu("PurrNet/Print Prototype")]
        private void PrintPrototype()
        {
            using var prototype = HierarchyPool.GetFullPrototype(transform);
            PurrLogger.Log(prototype.ToString());
        }

        [ContextMenu("PurrNet/Duplicate Prototype")]
        private void DuplicatePrototype()
        {
            Duplicate();
        }

        /// <summary>
        /// Duplicates the object.
        /// </summary>
        public GameObject Duplicate()
        {
            using var prototype = HierarchyPool.GetFullPrototype(transform);
            if (networkManager.TryGetModule<HierarchyFactory>(isServer, out var factory) &&
                factory.TryGetHierarchy(sceneId, out var hierarchy))
            {
                var go = hierarchy.CreatePrototype(prototype, new List<NetworkIdentity>());
                hierarchy.Spawn(go);
                return go;
            }

            return null;
        }

        [ContextMenu("PurrNet/Destroy GameObject")]
        private void DeleteGameObject()
        {
            Destroy(gameObject);
        }

        private void InternalOnSpawn(bool asServer)
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (_ticker == null && this is ITick ticker)
                _ticker = ticker;

            if (_ticker != null || _tickables.Count > 0)
            {
                if (asServer)
                {
                    _serverTickManager = networkManager.GetModule<TickManager>(true);
                    _serverTickManager.onTick += ServerTick;
                }
                else
                {
                    _clientTickManager = networkManager.GetModule<TickManager>(false);
                    _clientTickManager.onTick += ClientTick;
                }
            }

            if (networkManager.TryGetModule<PlayersManager>(asServer, out var players))
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                if (this is IPlayerEvents events)
                {
                    players.onPlayerJoined += events.OnPlayerConnected;
                    players.onPlayerLeft += events.OnPlayerDisconnected;
                }

                if (networkManager.TryGetModule<ScenePlayersModule>(asServer, out var scenePlayers))
                {
                    // ReSharper disable once SuspiciousTypeConversion.Global
                    if (this is IServerSceneEvents sceneEvents)
                    {
                        _serverSceneEvents = sceneEvents;
                        scenePlayers.onPlayerLoadedScene += OnServerJoinedScene;
                        scenePlayers.onPlayerUnloadedScene += OnServerLeftScene;
                    }
                }
            }
        }

        private void InternalOnDespawn(bool asServer)
        {
            if (_ticker != null || _tickables.Count > 0)
            {
                if (asServer)
                {
                    if (_serverTickManager != null)
                        _serverTickManager.onTick -= ServerTick;
                }
                else
                {
                    if (_clientTickManager != null)
                        _clientTickManager.onTick -= ClientTick;
                }
            }

            if (!networkManager.TryGetModule<PlayersManager>(asServer, out var players)) return;

            // ReSharper disable once SuspiciousTypeConversion.Global
            if (this is IPlayerEvents events)
            {
                players.onPlayerJoined -= events.OnPlayerConnected;
                players.onPlayerLeft -= events.OnPlayerDisconnected;
            }

            if (!networkManager.TryGetModule<ScenePlayersModule>(asServer, out var scenePlayers)) return;

            if (_serverSceneEvents == null) return;

            scenePlayers.onPlayerLoadedScene -= OnServerJoinedScene;
            scenePlayers.onPlayerUnloadedScene -= OnServerLeftScene;
        }

        void OnServerJoinedScene(PlayerID player, SceneID scene, bool asServer)
        {
            if (scene == sceneId)
                _serverSceneEvents?.OnPlayerLoadedScene(player);
        }

        void OnServerLeftScene(PlayerID player, SceneID scene, bool asServer)
        {
            if (scene == sceneId)
                _serverSceneEvents?.OnPlayerUnloadedScene(player);
        }

        private void ClientTick()
        {
            _ticker?.OnTick(_clientTickManager.tickDelta);

            for (var i = 0; i < _tickables.Count; i++)
            {
                var ticker = _tickables[i];
                ticker.OnTick(_clientTickManager.tickDelta);
            }
        }

        private void ServerTick()
        {
            if (!isClient)
            {
                _ticker?.OnTick(_serverTickManager.tickDelta);
                for (var i = 0; i < _tickables.Count; i++)
                {
                    var ticker = _tickables[i];
                    ticker.OnTick(_serverTickManager.tickDelta);
                }
            }
        }

        internal PlayerID? GetOwner(bool asServer) => asServer ? internalOwnerServer : internalOwnerClient;

        [UsedImplicitly]
        public bool IsSpawned(bool asServer) => asServer ? _isSpawnedServer : _isSpawnedClient;

        /// <summary>
        /// Called when this object is spawned
        /// This is only called once even if in host mode.
        /// </summary>
        protected virtual void OnSpawned()
        {
        }

        /// <summary>
        /// Called when this object is spawned but before any other data is received.
        /// At this point you might be missing ownership data, module data, etc.
        /// This is only called once even if in host mode.
        /// </summary>
        protected virtual void OnEarlySpawn()
        {
        }

        /// <summary>
        /// Called when this object is spawned but before any other data is received.
        /// At this point you might be missing ownership data, module data, etc.
        /// This is called twice in host mode, once for the server and once for the client.
        /// </summary>
        protected virtual void OnEarlySpawn(bool asServer)
        {
        }

        /// <summary>
        /// Called when this object is de-spawned.
        /// This is only called once even if in host mode.
        /// </summary>
        protected virtual void OnDespawned()
        {
        }

        /// <summary>
        /// Called when this object is spawned.
        /// This might be called twice times in host mode.
        /// Once for the server and once for the client.
        /// </summary>
        /// <param name="asServer">Is this on the server</param>
        protected virtual void OnSpawned(bool asServer)
        {
        }

        /// <summary>
        /// Called before the NetworkModules are initialized.
        /// You can use this to update their values before they are networked.
        /// </summary>
        protected virtual void OnInitializeModules()
        {
        }

        /// <summary>
        /// Called when this object is de-spawned.
        /// This might be called twice times in host mode.
        /// Once for the server and once for the client.
        /// </summary>
        /// <param name="asServer">Is this on the server</param>
        protected virtual void OnDespawned(bool asServer)
        {
        }

        /// <summary>
        /// Called when the owner of this object changes.
        /// </summary>
        /// <param name="oldOwner">The old owner of this object</param>
        /// <param name="newOwner">The new owner of this object</param>
        /// <param name="asServer">Is this on the server</param>
        protected virtual void OnOwnerChanged(PlayerID? oldOwner, PlayerID? newOwner, bool asServer)
        {
        }

        /// <summary>
        /// Called when the owner of this object disconnects.
        /// Server only.
        /// </summary>
        /// <param name="ownerId">The current owner id</param>
        protected virtual void OnOwnerDisconnected(PlayerID ownerId)
        {
        }

        /// <summary>
        /// Called when the owner of this object reconnects.
        /// Server only.
        /// </summary>
        /// <param name="ownerId">The current owner id</param>
        protected virtual void OnOwnerReconnected(PlayerID ownerId)
        {
        }

        /// <summary>
        /// Called when an observer is added.
        /// Server only.
        /// </summary>
        /// <param name="player">The observer player id</param>
        protected virtual void OnObserverAdded(PlayerID player)
        {
        }

        /// <summary>
        /// Same as OnObserverAdded but called after all other observers have been added.
        /// </summary>
        /// <param name="player">The observer player id</param>
        protected virtual void OnLateObserverAdded(PlayerID player)
        {
        }

        /// <summary>
        /// Called when an observer is removed.
        /// Server only.
        /// </summary>
        /// <param name="player">The observer player id</param>
        protected virtual void OnObserverRemoved(PlayerID player)
        {
        }

        public bool IsNotOwnerPredicate(PlayerID player)
        {
            return player != owner;
        }

        private void CallInitMethods()
        {
            var type = GetType();
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);

            for (int i = 0; i < methods.Length; i++)
            {
                var m = methods[i];
                if (m.Name.EndsWith("_CodeGen_Initialize"))
                    m.Invoke(this, Array.Empty<object>());
            }
        }

        /// <summary>
        /// The layer of this object. Avoids gameObject.layer.
        /// Only available when spawned.
        /// </summary>
        public int layer { get; private set; }

        public bool isInPool { get; private set; }

        [ContextMenu("PurrNet/Despawn")]
        public void Despawn()
        {
            if (isSpawned)
            {
                if (_serverHierarchy != null)
                    _serverHierarchy.Despawn(gameObject, false, false);
                else _clientHierarchy?.Despawn(gameObject, false, false);
            }
            else if (!isInPool)
                UnityProxy.DestroyDirectly(gameObject);
        }

        /// <summary>
        /// Called when this object is put back into the pool.
        /// Use this to reset any values for the next spawn.
        /// </summary>
        protected virtual void OnPoolReset()
        {
        }

        internal void ResetIdentity()
        {
            OnPoolReset();

            for (int i = 0; i < _externalModulesView.Count; i++)
                _externalModulesView[i].OnPoolReset();

            // notify parent
            if (parent && parent.isSpawned)
                parent.OnChildDespawned(this);

            // reset all values
            TriggerDespawnEvent(false);
            TriggerDespawnEvent(true);
            networkManager = null;
            sceneId = default;
            _localPlayer = null;
            _isSpawnedServer = false;
            _isSpawnedClient = false;
            _idServer = null;
            _idClient = null;
            internalOwnerServer = null;
            internalOwnerClient = null;
            _moduleId = 0;
            _modules.Clear();
            _externalModulesView.Clear();
            _tickables.Clear();
            _visitiblityRules = null;
            _spawnedCount = 0;
            _onSpawnedQueue?.Clear();
            _serverSceneEvents = null;
            _serverTickManager = null;
            _clientTickManager = null;
            _ticker = null;
            isInPool = true;
        }

        private void OnChildDespawned(NetworkIdentity networkIdentity)
        {
            _directChildren.Remove(networkIdentity);
        }

        internal void SetIdentity(NetworkManager manager, HierarchyV2 hierarchy, SceneID scene, bool asServer,
            bool asHost)
        {
            isInPool = false;
            layer = gameObject.layer;
            networkManager = manager;
            sceneId = scene;

            bool wasAlreadySpawned = isSpawned;

            if (asServer)
            {
                _isSpawnedServer = true;
                if (asHost)
                    _isSpawnedClient = true;

                _serverHierarchy = hierarchy;
                internalOwnerServer = null;
            }
            else
            {
                _isSpawnedClient = true;
                _clientHierarchy = hierarchy;
                internalOwnerClient = null;
            }

            if (!wasAlreadySpawned)
            {
                _modules.Clear();
                _externalModulesView.Clear();
                _moduleId = 0;

                OnInitializeModules();
                CallInitMethods();

                foreach (var module in _externalModulesView)
                    module.OnInitializeModules();

                _tickables.Clear();
                RegisterEvents();
            }

            if (_visitiblityRules && !_visitiblityRules.isInitialized)
            {
                _visitiblityRules = Instantiate(_visitiblityRules);
                _visitiblityRules.Setup(manager);
            }
        }

        private PlayerID? _pendingOwnershipRequest;

        /// <summary>
        /// Evaluates the visibility of this object.
        /// This will recalculate the observers of this object.
        /// This is server specific.
        /// Re-evaulation includes all children.
        /// </summary>
        public void EvaluateVisibility()
        {
            if (!isServer)
                return;

            _serverHierarchy.EvaluateVisibility(transform);
        }

        /// <summary>
        /// Gives ownership of this object to the player.
        /// </summary>
        /// <param name="player">PlayerID to give ownership to</param>
        /// <param name="silent">Dont log any errors if in silent mode</param>
        public void GiveOwnership(PlayerID player, bool silent = false)
        {
            if (!networkManager)
                return;
            GiveOwnershipInternal(player, silent);
        }

        /// <summary>
        /// Spawns the object over the network.
        /// The gameobject must contain a PrefabLink component in order to spawn.
        /// Errors will be logged if something goes wrong.
        /// </summary>
        /// <param name="prefab">Prefab used to spawn the object</param>
        /// <param name="manager">Optional NetworkManager to use, will use NetworkManager.main if not provided</param>
        public void Spawn(GameObject prefab, NetworkManager manager = null)
        {
            if (isSpawned)
                return;

            if (!manager)
            {
                manager = NetworkManager.main;

                if (!manager)
                {
                    PurrLogger.LogError("Failed to spawn object. No NetworkManager found.", this);
                    return;
                }
            }

            if (manager.TryGetModule(manager.isServer, out HierarchyFactory module) &&
                module.TryGetHierarchy(gameObject.scene, out var hierarchy))
            {
                hierarchy.OnGameObjectCreated(gameObject, prefab);
            }
        }

        internal void Spawn(NetworkManager manager = null)
        {
            if (isSpawned)
                return;

            if (!manager)
            {
                manager = NetworkManager.main;

                if (!manager)
                {
                    PurrLogger.LogError("Failed to spawn object. No NetworkManager found.", this);
                    return;
                }
            }

            if (manager.TryGetModule(manager.isServer, out HierarchyFactory module) &&
                module.TryGetHierarchy(gameObject.scene, out var hierarchy))
            {
                hierarchy.Spawn(gameObject);
            }
        }


        /// <summary>
        /// Spawn any child objects of this object.
        /// </summary>
        /// <param name="go">GameObject to spawn</param>
        /// <param name="prefab">Prefab used to spawn the object</param>
        /// <param name="manager">Optional NetworkManager to use, will use NetworkManager.main if not provided</param>
        public static void Spawn(GameObject go, GameObject prefab, NetworkManager manager = null)
        {
            if (!go)
                return;

            if (go.TryGetComponent(out NetworkIdentity identity))
            {
                identity.Spawn(prefab, manager);
                return;
            }

            using var identities = new DisposableList<TransformIdentityPair>(16);
            HierarchyPool.GetDirectChildren(go.transform, identities);

            for (var i = 0; i < identities.Count; i++)
            {
                var pair = identities[i];
                pair.identity.Spawn(prefab, manager);
            }
        }

        public static void SpawnInternal(GameObject go, NetworkManager manager = null)
        {
            if (!go)
                return;

            if (go.TryGetComponent(out NetworkIdentity identity))
            {
                identity.Spawn(manager);
                return;
            }

            using var identities = new DisposableList<TransformIdentityPair>(16);
            HierarchyPool.GetDirectChildren(go.transform, identities);

            for (var i = 0; i < identities.Count; i++)
            {
                var pair = identities[i];
                pair.identity.Spawn(manager);
            }
        }

        [UsedImplicitly]
        public void GiveOwnership(PlayerID? player, bool silent = false)
        {
            if (!player.HasValue)
            {
                RemoveOwnership();
                return;
            }

            GiveOwnership(player.Value, silent);
        }

        private void GiveOwnershipInternal(PlayerID player, bool silent = false)
        {
            if (!networkManager)
            {
                if (!silent)
                    PurrLogger.LogError("Trying to give ownership to " + player + " but identity isn't spawned.", this);
                return;
            }

            if (networkManager.TryGetModule(networkManager.isServer, out GlobalOwnershipModule module))
            {
                module.GiveOwnership(this, player, silent: silent);
            }
            else if (!silent) PurrLogger.LogError("Failed to get ownership module.", this);
        }

        public void RemoveOwnership()
        {
            if (!networkManager)
                return;

            if (networkManager.TryGetModule(networkManager.isServer, out GlobalOwnershipModule module))
            {
                module.RemoveOwnership(this);
            }
            else PurrLogger.LogError("Failed to get ownership module.");
        }

        protected virtual void OnDestroy()
        {
            if (ApplicationContext.isQuitting)
                return;

            TriggerDespawnEvent(true);
            TriggerDespawnEvent(false);

            _ticker = null;
        }

        private int _spawnedCount;
        private bool _wasEarlySpawned;

        internal void TriggerSpawnEvent(bool asServer)
        {
            InternalOnSpawn(asServer);
            OnSpawned(asServer);

            for (int i = 0; i < _externalModulesView.Count; i++)
                _externalModulesView[i].OnSpawn(asServer);

            if (_spawnedCount == 0)
            {
                while (_onSpawnedQueue is { Count: > 0 })
                    _onSpawnedQueue.Dequeue().Invoke();

                OnSpawned();

                for (int i = 0; i < _externalModulesView.Count; i++)
                    _externalModulesView[i].OnSpawn();
            }

            _spawnedCount++;
        }

        internal void TriggerEarlySpawnEvent(bool asServer)
        {
            OnEarlySpawn(asServer);

            for (int i = 0; i < _externalModulesView.Count; i++)
                _externalModulesView[i].OnEarlySpawn(asServer);

            if (!_wasEarlySpawned)
            {
                OnEarlySpawn();

                for (int i = 0; i < _externalModulesView.Count; i++)
                    _externalModulesView[i].OnEarlySpawn();

                _wasEarlySpawned = true;
            }
        }

        internal void TriggerDespawnEvent(bool asServer)
        {
            if (!IsSpawned(asServer)) return;

            InternalOnDespawn(asServer);

            --_spawnedCount;
            _wasEarlySpawned = false;

            OnDespawned(asServer);

            for (int i = 0; i < _externalModulesView.Count; i++)
                _externalModulesView[i].OnDespawned(asServer);

            if (_spawnedCount == 0)
            {
                OnDespawned();

                for (int i = 0; i < _externalModulesView.Count; i++)
                    _externalModulesView[i].OnDespawned();
            }

            if (asServer)
                _isSpawnedServer = false;
            else _isSpawnedClient = false;

            if (_spawnedCount == 0)
            {
                _externalModulesView.Clear();
                _modules.Clear();
            }
        }

        internal void TriggerOnOwnerChanged(PlayerID? oldOwner, PlayerID? newOwner, bool asServer)
        {
            OnOwnerChanged(oldOwner, newOwner, asServer);

            for (int i = 0; i < _externalModulesView.Count; i++)
                _externalModulesView[i].OnOwnerChanged(oldOwner, newOwner, asServer);
        }

        internal void TriggerOnOwnerDisconnected(PlayerID ownerId)
        {
            OnOwnerDisconnected(ownerId);

            for (int i = 0; i < _externalModulesView.Count; i++)
                _externalModulesView[i].OnOwnerDisconnected(ownerId);
        }

        internal void TriggerOnOwnerReconnected(PlayerID ownerId, bool asServer)
        {
            OnOwnerReconnected(ownerId);

            for (int i = 0; i < _externalModulesView.Count; i++)
                _externalModulesView[i].OnOwnerReconnected(ownerId);
        }

        public void TriggerOnObserverAdded(PlayerID target)
        {
            OnObserverAdded(target);
            for (int i = 0; i < _externalModulesView.Count; i++)
                _externalModulesView[i].OnObserverAdded(target);
            onObserverAdded?.Invoke(target);
        }

        public void TriggerOnObserverRemoved(PlayerID target)
        {
            OnObserverRemoved(target);
            for (int i = 0; i < _externalModulesView.Count; i++)
                _externalModulesView[i].OnObserverRemoved(target);
            onObserverRemoved?.Invoke(target);
        }

        internal void ClearObservers()
        {
            _observers.Clear();
        }

        public void SetID(NetworkID networkID)
        {
            _idServer = networkID;
            _idClient = networkID;
        }

        public void SetIsSpawned(bool isSpawned, bool asServer)
        {
            if (asServer)
                _isSpawnedServer = isSpawned;
            else _isSpawnedClient = isSpawned;
        }
    }
}
