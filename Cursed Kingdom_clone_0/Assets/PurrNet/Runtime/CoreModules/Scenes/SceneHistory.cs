using System;
using System.Collections.Generic;
using PurrNet.Packing;
using UnityEngine.SceneManagement;

namespace PurrNet.Modules
{
    internal enum SceneActionType : byte
    {
        Load,
        Unload,
        SetActive
    }
    
    internal struct SceneAction : IPackedSimple
    {
        public SceneActionType type;
        
        public LoadSceneAction loadSceneAction;
        public UnloadSceneAction unloadSceneAction;
        public SetActiveSceneAction setActiveSceneAction;
        
        public void Serialize(BitPacker packer)
        {
            Packer<SceneActionType>.Serialize(packer, ref type);
            
            switch (type)
            {
                case SceneActionType.Load:
                    Packer<LoadSceneAction>.Serialize(packer, ref loadSceneAction);
                    break;
                case SceneActionType.Unload:
                    Packer<UnloadSceneAction>.Serialize(packer, ref unloadSceneAction);
                    break;
                case SceneActionType.SetActive:
                    Packer<SetActiveSceneAction>.Serialize(packer, ref setActiveSceneAction);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    
    internal struct SceneActionsBatch
    {
        public List<SceneAction> actions;
    }
    
    internal struct LoadSceneAction
    {
        public int buildIndex;
        public SceneID sceneID;
        public PurrSceneSettings parameters; 
        
        public LoadSceneParameters GetLoadSceneParameters()
        {
            return new LoadSceneParameters
            {
                loadSceneMode = parameters.mode,
                localPhysicsMode = parameters.physicsMode
            };
        }
    }
    
    internal struct UnloadSceneAction
    {
        public SceneID sceneID;
        public UnloadSceneOptions options;
    }
    
    internal struct SetActiveSceneAction
    {
        public SceneID sceneID;
    }
    
    internal class SceneHistory
    {
        readonly List<SceneAction> _actions = new List<SceneAction>();
        readonly List<SceneAction> _pending = new List<SceneAction>();
        
        public bool hasUnflushedActions { get; private set; }
        
        internal SceneActionsBatch GetFullHistory()
        {
            return new SceneActionsBatch
            {
                actions = _actions
            };
        }
        
        internal SceneActionsBatch GetDelta()
        {
            return new SceneActionsBatch
            {
                actions = _pending
            };
        }
        
        internal void Flush()
        {
            _actions.AddRange(_pending);
            _pending.Clear();
            hasUnflushedActions = false;
            OptimizeHistory();
        }

        private readonly List<SceneID> _sceneIds = new List<SceneID>();

        private void OptimizeHistory()
        {
            _sceneIds.Clear();
            
            for (int i = 0; i < _actions.Count; i++)
            {
                var action = _actions[i];
                switch (action.type)
                {
                    case SceneActionType.Load:
                        if (action.loadSceneAction.parameters.mode == LoadSceneMode.Single)
                            _sceneIds.Clear();
                        _sceneIds.Add(action.loadSceneAction.sceneID);
                        break;
                    case SceneActionType.Unload:
                        _sceneIds.Remove(action.unloadSceneAction.sceneID);
                        break;
                }
            }
            
            for (int i = _actions.Count - 1; i >= 0; i--)
            {
                var action = _actions[i];
                switch (action.type)
                {
                    case SceneActionType.Load:
                        if (!_sceneIds.Contains(action.loadSceneAction.sceneID))
                            _actions.RemoveAt(i);
                        break;
                    case SceneActionType.Unload:
                        if (!_sceneIds.Contains(action.unloadSceneAction.sceneID))
                            _actions.RemoveAt(i);
                        break;
                }
            }
        }
        
        internal void AddLoadAction(LoadSceneAction action)
        {
            _pending.Add(new SceneAction
            {
                type = SceneActionType.Load,
                loadSceneAction = action
            });
            
            hasUnflushedActions = true;
        }
        
        internal void AddUnloadAction(UnloadSceneAction action)
        {
            _pending.Add(new SceneAction
            {
                type = SceneActionType.Unload,
                unloadSceneAction = action
            });
            
            hasUnflushedActions = true;
        }
    }
}
