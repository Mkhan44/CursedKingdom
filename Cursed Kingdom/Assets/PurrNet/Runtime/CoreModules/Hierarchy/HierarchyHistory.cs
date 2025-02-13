using System;
using System.Collections.Generic;
using PurrNet.Logging;
using PurrNet.Pooling;

namespace PurrNet.Modules
{
    public class HierarchyHistory
    {
        readonly List<HierarchyAction> _actions = new List<HierarchyAction>();
        readonly List<HierarchyAction> _pending = new List<HierarchyAction>();
        
        readonly SceneID _sceneId;
        
        static readonly List<Prefab> _prefabs = new List<Prefab>();
        static readonly List<int> _toRemove = new List<int>();
        
        public bool hasUnflushedActions { get; private set; }

        public HierarchyHistory(SceneID sceneId)
        {
            _sceneId = sceneId;
        }
        
        internal HierarchyActionBatch GetFullHistory()
        {
            return new HierarchyActionBatch
            {
                sceneId = _sceneId,
                actions = _actions
            };
        }
        
        internal HierarchyActionBatch GetHistoryThatAffects(List<NetworkIdentity> roots)
        {
            var actions = new List<HierarchyAction>();

            var relevant = HashSetPool<NetworkID>.Instantiate();
            var spawned = HashSetPool<NetworkID>.Instantiate();
            var tmp = ListPool<NetworkIdentity>.Instantiate();

            for (var rootIdx = 0; rootIdx < roots.Count; rootIdx++)
            {
                var rootIdentity = roots[rootIdx];
                
                if (!rootIdentity) continue;
                
                rootIdentity.GetComponentsInChildren(true, tmp);

                for (var index = 0; index < tmp.Count; index++)
                {
                    var nid = tmp[index];
                    if (nid.id.HasValue)
                        relevant.Add(nid.id.Value);
                }
            }

            ListPool<NetworkIdentity>.Destroy(tmp);
            
            for (var i = 0; i < _actions.Count; ++i)
            {
                var action = _actions[i];

                switch (action.type)
                {
                    // build prefab list as we go
                    case HierarchyActionType.Spawn:
                    {
                        var spawnAction = action.spawnAction;
                        bool isRelevant = false;
                        
                        for (int child = 0; child < spawnAction.childCount; ++child)
                        {
                            var childNid = new NetworkID(spawnAction.identityId, (ushort)child);
                            if (relevant.Contains(childNid))
                            {
                                isRelevant = true;
                                break;
                            }
                        }

                        if (isRelevant)
                        {
                            for (int child = 0; child < spawnAction.childCount; ++child)
                            {
                                var childNid = new NetworkID(spawnAction.identityId, (ushort)child);
                                spawned.Add(childNid);
                            }

                            actions.Add(action);
                        }
                        break;
                    }
                    // apply actions to the prefab list
                    case HierarchyActionType.Despawn:
                    {
                        if (spawned.Contains(action.despawnAction.identityId) || relevant.Contains(action.despawnAction.identityId))
                        {
                            spawned.Remove(action.despawnAction.identityId);
                            actions.Add(action);
                        }
                        break;
                    }
                    // apply actions to the prefab list
                    case HierarchyActionType.ChangeParent:
                    {
                        if (relevant.Contains(action.changeParentAction.identityId))
                            actions.Add(action);
                        break;
                    }
                    case HierarchyActionType.SetActive:
                    {
                        if (relevant.Contains(action.setActiveAction.identityId))
                            actions.Add(action);
                        break;
                    }
                    case HierarchyActionType.SetEnabled:
                    {
                        if (relevant.Contains(action.setEnabledAction.identityId))
                            actions.Add(action);
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            spawned.ExceptWith(relevant);

            foreach (var spawnedButNotUsed in spawned)
            {
                actions.Add(new HierarchyAction
                {
                    type = HierarchyActionType.Despawn,
                    actor = spawnedButNotUsed.scope,
                    despawnAction = new DespawnAction
                    {
                        identityId = spawnedButNotUsed,
                        despawnType = DespawnType.GameObject
                    }
                });
            }
            
            HashSetPool<NetworkID>.Destroy(relevant);
            HashSetPool<NetworkID>.Destroy(spawned);
            
            return new HierarchyActionBatch
            {
                sceneId = _sceneId,
                actions = actions
            };
        }

        internal HierarchyActionBatch GetDelta()
        {
            return new HierarchyActionBatch
            {
                sceneId = _sceneId,
                actions = _pending
            };
        }

        internal void Flush()
        {
            _actions.AddRange(_pending);
            OptimizeActions();
            _pending.Clear();
            hasUnflushedActions = false;
        }

        internal void AddSpawnAction(SpawnAction action, PlayerID actor)
        {
            _pending.Add(new HierarchyAction
            {
                type = HierarchyActionType.Spawn,
                actor = actor,
                spawnAction = action
            });
            
            hasUnflushedActions = true;
        }
        
        internal void AddDespawnAction(DespawnAction action, PlayerID actor)
        {
            _pending.Add(new HierarchyAction
            {
                type = HierarchyActionType.Despawn,
                actor = actor,
                despawnAction = action
            });
            
            hasUnflushedActions = true;
        }
        
        internal void AddChangeParentAction(ChangeParentAction action, PlayerID actor)
        {
            _pending.Add(new HierarchyAction
            {
                type = HierarchyActionType.ChangeParent,
                actor = actor,
                changeParentAction = action
            });
            
            hasUnflushedActions = true;
        }
        
        internal void AddSetActiveAction(SetActiveAction action, PlayerID actor)
        {
            _pending.Add(new HierarchyAction
            {
                type = HierarchyActionType.SetActive,
                actor = actor,
                setActiveAction = action
            });
            
            hasUnflushedActions = true;
        }
        
        internal void AddSetEnabledAction(SetEnabledAction action, PlayerID actor)
        {
            _pending.Add(new HierarchyAction
            {
                type = HierarchyActionType.SetEnabled,
                actor = actor,
                setEnabledAction = action
            });
            
            hasUnflushedActions = true;
        }

        private void OptimizeActions()
        {
            CleanupPrefabs();
            RemoveIrrelevantActions();
            CompressParentChanges();
        }

        private void CleanupPrefabs()
        {
            _prefabs.Clear();
            
            for (var i = 0; i < _actions.Count; ++i)
            {
                var action = _actions[i];
                switch (action.type)
                {
                    case HierarchyActionType.Spawn:
                    {
                        var prefab = new Prefab
                        {
                            childCount = action.spawnAction.childCount,
                            identityId = action.spawnAction.identityId
                        };

                        _prefabs.Add(prefab);
                        break;
                    }
                    case HierarchyActionType.Despawn:
                    {
                        int index = -1;
                        for (int j = 0; j < _prefabs.Count; j++)
                        {
                            if (!_prefabs[j].IsChild(action.despawnAction.identityId))
                                continue;
                            index = j;
                            break;
                        }

                        if (index == -1)
                            continue;

                        var prefab = _prefabs[index];
                        prefab.despawnedChildren++;
                        _prefabs[index] = prefab;
                        break;
                    }
                }
            }

            for (var i = 0; i < _prefabs.Count; ++i)
            {
                var prefab = _prefabs[i];
                
                if (prefab.despawnedChildren == prefab.childCount)
                    RemoveRelevantActions(prefab);
            }
        }
        
        private void RemoveIrrelevantActions()
        {
            for (int i = _actions.Count - 1; i >= 0; i--)
            {
                var action = _actions[i];
                switch (action.type)
                {
                    case HierarchyActionType.SetActive:
                    {
                        var identityId = action.setActiveAction.identityId;
                        
                        for (int j = i - 1; j >= 0; j--)
                        {
                            var previousAction = _actions[j];
                            if (previousAction.type == HierarchyActionType.SetActive &&
                                previousAction.setActiveAction.identityId == identityId)
                            {
                                _actions.RemoveAt(j);
                                i--;
                            }
                        }
                        
                        break;
                    }
                    case HierarchyActionType.SetEnabled:
                    {
                        var identityId = action.setEnabledAction.identityId;

                        for (int j = i - 1; j >= 0; j--)
                        {
                            var previousAction = _actions[j];
                            if (previousAction.type == HierarchyActionType.SetEnabled &&
                                previousAction.setEnabledAction.identityId == identityId)
                            {
                                _actions.RemoveAt(j);
                                i--;
                            }
                        }
                        
                        break;
                    }
                }
            }
        }
        
        private void CompressParentChanges()
        {
            for (int i = _actions.Count - 1; i >= 0; i--)
            {
                var action = _actions[i];
                if (action.type != HierarchyActionType.ChangeParent)
                    continue;

                var identityId = action.changeParentAction.identityId;

                for (int j = i - 1; j >= 0; j--)
                {
                    var previousAction = _actions[j];
                    if (previousAction.type != HierarchyActionType.ChangeParent)
                        continue;

                    if (previousAction.changeParentAction.identityId != identityId)
                        continue;

                    _actions.RemoveAt(j);
                    i--;
                }
            }
        }

        private void RemoveRelevantActions(Prefab prefab)
        {
            _toRemove.Clear();
            
            for (int i = 0; i < _actions.Count; i++)
            {
                var identityId = GetObjectId(_actions[i]);

                if (prefab.IsChild(identityId))
                {
                    _toRemove.Add(i);
                }
            }
            
            for (int i = _toRemove.Count - 1; i >= 0; i--)
            {
                _actions.RemoveAt(_toRemove[i]);
            }
        }

        private static NetworkID GetObjectId(HierarchyAction action)
        {
            return action.type switch
            {
                HierarchyActionType.Spawn => action.spawnAction.identityId,
                HierarchyActionType.Despawn => action.despawnAction.identityId,
                HierarchyActionType.ChangeParent => action.changeParentAction.identityId,
                HierarchyActionType.SetActive => action.setActiveAction.identityId,
                HierarchyActionType.SetEnabled => action.setEnabledAction.identityId,
                _ => throw new ArgumentException("Unknown action type")
            };
        }
        
        private struct Prefab
        {
            public ushort childCount;
            public NetworkID identityId;
            public int despawnedChildren;

            public bool IsChild(NetworkID id)
            {
                if (id.scope != identityId.scope)
                    return false;
                
                return id.id >= identityId.id && id.id < identityId.id + childCount;
            }
        }

        public void Clear()
        {
            _actions.Clear();
            _pending.Clear();
            hasUnflushedActions = false;
        }
    }
}
