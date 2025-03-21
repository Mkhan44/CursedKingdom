using System.Collections.Generic;
using PurrNet.Modules;
using PurrNet.Transports;

namespace PurrNet
{
    internal readonly struct ModulesCollection
    {
        private readonly List<INetworkModule> _modules;
        private readonly List<IConnectionListener> _connectionListeners;
        private readonly List<IDataListener> _dataListeners;
        private readonly List<IFixedUpdate> _fixedUpdatesListeners;
        private readonly List<IPreFixedUpdate> _preFixedUpdatesListeners;
        private readonly List<IPostFixedUpdate> _posteFixedUpdatesListeners;
        private readonly List<IDrawGizmos> _drawGizmosListeners;
        private readonly List<IUpdate> _updateListeners;
        private readonly List<ICleanup> _cleanupListeners;

        private readonly NetworkManager _manager;
        private readonly bool _asServer;

        public ModulesCollection(NetworkManager manager, bool asServer)
        {
            _modules = new List<INetworkModule>();
            _connectionListeners = new List<IConnectionListener>();
            _preFixedUpdatesListeners = new List<IPreFixedUpdate>();
            _posteFixedUpdatesListeners = new List<IPostFixedUpdate>();
            _dataListeners = new List<IDataListener>();
            _updateListeners = new List<IUpdate>();
            _fixedUpdatesListeners = new List<IFixedUpdate>();
            _cleanupListeners = new List<ICleanup>();
            _drawGizmosListeners = new List<IDrawGizmos>();
            _manager = manager;
            _asServer = asServer;
        }

        public bool TryGetModule<T>(out T module) where T : INetworkModule
        {
            if (_modules == null)
            {
                module = default;
                return false;
            }

            for (int i = 0; i < _modules.Count; i++)
            {
                if (_modules[i] is T mod)
                {
                    module = mod;
                    return true;
                }
            }

            module = default;
            return false;
        }

        public void RegisterModules()
        {
            UnregisterModules();

            _manager.RegisterModules(this, _asServer);

            for (int i = 0; i < _modules.Count; i++)
            {
                _modules[i].Enable(_asServer);

                if (_modules[i] is IConnectionListener connectionListener)
                    _connectionListeners.Add(connectionListener);

                if (_modules[i] is IDataListener dataListener)
                    _dataListeners.Add(dataListener);

                if (_modules[i] is IFixedUpdate fixedUpdate)
                    _fixedUpdatesListeners.Add(fixedUpdate);

                if (_modules[i] is IUpdate update)
                    _updateListeners.Add(update);

                if (_modules[i] is ICleanup cleanup)
                    _cleanupListeners.Add(cleanup);

                if (_modules[i] is IPreFixedUpdate preFixedUpdate)
                    _preFixedUpdatesListeners.Add(preFixedUpdate);

                if (_modules[i] is IPostFixedUpdate postFixedUpdate)
                    _posteFixedUpdatesListeners.Add(postFixedUpdate);

                if (_modules[i] is IDrawGizmos drawGizmos)
                    _drawGizmosListeners.Add(drawGizmos);
            }
        }

        public void OnNewConnection(Connection conn, bool asServer)
        {
            for (int i = 0; i < _connectionListeners.Count; i++)
                _connectionListeners[i].OnConnected(conn, asServer);
        }

        public void OnLostConnection(Connection conn, bool asServer)
        {
            for (int i = 0; i < _connectionListeners.Count; i++)
                _connectionListeners[i].OnDisconnected(conn, asServer);
        }

        public void OnDataReceived(Connection conn, ByteData data, bool asServer)
        {
            for (int i = 0; i < _dataListeners.Count; i++)
                _dataListeners[i].OnDataReceived(conn, data, asServer);
        }

        public void TriggerOnUpdate()
        {
            for (int i = 0; i < _updateListeners.Count; i++)
                _updateListeners[i].Update();
        }

        public void TriggerOnFixedUpdate()
        {
            for (int i = 0; i < _fixedUpdatesListeners.Count; i++)
                _fixedUpdatesListeners[i].FixedUpdate();
        }

        public void TriggerOnPreFixedUpdate()
        {
            for (int i = 0; i < _preFixedUpdatesListeners.Count; i++)
                _preFixedUpdatesListeners[i].PreFixedUpdate();
        }

        public void TriggerOnPostFixedUpdate()
        {
            for (int i = 0; i < _posteFixedUpdatesListeners.Count; i++)
                _posteFixedUpdatesListeners[i].PostFixedUpdate();
        }

        public void TriggerOnDrawGizmos()
        {
            for (int i = 0; i < _drawGizmosListeners.Count; i++)
                _drawGizmosListeners[i].DrawGizmos();
        }

        public bool Cleanup()
        {
            bool allTrue = true;

            for (int i = 0; i < _cleanupListeners.Count; i++)
            {
                if (!_cleanupListeners[i].Cleanup())
                {
                    allTrue = false;
                }
            }

            return allTrue;
        }

        public void UnregisterModules()
        {
            for (int i = 0; i < _modules.Count; i++)
                _modules[i].Disable(_asServer);

            _modules.Clear();
            _connectionListeners.Clear();
            _dataListeners.Clear();
            _updateListeners.Clear();
            _fixedUpdatesListeners.Clear();
            _cleanupListeners.Clear();
            _preFixedUpdatesListeners.Clear();
            _posteFixedUpdatesListeners.Clear();
            _drawGizmosListeners.Clear();
        }

        public void AddModule(INetworkModule module)
        {
            _modules.Add(module);
        }
    }
}