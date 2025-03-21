using PurrNet.Modules;
using PurrNet.Utils;
using UnityEngine;

namespace PurrNet
{
    public class ColliderRollback : PurrMonoBehaviour
    {
        [Tooltip("How long to store the collider state for rollback in seconds.\n" +
                 "This should be long enough to account for ping and jitter.")]
        [SerializeField, PurrLock, HideInInspector]
        float _storeHistoryInSeconds = 5f;

        [SerializeField, PurrLock, HideInInspector]
        bool _autoAddAllChildren = true;

        [SerializeField, PurrLock, HideInInspector]
        Collider[] _colliders3D;

        [SerializeField, PurrLock, HideInInspector]
        Collider2D[] _colliders2D;

        public Collider[] colliders3D => _colliders3D;
        public Collider2D[] colliders2D => _colliders2D;

        public float storeHistoryInSeconds => _storeHistoryInSeconds;

        private RollbackModule _moduleServer;
        private RollbackModule _moduleClient;

        private void Awake()
        {
            if (_autoAddAllChildren)
            {
                _colliders3D = GetComponentsInChildren<Collider>(true);
                _colliders2D = GetComponentsInChildren<Collider2D>(true);
            }
        }

        public override void Subscribe(NetworkManager manager, bool asServer)
        {
            if (!manager.TryGetModule<ScenesModule>(asServer, out var scenesModule))
                return;

            if (!scenesModule.TryGetSceneID(gameObject.scene, out var sceneID))
                return;

            if (manager.TryGetModule<ColliderRollbackFactory>(asServer, out var factory) &&
                factory.TryGetModule(sceneID, out var module))
            {
                if (asServer)
                    _moduleServer = module;
                else _moduleClient = module;
                module.Register(this);
            }
        }

        public override void Unsubscribe(NetworkManager manager, bool asServer)
        {
            if (_moduleServer != null)
            {
                _moduleServer.Unregister(this);
                _moduleServer = null;
            }

            if (_moduleClient != null)
            {
                _moduleClient.Unregister(this);
                _moduleClient = null;
            }
        }
    }
}