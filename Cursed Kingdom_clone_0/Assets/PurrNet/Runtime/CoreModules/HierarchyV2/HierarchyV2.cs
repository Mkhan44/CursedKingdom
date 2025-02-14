using System.Collections.Generic;
using PurrNet.Logging;
using PurrNet.Pooling;
using PurrNet.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PurrNet.Modules
{
    public delegate void IdentityAction(NetworkIdentity identity);
    public delegate void ObserverAction(PlayerID player, NetworkIdentity identity);
    
    public class HierarchyV2
    {
        private readonly NetworkManager _manager;
        private readonly bool _asServer;
        private readonly SceneID _sceneId;
        private readonly Scene _scene;
        private readonly ScenePlayersModule _scenePlayers;
        private readonly PlayersManager _playersManager;
        private readonly VisilityV2 _visibility;
        
        private readonly HierarchyPool _scenePool;
        private readonly HierarchyPool _prefabsPool;
        
        private readonly List<NetworkIdentity> _spawnedIdentities = new();
        private readonly Dictionary<NetworkID, NetworkIdentity> _spawnedIdentitiesMap = new();
        
        private uint _nextId;

        private bool _areSceneObjectsReady;
        
        public event IdentityAction onEarlyIdentityAdded;

        public event IdentityAction onIdentityAdded;
        
        public event IdentityAction onIdentityRemoved;

        public event ObserverAction onObserverAdded;
        
        public event ObserverAction onObserverRemoved;

        private bool _isPlayerReady;
        
        public HierarchyV2(NetworkManager manager, SceneID sceneId, Scene scene, 
            ScenePlayersModule players, PlayersManager playersManager, bool asServer)
        {
            _manager = manager;
            _sceneId = sceneId;
            _scene = scene;
            _scenePlayers = players;
            _visibility = new VisilityV2(_manager);
            _asServer = asServer;
            _playersManager = playersManager;
            
            _scenePool = NetworkPoolManager.GetScenePool(scene, sceneId);
            _prefabsPool = NetworkPoolManager.GetPool(manager);
            
            SetupSceneObjects(scene);
        }

        readonly List<GameObjectPrototype> _defaultPrototypes = new List<GameObjectPrototype>();

        private void SetupSceneObjects(Scene scene)
        {
            if (_manager.TryGetModule<HierarchyFactory>(!_asServer, out var factory) &&
                factory.TryGetHierarchy(_sceneId, out var other))
            {
                if (other._areSceneObjectsReady)
                {
                    _areSceneObjectsReady = true;
                    return;
                }
            }
            
            if (_areSceneObjectsReady)
                return;
            
            _defaultPrototypes.Clear();
            
            var allSceneIdentities = ListPool<NetworkIdentity>.Instantiate();
            SceneObjectsModule.GetSceneIdentities(scene, allSceneIdentities);
            
            var roots = HashSetPool<NetworkIdentity>.Instantiate();

            var count = allSceneIdentities.Count;
            for (int i = 0; i < count; i++)
            {
                var identity = allSceneIdentities[i];
                var root = identity.GetRootIdentity();
                
                if (!roots.Add(root))
                    continue;
                
                var children = ListPool<NetworkIdentity>.Instantiate();
                root.GetComponentsInChildren(true, children);
                
                var cc = children.Count;
                var pid = -i - 2;
                
                for (int j = 0; j < cc; j++)
                {
                    var child = children[j];
                    
                    if (child.isSetup)
                        continue;
                    
                    var trs = child.transform;
                    var first = trs.GetComponent<NetworkIdentity>();

                    child.PreparePrefabInfo(pid, child == first ? j : first.componentIndex, true, true);
                    
                    if (!_asServer)
                        child.ResetIdentity();
                }

                SpawnSceneObject(children);
                _defaultPrototypes.Add(HierarchyPool.GetFullPrototype(root.transform));
                ListPool<NetworkIdentity>.Destroy(children);
            }

            if (!_asServer)
            {
                foreach (var root in roots)
                    _scenePool.PutBackInPool(root.gameObject);
            }
            
            ListPool<NetworkIdentity>.Destroy(allSceneIdentities);
            _areSceneObjectsReady = true;
        }

        public void Enable()
        {
            PurrNetGameObjectUtils.onGameObjectCreated += OnGameObjectCreated;
            _visibility.visibilityChanged += OnVisibilityChanged;
            _scenePlayers.onPrePlayerloadedScene += OnPlayerLoadedScene;
            _scenePlayers.onPlayerUnloadedScene += OnPlayerUnloadedScene;
            _playersManager.onNetworkIDReceived += OnNetworkIDReceived;

            if (_playersManager.localPlayerId.HasValue)
                _isPlayerReady = true;
            else _playersManager.onLocalPlayerReceivedID += OnPlayerReceivedID;

            
            _playersManager.Subscribe<SpawnPacket>(OnSpawnPacket);
            _playersManager.Subscribe<DespawnPacket>(OnDespawnPacket);
            _playersManager.Subscribe<FinishSpawnPacket>(OnFinishSpawnPacket);
            _playersManager.Subscribe<ChangeParentPacket>(OnParentChangedPacket);
        }

        public void Disable()
        {
            PurrNetGameObjectUtils.onGameObjectCreated -= OnGameObjectCreated;
            _visibility.visibilityChanged -= OnVisibilityChanged;
            _scenePlayers.onPrePlayerloadedScene -= OnPlayerLoadedScene;
            _scenePlayers.onPlayerUnloadedScene -= OnPlayerUnloadedScene;
            _playersManager.onLocalPlayerReceivedID -= OnPlayerReceivedID;
            _playersManager.onNetworkIDReceived -= OnNetworkIDReceived;

            _playersManager.Unsubscribe<SpawnPacket>(OnSpawnPacket);
            _playersManager.Unsubscribe<DespawnPacket>(OnDespawnPacket);
            _playersManager.Unsubscribe<FinishSpawnPacket>(OnFinishSpawnPacket);
            _playersManager.Unsubscribe<ChangeParentPacket>(OnParentChangedPacket);
        }
        
        bool _isDisposed;
        
        public bool Cleanup()
        {
            if (_isDisposed)
                return true;
            
            _isDisposed = true;
            
            if (ApplicationContext.isQuitting)
            {
                return true;
            }
            
            var hash = HashSetPool<NetworkIdentity>.Instantiate();

            for (var i = 0; i < _spawnedIdentities.Count; i++)
            {
                var nid = _spawnedIdentities[i];
                var root = nid.GetRootIdentity();

                if (!root)
                    continue;

                hash.Add(root);
            }

            foreach (var r in hash)
                Despawn(r.gameObject, true, true);

            foreach (var defaultPrototype in _defaultPrototypes)
            {
                CreatePrototype(defaultPrototype, null);
                defaultPrototype.Dispose();
            }
            
            _defaultPrototypes.Clear();
            
            HashSetPool<NetworkIdentity>.Destroy(hash);
            return true;
        }

        private void OnNetworkIDReceived(NetworkID nid)
        {
            if (nid.id >= _nextId)
                _nextId = nid.id + 1;
        }

        private void OnPlayerReceivedID(PlayerID player)
        {
            _isPlayerReady = true;
            
            if (!_asServer && _manager.TryGetModule<HierarchyFactory>(true, out var factory) &&
                factory.TryGetHierarchy(_sceneId, out var other))
            {
                other.CatchupClient();
            }
        }

        private void OnParentChangedPacket(PlayerID player, ChangeParentPacket data, bool asserver)
        {
            // when in host mode, let the server handle the spawning on their module
            if (!_asServer && _manager.isServer)
                return;
            
            if (data.sceneId != _sceneId)
                return;

            if (!TryGetIdentity(data.childId, out var identity))
                return;

            if (_asServer && !identity.HasChangeParentAuthority(player, !_asServer))
            {
                PurrLogger.LogError($"Change parent failed for '{identity.gameObject.name}' due to lack of permissions.", identity.gameObject);
                return;
            }
            
            NetworkIdentity parent = null;
            
            if (data.newParentId.HasValue && !TryGetIdentity(data.newParentId.Value, out parent))
            {
                PurrLogger.LogError($"Change parent failed for '{identity.gameObject.name}'. Parent not found.", identity.gameObject);
                return;
            }
            
            ApplyParentChange(identity, parent, data.path, true);
        }

        static NetworkIdentity ClosestParent(Transform trs)
        {
            if (!trs)
                return null;
            
            var parent = trs;
            while (parent)
            {
                if (parent.TryGetComponent<NetworkIdentity>(out var nid))
                    return nid;
                
                parent = parent.parent;
            }

            return null;
        }
        
        void ApplyParentChange(NetworkIdentity identity, NetworkIdentity parent, int[] path, bool refreshVisibility)
        {
            var idTrs = identity.transform;
            var oldParent = identity.parent;
            
            var tmpList = ListPool<NetworkIdentity>.Instantiate();
            identity.GetComponents(tmpList);
            
            var first = tmpList[0];
            
            for (var i = 0; i < tmpList.Count; i++)
            {
                var child = tmpList[i];
                child.parent = parent;
                child.invertedPathToNearestParent = path;
            }
            
            ListPool<NetworkIdentity>.Destroy(tmpList);

            if (parent)
            {
                var nt = identity.GetComponent<NetworkTransform>();
                if (nt) nt.StartIgnoringParentChanges();
                HierarchyPool.WalkThePath(parent.transform, idTrs, path);
                if (nt) nt.StopIgnoringParentChanges();
            }
            else idTrs.SetParent(null, true);
            
            if (parent)
                parent.AddDirectChild(first);
            
            if (oldParent && parent != oldParent)
                oldParent.RemoveDirectChild(first);
            
            if (refreshVisibility && _asServer && _scenePlayers.TryGetPlayersInScene(_sceneId, out var players))
            {
                foreach (var player in players)
                    _visibility.RefreshVisibilityForGameObject(player, idTrs, parent);
            }
        }
        
        internal void OnParentChanged(NetworkIdentity identity, Transform parent)
        {
            var closestNid = ClosestParent(parent);
            var oldParent = identity.parent;
            
            var tmpList = ListPool<NetworkIdentity>.Instantiate();
            identity.GetComponents(tmpList);

            var first = tmpList[0];
            first.parent = closestNid;
            first.RecalculateNearestPath();
            
            for (var i = 1; i < tmpList.Count; i++)
            {
                var child = tmpList[i];
                child.parent = closestNid;
                child.invertedPathToNearestParent = first.invertedPathToNearestParent;
            }
            
            ListPool<NetworkIdentity>.Destroy(tmpList);

            if (closestNid)
                closestNid.AddDirectChild(first);
            
            if (oldParent && oldParent != closestNid)
                oldParent.RemoveDirectChild(first);

            if (identity.id.HasValue)
            {
                var packet = new ChangeParentPacket
                {
                    sceneId = _sceneId,
                    childId = identity.id.Value,
                    newParentId = closestNid?.id,
                    path = identity.invertedPathToNearestParent
                };

                if (_asServer)
                    _playersManager.Send(identity.observers, packet);
                else _playersManager.SendToServer(packet);
            }
            
            if (_asServer && _scenePlayers.TryGetPlayersInScene(_sceneId, out var players))
            {
                var trs = identity.transform;
                foreach (var player in players)
                    _visibility.RefreshVisibilityForGameObject(player, trs, closestNid);
            }
        }
        
        private readonly Dictionary<SpawnID, DisposableList<NetworkIdentity>> _pendingSpawns = new();

        private void OnFinishSpawnPacket(PlayerID player, FinishSpawnPacket data, bool asServer)
        {
            if (data.sceneId != _sceneId)
                return;

            if (_pendingSpawns.Remove(data.packetIdx, out var list))
            {
                using (list)
                {
                    int count = list.Count;

                    // if server, refresh visibility for all players in scene
                    if (count > 0 && list[0] && _asServer && 
                        _scenePlayers.TryGetPlayersInScene(_sceneId, out var players))
                    {
                        foreach (var playerInScene in players)
                            _visibility.RefreshVisibilityForGameObject(playerInScene, list[0].transform);
                    }

                    bool isHost = IsServerHost();
                    
                    // trigger spawn event
                    for (var i = 0; i < count; i++)
                    {
                        var nid = list[i];
                        if (!nid || !nid.isSpawned) continue;
                        
                        nid.TriggerSpawnEvent(_asServer);
                        if (isHost)
                            nid.TriggerSpawnEvent(false);
                        onIdentityAdded?.Invoke(nid);
                    }
                }
            }
        }

        private void OnPlayerUnloadedScene(PlayerID player, SceneID scene, bool asserver)
        {
            if (!asserver)
                return;
            
            if (scene != _sceneId)
                return;

            var roots = HashSetPool<NetworkIdentity>.Instantiate();
            var count = _spawnedIdentities.Count;

            for (var i = 0; i < count; i++)
            {
                var id = _spawnedIdentities[i];
                
                if (!id) continue;
                
                var root = id.GetRootIdentity();
                
                if (!root || !roots.Add(root))
                    continue;
                
                _visibility.ClearVisibilityForGameObject(root.transform, player);
            }

            HashSetPool<NetworkIdentity>.Destroy(roots);
        }

        private void OnSpawnPacket(PlayerID player, SpawnPacket data, bool asServer)
        {
            if (_asServer && !_manager.networkRules.HasSpawnAuthority(_manager, false))
            {
                PurrLogger.LogError($"Spawn failed from client due to lack of permissions.");
                return;
            }
            
            if (data.sceneId != _sceneId)
                return;
            
            // when in host mode, let the server handle the spawning on their module
            if (!_asServer && _manager.isServer)
                return;
            
            var createdNids = new DisposableList<NetworkIdentity>(16);
            CreatePrototype(data.prototype, createdNids.list);

            if (_asServer)
            {
                foreach (var nid in createdNids)
                {
                    nid.SetIdentity(_manager, this, _sceneId, _asServer);
                    RegisterIdentity(nid, false);
                    nid.TryAddObserver(player);
                    
                    // I think it makes more sense to not trigger this event here
                    // as it makes sense to assume they were already observers before the spawn
                    /*nid.TriggerOnObserverAdded(player);
                    onEarlyObserverAdded?.Invoke(player, nid);*/
                }

                if (createdNids.Count > 0)
                {
                    var lastNid = createdNids[^1];
                    if (lastNid.id.HasValue)
                        _playersManager.RegisterClientLastId(player, lastNid.id.Value);
                }
            }
            else
            {
                foreach (var nid in createdNids)
                {
                    nid.SetIdentity(_manager, this, _sceneId, _asServer);
                    RegisterIdentity(nid, false);
                }
            }

            _pendingSpawns.Add(data.packetIdx, createdNids);
        }

        private void OnDespawnPacket(PlayerID player, DespawnPacket data, bool asServer)
        {
            if (data.sceneId != _sceneId)
                return;
            
            if (!TryGetIdentity(data.parentId, out var identity))
                return;
            
            if (_asServer && !identity.HasDespawnAuthority(player, !_asServer))
            {
                PurrLogger.LogError($"Despawn failed for '{identity.gameObject.name}' due to lack of permissions.", identity.gameObject);
                return;
            }
            
            Despawn(identity.gameObject, true, true);
        }

        public void EvaluateAllVisibilities()
        {
            if (_asServer && _scenePlayers.TryGetPlayersInScene(_sceneId, out var players))
                _visibility.EvaluateAll(players, _spawnedIdentities);
        }

        private void OnPlayerLoadedScene(PlayerID player, SceneID scene, bool asserver)
        {
            if (!_asServer)
                return;
            
            if (scene != _sceneId)
                return;

            var roots = HashSetPool<NetworkIdentity>.Instantiate();
            var count = _spawnedIdentities.Count;

            for (var i = 0; i < count; i++)
            {
                var id = _spawnedIdentities[i];
                
                if (!id) continue;
                
                var root = id.GetRootIdentity();
                
                if (!roots.Add(root))
                    continue;
                
                _visibility.RefreshVisibilityForGameObject(player, root.transform);
            }

            HashSetPool<NetworkIdentity>.Destroy(roots);
        }
        
        private int _nextPacketIdx;

        struct PlayerNid
        {
            public PlayerID player;
            public NetworkIdentity nid;
        }
        
        private readonly List<PlayerNid> _triggerLateObserverAdded = new List<PlayerNid>();
        
        private void OnVisibilityChanged(PlayerID player, Transform scope, bool isVisible)
        {
            if (isVisible)
            {
                var children = ListPool<NetworkIdentity>.Instantiate();
                if (HierarchyPool.TryGetPrototype(scope, player, children, out var prototype))
                {
                    using (prototype)
                    {
                        if (_scenePlayers.IsPlayerLoadedInScene(player, _sceneId))
                            SendSpawnPacket(player, prototype);

                        for (var i = 0; i < children.Count; i++)
                        {
                            var nid = children[i];
                            onObserverAdded?.Invoke(player, nid);
                            _triggerLateObserverAdded.Add(new PlayerNid { player = player, nid = nid });
                        }
                    }
                }
                else PurrLogger.LogError($"Failed to get prototype for '{scope.name}'.", scope);
                
                ListPool<NetworkIdentity>.Destroy(children);
                return;
            }

            if (scope.TryGetComponent<NetworkIdentity>(out var identity))
            {
                var children = ListPool<NetworkIdentity>.Instantiate();
                GetComponentsInChildren(identity.gameObject, children);

                foreach (var child in children)
                {
                    child.TriggerOnObserverRemoved(player);
                    onObserverRemoved?.Invoke(player, child);
                }
                
                ListPool<NetworkIdentity>.Destroy(children);
                
                if (_scenePlayers.IsPlayerLoadedInScene(player, _sceneId))
                    SendDespawnPacket(player, identity);
            }
        }

        private void SendDespawnPacket(PlayerID player, NetworkIdentity identity)
        {
            if (!identity.id.HasValue || !identity.IsSpawned(_asServer))
                return;
            
            var packet = new DespawnPacket
            {
                sceneId = _sceneId,
                parentId = identity.id.Value
            };
            
            if (player.isServer)
                 _playersManager.SendToServer(packet);
            else _playersManager.Send(player, packet);
        }

        private void SendSpawnPacket(PlayerID player, GameObjectPrototype prototype)
        {
            var spawnId = new SpawnID(_nextPacketIdx++, player);
            var packet = new SpawnPacket
            {
                sceneId = _sceneId,
                packetIdx = spawnId,
                prototype = prototype
            };

            if (player.isServer)
                 _playersManager.SendToServer(packet);
            else _playersManager.Send(player, packet);

            _toCompleteNextFrame.Add(spawnId);
        }

        private void OnGameObjectCreated(GameObject obj, GameObject prefab)
        {
            if (!obj)
                return;
            
            if (!_asServer && _manager.isServer)
                return;

            if (obj.scene.handle != _scene.handle)
                return;

            if (!_manager.TryGetPrefabData(prefab, out var data, out var idx))
                return;
            
            NetworkManager.SetupPrefabInfo(obj, idx, data.pooled);

            Spawn(obj);
        }

        public void Spawn(GameObject gameObject)
        {
            if (!gameObject)
                return;
            
            if (!gameObject.TryGetComponent<NetworkIdentity>(out var id))
            {
                PurrLogger.LogError($"Failed to spawn object '{gameObject.name}'. No NetworkIdentity found.", gameObject);
                return;
            }

            if (id.isSpawned)
                return;
            
            if (!id.HasSpawnAuthority(_manager, _asServer))
            {
                PurrLogger.LogError($"Spawn failed from for '{gameObject.name}' due to lack of permissions.", gameObject);
                return;
            }

            PlayerID scope = default;
            
            if (!_asServer)
            {
                if (!_playersManager.localPlayerId.HasValue)
                {
                    PurrLogger.LogError($"Failed to spawn object '{gameObject.name}'. No local player id found.", gameObject);
                    return;
                }
                
                scope = _playersManager.localPlayerId.Value;
            }
            
            
            var baseNid = new NetworkID(_nextId++, scope);
            SetupIdsLocally(id, ref baseNid);
            ApplyParentChange(id, id.parent, id.invertedPathToNearestParent, false);

            if (!_asServer)
            {
                SendSpawnPacket(default, HierarchyPool.GetFullPrototype(gameObject.transform));
            }
            else if (_scenePlayers.TryGetPlayersInScene(_sceneId, out var players))
            {
                foreach (var player in players)
                    _visibility.RefreshVisibilityForGameObject(player, gameObject.transform);
            }
            AutoAssignOwnership(id);
        }

        /*private void TriggerObserverEvents()
        {
            if (_triggerObserverAdded.Count > 0)
            {
                foreach (var nid in _triggerObserverAdded)
                {
                    nid.nid.TriggerOnObserverAdded(nid.player);
                    onObserverAdded?.Invoke(nid.player, nid.nid);
                }
                _triggerObserverAdded.Clear();
            }
        }*/

        private void AutoAssignOwnership(NetworkIdentity id)
        {
            if (!id.ShouldClientGiveOwnershipOnSpawn(_manager))
                return;
            
            PlayersManager playersManager;
            
            switch (_asServer)
            {
                case true when _manager.isClient:
                    playersManager = _manager.GetModule<PlayersManager>(false);
                    break;
                case false:
                    playersManager = _playersManager;
                    break;
                default:
                    return;
            }

            if (playersManager.localPlayerId.HasValue)
                id.GiveOwnership(playersManager.localPlayerId.Value);
        }

        public static void GetComponentsInChildren(GameObject go, List<NetworkIdentity> list)
        {
            // workaround for the fact that GetComponents clears the list
            var tmpList = ListPool<NetworkIdentity>.Instantiate();
            int startIdx = list.Count;
            go.GetComponents(tmpList);
            list.AddRange(tmpList);
            ListPool<NetworkIdentity>.Destroy(tmpList);
            
            if (list.Count <= startIdx)
                return;
            
            var identity = list[startIdx];
            var children = identity.directChildren;
            var dcount = children.Count;
            
            for (int j = 0; j < dcount; j++)
                GetComponentsInChildren(children[j].gameObject, list);
        }
        
        public void Despawn(GameObject gameObject, bool bypassPermissions = false, bool bypassBroadcast = false)
        {
            var children = ListPool<NetworkIdentity>.Instantiate();
            GetComponentsInChildren(gameObject, children);

            if (children.Count == 0)
            {
                ListPool<NetworkIdentity>.Destroy(children);
                return;
            }
            
            using var directChildren = new DisposableList<TransformIdentityPair>(16);
            HierarchyPool.GetDirectChildrenWithRoot(gameObject.transform, directChildren);

            foreach (var idPair in directChildren)
            {
                var p = idPair.identity.parent;
                if (p) p.RemoveDirectChild(idPair.identity);
            }
            
            if (!bypassPermissions && !children[0].HasDespawnAuthority(_playersManager?.localPlayerId ?? default, _asServer))
            {
                PurrLogger.LogError($"Despawn failed for '{gameObject.name}' due to lack of permissions.", gameObject);
                ListPool<NetworkIdentity>.Destroy(children);
                return;
            }

            if (_asServer)
                _visibility.ClearVisibilityForGameObject(gameObject.transform);
            else if (!bypassBroadcast)
                SendDespawnPacket(default, children[0]);
            
            for (var i = 0; i < children.Count; i++)
            {
                var child = children[i];
                
                UnregisterIdentity(child);
                
                if (child.shouldBePooled)
                    child.ResetIdentity();
            }

            var pair = new PoolPair(_scenePool, _prefabsPool);
            HierarchyPool.PutBackInPool(pair, gameObject);
            
            ListPool<NetworkIdentity>.Destroy(children);
        }

        private void SetupIdsLocally(NetworkIdentity root, ref NetworkID baseNid)
        {
            using var siblings = new DisposableList<NetworkIdentity>(16);
            root.GetComponents(siblings.list);
            
            // handle root
            for (var i = 0; i < siblings.Count; i++)
            {
                var sibling = siblings[i];
                sibling.SetID(new NetworkID(baseNid, (uint)i));
                sibling.SetIdentity(_manager, this, _sceneId, _asServer);
                RegisterIdentity(sibling, true);
            }

            // update next id
            _nextId += (uint)siblings.list.Count;
            baseNid = new NetworkID(_nextId, baseNid.scope);
            
            // handle children
            if (root.directChildren == null)
                return;
            
            for (var i = 0; i < root.directChildren.Count; i++)
            {
                SetupIdsLocally(root.directChildren[i], ref baseNid);
            }
        }
        
        private void SpawnSceneObject(List<NetworkIdentity> children)
        {
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                if (child.isSceneObject)
                {
                    var id = new NetworkID(default, _nextId++);
                    child.SetID(id);
                    if (_asServer)
                    {
                        child.SetIdentity(_manager, this, _sceneId, _asServer);
                        RegisterIdentity(child, true);
                    }
                }
            }
        }
        
        public void PreNetworkMessages()
        {
            SendDelayedObserverEvents();
            SendDelayedCompleteSpawns();
        }
        
        public void PostNetworkMessages()
        {
            SpawnDelayedIdentities();
        }
        
        private void SendDelayedObserverEvents()
        {
            for (var i = 0; i < _triggerLateObserverAdded.Count; i++)
            {
                var nid = _triggerLateObserverAdded[i];
                nid.nid.TriggerOnObserverAdded(nid.player);
            }
            
            _triggerLateObserverAdded.Clear();
        }

        private void SendDelayedCompleteSpawns()
        {
            for (var i = 0; i < _toCompleteNextFrame.Count; i++)
            {
                var toComplete = _toCompleteNextFrame[i];
                var packet = new FinishSpawnPacket
                {
                    sceneId = _sceneId,
                    packetIdx = toComplete
                };
                
                if (_asServer)
                     _playersManager.Send(toComplete.player, packet);
                else _playersManager.SendToServer(packet);
            }
            
            _toCompleteNextFrame.Clear();
        }

        private void CatchupClient()
        {
            for (var i = 0; i < _spawnedIdentities.Count; i++)
            {
                var identity = _spawnedIdentities[i];
                
                if (!identity.isSpawned)
                    continue;
                
                if (_toSpawnNextFrame.Contains(identity))
                    continue;
                
                identity.SetIsSpawned(true, false);
                identity.TriggerSpawnEvent(false);
                onIdentityAdded?.Invoke(identity);
            }
        }

        private bool IsServerHost()
        {
            if (!_asServer)
                return false;
            
            if (_manager.TryGetModule<HierarchyFactory>(false, out var factory) &&
                factory.TryGetHierarchy(_sceneId, out var other))
            {
                return other._isPlayerReady;
            }
            
            return false;
        }

        private void SpawnDelayedIdentities()
        {
            bool isHost = IsServerHost();

            // swap buffers to avoid editing while iterating
            var actual = _toSpawnNextFrame;
            _toSpawnNextFrame = _toSpawnNextFrameBuffer;
            _toSpawnNextFrameBuffer = actual;
            
            // trigger spawn events
            foreach (var toSpawn in actual)
            {
                if (!toSpawn || !toSpawn.isSpawned) continue;

                toSpawn.TriggerSpawnEvent(_asServer);

                if (isHost)
                    toSpawn.TriggerSpawnEvent(false);
                
                onIdentityAdded?.Invoke(toSpawn);
            }

            actual.Clear();
        }

        public GameObject CreatePrototype(GameObjectPrototype prototype, List<NetworkIdentity> createdNids)
        {
            var pair = new PoolPair(_scenePool, _prefabsPool);

            if (!HierarchyPool.TryBuildPrototype(pair, prototype, createdNids, out var result, out var shouldActivate))
                return null;
            
            var resultTrs = result.transform;
            result.transform.SetParent(null, false);
            SceneManager.MoveGameObjectToScene(result, _scene);

            if (prototype.parentID.HasValue)
            {
                if (TryGetIdentity(prototype.parentID.Value, out var parent))
                {
                    result.transform.SetParent(parent.transform, false);
                    resultTrs.SetLocalPositionAndRotation(prototype.position, prototype.rotation);

                    if (result.TryGetComponent<NetworkIdentity>(out var nid))
                        ApplyParentChange(nid, parent, prototype.path, false);
                }
                else
                {
                    PurrLogger.LogError($"Failed to find parent for '{result.name}' with id '{prototype.parentID}'.", result);
                }
            }
            else if (prototype.defaultParentSiblingIndex.HasValue && result.TryGetComponent<NetworkIdentity>(out var nid) && nid.defaultParent)
            {
                result.transform.SetParent(nid.defaultParent, false);
                result.transform.SetSiblingIndex(prototype.defaultParentSiblingIndex.Value);
                resultTrs.SetLocalPositionAndRotation(prototype.position, prototype.rotation);
            }
            else
            {
                resultTrs.SetLocalPositionAndRotation(prototype.position, prototype.rotation);
            }
            
            if (shouldActivate && !result.activeSelf)
                result.SetActive(true);

            return result;
        }
        
        HashSet<NetworkIdentity> _toSpawnNextFrame = new HashSet<NetworkIdentity>();
        HashSet<NetworkIdentity> _toSpawnNextFrameBuffer = new HashSet<NetworkIdentity>();
        
        readonly List<SpawnID> _toCompleteNextFrame = new List<SpawnID>();

        /// <summary>
        /// Local spawn will trigger the spawn event next frame immediately after the identity is registered.
        /// </summary>
        private void RegisterIdentity(NetworkIdentity identity, bool isLocalSpawn)
        {
            if (identity.id.HasValue)
            {
                _spawnedIdentities.Add(identity);
                _spawnedIdentitiesMap.Add(identity.id.Value, identity);
                
                identity.TriggerEarlySpawnEvent(_asServer);
                if (_asServer && _manager.isClient) 
                    identity.TriggerEarlySpawnEvent(false);
                
                onEarlyIdentityAdded?.Invoke(identity);
                
                if (isLocalSpawn)
                    _toSpawnNextFrame.Add(identity);
            }
        }
        
        private void UnregisterIdentity(NetworkIdentity identity)
        {
            if (identity.id.HasValue)
            {
                _spawnedIdentities.Remove(identity);
                _spawnedIdentitiesMap.Remove(identity.id.Value);

                if (_asServer && IsServerHost())
                    identity.TriggerDespawnEvent(false);
                identity.TriggerDespawnEvent(_asServer);
                
                onIdentityRemoved?.Invoke(identity);
            }
        }
        
        public bool TryGetIdentity(NetworkID id, out NetworkIdentity identity)
        {
            if (_spawnedIdentitiesMap.TryGetValue(id, out identity))
                return identity;
            
            if (!_asServer && _manager.isServer)
            {
                if (_manager.TryGetModule<HierarchyFactory>(true, out var factory) &&
                    factory.TryGetHierarchy(_sceneId, out var other))
                {
                    return other.TryGetIdentity(id, out identity);
                }
            }
            
            return false;
        }
    }
}