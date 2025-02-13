using System;
using System.Collections.Generic;
using System.Security.Principal;
using PurrNet.Logging;
using PurrNet.Packing;
using PurrNet.Pooling;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PurrNet.Modules
{
    internal struct OwnershipInfo 
    {
        public NetworkID identity;
        public PlayerID player;
    }
    
    internal struct FullOwnershipChange
    {
        public PlayerID actor;
        public OwnershipChange data;
    }
    
    internal struct OwnershipChangeBatch 
    {
        public SceneID scene;
        public List<OwnershipInfo> state;
    }
    
    internal struct OwnershipChange : IPackedSimple
    {
        public SceneID sceneId;
        public List<NetworkID> identities;
        public bool isAdding;
        public PlayerID player;

        public void Serialize(BitPacker packer)
        {
            Packer<SceneID>.Serialize(packer, ref sceneId);
            Packer<List<NetworkID>>.Serialize(packer, ref identities);
            Packer<bool>.Serialize(packer, ref isAdding);

            if (isAdding)
                Packer<PlayerID>.Serialize(packer, ref player);
        }
    }
    
    public class GlobalOwnershipModule : INetworkModule, IFixedUpdate, IPreFixedUpdate
    {
        readonly PlayersManager _playersManager;
        readonly ScenePlayersModule _scenePlayers;
        readonly HierarchyFactory _hierarchy;
        
        readonly ScenesModule _scenes;
        readonly Dictionary<SceneID, SceneOwnership> _sceneOwnerships = new Dictionary<SceneID, SceneOwnership>();
        
        private bool _asServer;
        
        public GlobalOwnershipModule(HierarchyFactory hierarchy,
            PlayersManager players, ScenePlayersModule scenePlayers, ScenesModule scenes)
        {
            _hierarchy = hierarchy;
            _scenes = scenes;
            _playersManager = players;
            _scenePlayers = scenePlayers;
        }
        
        public void Enable(bool asServer)
        {
            _asServer = asServer;
            
            var scenes = _scenes.sceneStates;

            foreach (var (id, sceneState) in scenes)
            {
                if (sceneState.scene.isLoaded)
                    OnSceneLoaded(id, asServer);
            }
            
            _scenes.onPreSceneLoaded += OnSceneLoaded;
            _scenes.onSceneUnloaded += OnSceneUnloaded;
            
            _hierarchy.onIdentityRemoved += OnIdentityDespawned;
            _hierarchy.onEarlyObserverAdded += OnPlayerObserverAdded;

            _scenePlayers.onPlayerUnloadedScene += OnPlayerUnloadedScene;
            _scenePlayers.onPlayerLoadedScene += OnPlayerLoadedScene;
            
            _playersManager.onPlayerLeft += OnPlayerLeft;
            
            _playersManager.Subscribe<OwnershipChangeBatch>(OnOwnershipChange);
            _playersManager.Subscribe<OwnershipChange>(OnOwnershipChange);
        }

        public void Disable(bool asServer)
        {
            _scenes.onPreSceneLoaded -= OnSceneLoaded;
            _scenes.onSceneUnloaded -= OnSceneUnloaded;
            
            _hierarchy.onIdentityRemoved -= OnIdentityDespawned;
            _hierarchy.onEarlyObserverAdded -= OnPlayerObserverAdded;

            _scenePlayers.onPlayerUnloadedScene -= OnPlayerUnloadedScene;
            _scenePlayers.onPlayerLoadedScene -= OnPlayerLoadedScene;

            _playersManager.onPlayerLeft -= OnPlayerLeft;

            _playersManager.Unsubscribe<OwnershipChangeBatch>(OnOwnershipChange);
            _playersManager.Unsubscribe<OwnershipChange>(OnOwnershipChange);
        }

        /// <summary>
        /// Gets all the objects owned by the given player.
        /// This creates a new list every time it's called.
        /// So it's recommended to cache the result if you're going to use it multiple times.
        /// </summary>
        public List<NetworkIdentity> GetAllPlayerOwnedIds(PlayerID player)
        {
            List<NetworkIdentity> ids = new List<NetworkIdentity>();

            foreach (var (scene, owned) in _sceneOwnerships)
            {
                if (!_hierarchy.TryGetHierarchy(scene, out var hierarchy))
                    continue;

                var ownedIds = owned.TryGetOwnedObjects(player);
                foreach (var id in ownedIds)
                {
                    if (hierarchy.TryGetIdentity(id, out var identity))
                        ids.Add(identity);
                }
            }
            
            return ids;
        }
        
        public bool PlayerOwnsSomething(PlayerID player)
        {
            foreach (var (_, owned) in _sceneOwnerships)
            {
                var ownedIds = owned.TryGetOwnedObjects(player);
                if (ownedIds.Count > 0)
                    return true;
            }
            
            return false;
        }
        
        public void GetAllPlayerOwnedIds(PlayerID player, List<NetworkIdentity> ids)
        {
            foreach (var (scene, owned) in _sceneOwnerships)
            {
                if (!_hierarchy.TryGetHierarchy(scene, out var hierarchy))
                    continue;

                var ownedIds = owned.TryGetOwnedObjects(player);
                foreach (var id in ownedIds)
                {
                    if (hierarchy.TryGetIdentity(id, out var identity))
                        ids.Add(identity);
                }
            }
        }
        
        public IEnumerable<NetworkIdentity> EnumerateAllPlayerOwnedIds(PlayerID player)
        {
            foreach (var (scene, owned) in _sceneOwnerships)
            {
                if (!_hierarchy.TryGetHierarchy(scene, out var hierarchy))
                    continue;

                var ownedIds = owned.TryGetOwnedObjects(player);
                foreach (var id in ownedIds)
                {
                    if (hierarchy.TryGetIdentity(id, out var identity))
                        yield return identity;
                }
            }
        }

        private void OnIdentityDespawned(NetworkIdentity identity)
        {
            if (!identity.id.HasValue)
                return;
            
            if (_sceneOwnerships.TryGetValue(identity.sceneId, out var module))
            {
                if (module.TryGetOwner(identity, out var oldOwner) && module.RemoveOwnership(identity))
                    identity.TriggerOnOwnerChanged(oldOwner, null, _asServer);
            }
        }

        struct PlayerSceneID : IEquatable<PlayerSceneID>
        {
            public PlayerID player;
            public SceneID scene;

            public bool Equals(PlayerSceneID other)
            {
                return player.Equals(other.player) && scene.Equals(other.scene);
            }

            public override bool Equals(object obj)
            {
                return obj is PlayerSceneID other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(player, scene);
            }
        }

        readonly Dictionary<PlayerSceneID, DisposableList<OwnershipInfo>> _pendingOwnershipChanges = new Dictionary<PlayerSceneID, DisposableList<OwnershipInfo>>();

        private void OnPlayerObserverAdded(PlayerID player, NetworkIdentity target)
        {
            if (!target.id.HasValue)
                return;

            if (!_sceneOwnerships.TryGetValue(target.sceneId, out var ownerships))
                return;

            if (!_asServer)
                return;

            if (!ownerships.TryGetOwner(target, out var owner))
                return;
            
            var info = new OwnershipInfo
            {
                identity = target.id.Value,
                player = owner
            };
                
            var key = new PlayerSceneID
            {
                player = player,
                scene = target.sceneId
            };

            if (_pendingOwnershipChanges.TryGetValue(key, out var list))
            {
                list.Add(info);
            }
            else
            {
                list = new DisposableList<OwnershipInfo>(16);
                list.Add(info);
                _pendingOwnershipChanges.Add(key, list);
            }
        }
        
        private void OnPlayerLoadedScene(PlayerID player, SceneID scene, bool asServer)
        {
            if (!_sceneOwnerships.TryGetValue(scene, out var ownerships)) return;

            var owned = ownerships.TryGetOwnedObjects(player);

            foreach (var id in owned)
            {
                if (_hierarchy.TryGetIdentity(scene, id, out var identity))
                    identity.TriggerOnOwnerReconnected(player, asServer);
            }
        }
        
        private void OnPlayerLeft(PlayerID player, bool asServer)
        {
            foreach (var (scene, ownerships) in _sceneOwnerships)
                OnOwnerDisconnect(player, scene, ownerships);
        }

        private void OnPlayerUnloadedScene(PlayerID player, SceneID scene, bool asServer)
        {
            if (!_sceneOwnerships.TryGetValue(scene, out var ownerships)) return;

            OnOwnerDisconnect(player, scene, ownerships);
            
            var owned = ownerships.TryGetOwnedObjects(player);
            var ownedCache = ListPool<NetworkID>.Instantiate();
            ownedCache.AddRange(owned);

            for (var i = 0; i < ownedCache.Count; i++)
            {
                var id = ownedCache[i];
                if (_hierarchy.TryGetIdentity(scene, id, out var identity))
                    identity.TriggerOnOwnerDisconnected(player);
            }
            ListPool<NetworkID>.Destroy(ownedCache);
        }

        private void OnOwnerDisconnect(PlayerID player, SceneID scene, SceneOwnership ownerships)
        {
            var owned = ownerships.TryGetOwnedObjects(player);
            var toDestroy = HashSetPool<GameObject>.Instantiate();
            
            foreach (var id in owned)
            {
                if (_hierarchy.TryGetIdentity(scene, id, out var identity))
                {
                    bool shouldDespawn = identity.ShouldDespawnOnOwnerDisconnect();

                    if (shouldDespawn && !identity.isSceneObject)
                        toDestroy.Add(identity.gameObject);
                }
            }
            
            foreach (var go in toDestroy)
                if (go) Object.Destroy(go);
            
            HashSetPool<GameObject>.Destroy(toDestroy);
        }

        private void OnOwnershipChange(PlayerID player, OwnershipChangeBatch data, bool asServer)
        {
            HandleOwenshipBatch(data);
        }

        private void OnOwnershipChange(PlayerID player, OwnershipChange change, bool asServer)
        {
            var idCount = change.identities.Count;
                
            for (var j = 0; j < idCount; j++)
                HandleOwnershipChange(player, change, change.identities[j]);
        }
        
        private void OnSceneUnloaded(SceneID scene, bool asServer)
        {
            _sceneOwnerships.Remove(scene);
        }

        private static readonly List<NetworkID> _idsCache = new List<NetworkID>();

        public void GiveOwnership(NetworkIdentity nid, PlayerID player, bool? propagateToChildren = null, bool? overrideExistingOwners = null, bool silent = false)
        {
            if (!nid.id.HasValue)
            {
                if (!silent)
                    PurrLogger.LogError($"Failed to give ownership of '{nid.gameObject.name}' to {player} because it isn't spawned.");
                return;
            }
            
            bool hadOwnerPreviously = nid.hasOwner;
            
            switch (hadOwnerPreviously)
            {
                case true when nid.owner == player:
                {
                    return;
                }
                case true when !nid.HasTransferOwnershipAuthority(_asServer):
                case false when !nid.HasGiveOwnershipAuthority(_asServer):
                {
                    if (!silent)
                        PurrLogger.LogError($"Failed to give ownership of '{nid.gameObject.name}' to {player} because of missing authority.");
                    return;
                }
            }

            if (!_sceneOwnerships.TryGetValue(nid.sceneId, out var module))
            {
                if (!silent)
                    PurrLogger.LogError($"No ownership module avaible for scene {nid.sceneId} '{nid.gameObject.scene.name}'");
                return;
            }
            
            var shouldOverride = overrideExistingOwners ?? nid.ShouldOverrideExistingOwnership(_asServer);
            var affectedIds = ListPool<NetworkIdentity>.Instantiate();
            GetAllChildrenOrSelf(nid, affectedIds, propagateToChildren);

            _idsCache.Clear();

            for (var i = 0; i < affectedIds.Count; i++)
            {
                var identity = affectedIds[i];
                
                if (!identity.id.HasValue) continue;

                if (identity.hasOwner)
                {
                    if (!shouldOverride)
                        continue;

                    if (!identity.HasTransferOwnershipAuthority(_asServer))
                    {
                        if (!silent)
                            PurrLogger.LogError($"Failed to override ownership of '{identity.gameObject.name}' because of missing authority.");
                        continue;
                    }
                }

                var oldOwner = identity.GetOwner(_asServer);

                if (module.GiveOwnership(identity, player))
                    identity.TriggerOnOwnerChanged(oldOwner, player, _asServer);

                _idsCache.Add(identity.id.Value);
            }

            if (_idsCache.Count == 0)
            {
                if (!silent)
                    PurrLogger.LogError($"Failed to give ownership of '{nid.gameObject.name}' to {player} because no identities were affected.");
                
                ListPool<NetworkIdentity>.Destroy(affectedIds);
                return;
            }

            // TODO: compress _idsCache using RLE
            var data = new OwnershipChange
            {
                sceneId = nid.sceneId,
                identities = _idsCache,
                isAdding = true,
                player = player
            };

            if (_asServer)
            {
                if (_scenePlayers.TryGetPlayersInScene(nid.sceneId, out var players))
                    _playersManager.Send(players, data);
            }
            else
            {
                _playersManager.SendToServer(data);
            }
            
            ListPool<NetworkIdentity>.Destroy(affectedIds);
        }

        /// <summary>
        /// Clears all ownerships of the given identity and its children.
        /// </summary>
        public void ClearOwnerships(NetworkIdentity id, bool supressErrorMessages = false)
        {
            if (!id.id.HasValue)
            {
                PurrLogger.LogError($"Failed to remove ownership of '{id.gameObject.name}' because it isn't spawned.");
                return;
            }
            
            if (!id.owner.HasValue)
                return;
            
            if (!id.HasTransferOwnershipAuthority(_asServer))
            {
                PurrLogger.LogError($"Failed to remove ownership of '{id.gameObject.name}' because of missing authority.");
                return;
            }

            if (!_sceneOwnerships.TryGetValue(id.sceneId, out var module))
            {
                PurrLogger.LogError($"No ownership module avaible for scene {id.sceneId} '{id.gameObject.scene.name}'");
                return;
            }
            
            var children = ListPool<NetworkIdentity>.Instantiate();
            GetAllChildrenOrSelf(id, children, true);
            
            _idsCache.Clear();

            for (var i = 0; i < children.Count; i++)
            {
                var identity = children[i];
                
                if (!identity.id.HasValue) continue;
                if (!identity.hasOwner) continue;
                if (!identity.HasTransferOwnershipAuthority(_asServer))
                {
                    if (!supressErrorMessages)
                        PurrLogger.LogError($"Failed to override ownership of '{identity.gameObject.name}' because of missing authority.");
                    continue;
                }

                _idsCache.Add(identity.id.Value);
            }

            //TODO: compress _idsCache using RLE
            var data = new OwnershipChange
            {
                sceneId = id.sceneId,
                identities = _idsCache,
                isAdding = false,
                player = default
            };

            var oldOwner = id.GetOwner(_asServer);

            if (module.RemoveOwnership(id))
                id.TriggerOnOwnerChanged(oldOwner, null, _asServer);

            if (_asServer)
            {
                if (_scenePlayers.TryGetPlayersInScene(id.sceneId, out var players))
                    _playersManager.Send(players, data);
            }
            else _playersManager.SendToServer(data);
            
            ListPool<NetworkIdentity>.Destroy(children);
        }
        
        /// <summary>
        /// Only removes ownership for the existing owner.
        /// This won't remove ownership of children with different owners.
        /// </summary>
        public void RemoveOwnership(NetworkIdentity id, bool? propagateToChildren = null, bool supressErrorMessages = false)
        {
            if (!id.id.HasValue)
            {
                PurrLogger.LogError($"Failed to remove ownership of '{id.gameObject.name}' because it isn't spawned.");
                return;
            }
            
            if (!id.owner.HasValue)
                return;
            
            if (!id.HasTransferOwnershipAuthority(_asServer))
            {
                PurrLogger.LogError($"Failed to remove ownership of '{id.gameObject.name}' because of missing authority.");
                return;
            }

            if (!_sceneOwnerships.TryGetValue(id.sceneId, out var module))
            {
                PurrLogger.LogError($"No ownership module avaible for scene {id.sceneId} '{id.gameObject.scene.name}'");
                return;
            }
            
            var originalOwner = id.owner.Value;
            var children = ListPool<NetworkIdentity>.Instantiate();
            GetAllChildrenOrSelf(id, children, propagateToChildren);
            
            _idsCache.Clear();

            for (var i = 0; i < children.Count; i++)
            {
                var identity = children[i];
                
                if (!identity.id.HasValue) continue;
                if (!module.TryGetOwner(identity, out var player) || player != originalOwner) continue;
                if (!identity.HasTransferOwnershipAuthority(_asServer))
                {
                    if (!supressErrorMessages)
                        PurrLogger.LogError($"Failed to override ownership of '{identity.gameObject.name}' because of missing authority.");
                    continue;
                }
                    
                _idsCache.Add(identity.id.Value);
            }

            //TODO: compress _idsCache using RLE
            var data = new OwnershipChange
            {
                sceneId = id.sceneId,
                identities = _idsCache,
                isAdding = false,
                player = default
            };

            var oldOwner = id.GetOwner(_asServer);

            if (module.RemoveOwnership(id))
                id.TriggerOnOwnerChanged(oldOwner, id.owner, _asServer);

            if (_asServer)
            {
                if (_scenePlayers.TryGetPlayersInScene(id.sceneId, out var players))
                    _playersManager.Send(players, data);
            }
            else _playersManager.SendToServer(data);
            
            ListPool<NetworkIdentity>.Destroy(children);
        }

        public bool TryGetOwner(NetworkIdentity id, out PlayerID player)
        {
            if (_sceneOwnerships.TryGetValue(id.sceneId, out var module) && module.TryGetOwner(id, out player))
                return true;
            
            player = default;
            return false;
        }

        private void OnSceneLoaded(SceneID scene, bool asServer)
        {
            _sceneOwnerships[scene] = new SceneOwnership(asServer);
        }
        
        private void HandlePendingChanges()
        {
            if (_pendingOwnershipChanges.Count == 0)
                return;
            
            foreach (var (player, changes) in _pendingOwnershipChanges)
            {
                _playersManager.Send(player.player, new OwnershipChangeBatch
                {
                    scene = player.scene,
                    state = changes.list
                });
                
                changes.Dispose();
            }

            _pendingOwnershipChanges.Clear();
        }

        private void HandleOwenshipBatch(OwnershipChangeBatch data)
        {
            var stateCount = data.state.Count;

            for (var j = 0; j < stateCount; j++)
                HandleOwnershipBatch(data.scene, data.state[j]);
        }

        private void HandleOwnershipBatch(SceneID scene, OwnershipInfo change)
        {
            if (!_hierarchy.TryGetIdentity(scene, change.identity, out var identity))
                return;

            if (!identity.id.HasValue)
                return;

            if (!identity.HasGiveOwnershipAuthority(!_asServer))
            {
                PurrLogger.LogError(
                    $"Failed to give ownership of '{identity.gameObject.name}' to {change.player} because of missing authority.");
                return;
            }

            if (!_sceneOwnerships.TryGetValue(scene, out var module))
            {
                PurrLogger.LogError(
                    $"Failed to find ownership module for scene {scene} when applying ownership change for identity {change.identity}");
                return;
            }

            var oldOwner = identity.GetOwner(_asServer);

            if (oldOwner == change.player)
                return;

            if (module.GiveOwnership(identity, change.player))
                identity.TriggerOnOwnerChanged(oldOwner, change.player, _asServer);
        }

        private void HandleOwnershipChange(PlayerID actor, OwnershipChange change, NetworkID id)
        {
            string verb = change.isAdding ? "give" : "remove";
            
            if (!_hierarchy.TryGetIdentity(change.sceneId, id, out var identity))
                return;

            if (!_sceneOwnerships.TryGetValue(change.sceneId, out var module))
            {
                PurrLogger.LogError(
                    $"Failed to find ownership module for scene {change.sceneId} when applying ownership change for identity {id}");
                return;
            }

            if (identity.hasOwner)
            {
                if (!identity.HasTransferOwnershipAuthority(actor, !_asServer))
                {
                    PurrLogger.LogError(
                        $"Failed to {verb} (transfer) ownership of '{identity.gameObject.name}' to {change.player} because of missing authority.", identity);
                    return;
                }
            }
            else if (!identity.HasGiveOwnershipAuthority(!_asServer))
            {
                PurrLogger.LogError(
                    $"Failed to {verb} ownership of '{identity.gameObject.name}' to {change.player} because of missing authority.", identity);
                return;
            }

            var oldOwner = identity.GetOwner(_asServer);

            if (change.isAdding)
            {
                module.GiveOwnership(identity, change.player);

                if (oldOwner != change.player)
                    identity.TriggerOnOwnerChanged(oldOwner, change.player, _asServer);
            }
            else
            {
                if (!identity.HasRemoveOwnershipAuthority(actor, !_asServer))
                {
                    PurrLogger.LogError(
                        $"Failed to remove ownership of '{identity.gameObject.name}' to {change.player} because of missing authority.", identity);
                }
                else if (module.RemoveOwnership(identity) && identity.id.HasValue)
                {
                    identity.TriggerOnOwnerChanged(oldOwner, null, _asServer);
                }
            }
        }

        static void GetAllChildrenOrSelf(NetworkIdentity id, List<NetworkIdentity> result, bool? propagateToChildren)
        {
            bool shouldPropagate = propagateToChildren ?? id.ShouldPropagateToChildren();

            if (shouldPropagate && id.HasPropagateOwnershipAuthority())
            {
                HierarchyV2.GetComponentsInChildren(id.gameObject, result);
            }
            else
            {
                if (propagateToChildren == true)
                    PurrLogger.LogError(
                        $"Failed to propagate ownership of '{id.gameObject.name}' because of missing authority, assigning only to the identity.");

                result.Add(id);
            }
        }

        public void FixedUpdate()
        {
            HandlePendingChanges();
        }

        public void PreFixedUpdate()
        {
            HandlePendingChanges();
        }
    }

    internal class SceneOwnership
    {
        static readonly List<OwnershipInfo> _cache = new List<OwnershipInfo>();
        
        readonly Dictionary<NetworkID, PlayerID> _owners = new Dictionary<NetworkID, PlayerID>();

        readonly Dictionary<PlayerID, HashSet<NetworkID>> _playerOwnedIds = new Dictionary<PlayerID, HashSet<NetworkID>>();

        private readonly bool _asServer;
        
        public SceneOwnership(bool asServer)
        {
            _asServer = asServer;
        }
        
        public List<OwnershipInfo> GetState()
        {
            _cache.Clear();
            
            foreach (var (id, player) in _owners)
                _cache.Add(new OwnershipInfo { identity = id, player = player });

            return _cache;
        }

        public ICollection<NetworkID> TryGetOwnedObjects(PlayerID player)
        {
            if (_playerOwnedIds.TryGetValue(player, out _))
                return _playerOwnedIds[player];
            return Array.Empty<NetworkID>();
        }
        
        public bool TryGetOwner(NetworkIdentity id, out PlayerID player)
        {
            if (!id.id.HasValue)
            {
                player = default;
                return false;
            }
            
            return _owners.TryGetValue(id.id.Value, out player);
        }

        public bool GiveOwnership(NetworkIdentity identity, PlayerID player)
        {
            if (identity.id == null)
                return false;
            
            _owners[identity.id.Value] = player;

            if (!_playerOwnedIds.TryGetValue(player, out var ownedIds))
            {
                ownedIds = new HashSet<NetworkID> { identity.id.Value };
                _playerOwnedIds[player] = ownedIds;
            }
            else ownedIds.Add(identity.id.Value);
            
            if (_asServer)
                 identity.internalOwnerServer = player;
            else identity.internalOwnerClient = player;

            return true;
        }

        public bool RemoveOwnership(NetworkIdentity identity)
        {
            if (identity.id.HasValue && _owners.Remove(identity.id.Value, out var oldOwner))
            {
                if (_playerOwnedIds.TryGetValue(oldOwner, out HashSet<NetworkID> ownedIds))
                {
                    ownedIds.Remove(identity.id.Value);

                    if (ownedIds.Count == 0)
                        _playerOwnedIds.Remove(oldOwner);
                }

                if (_asServer)
                     identity.internalOwnerServer = null;
                else identity.internalOwnerClient = null;
                return true;
            }
            
            return false;
        }
    }
}
