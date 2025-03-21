using UnityEngine;
using PurrNet.Logging;
using System;
using UnityEngine.Events;
using PurrNet.Transports;
using PurrNet.Utils;

namespace PurrNet
{
    [Serializable]
    public abstract class SyncEventBase : NetworkModule
    {
        [SerializeField, PurrLock] protected bool _ownerAuth;

        /// <summary>
        /// Whether it is the owner or  the server that has the authority to invoke the event
        /// </summary>
        public bool ownerAuth => _ownerAuth;

        protected SyncEventBase(bool ownerAuth = false)
        {
            _ownerAuth = ownerAuth;
        }

        protected bool ValidateInvoke()
        {
            if (!isSpawned) return true;

            bool isController = parent.IsController(_ownerAuth);
            if (!isController)
            {
                PurrLogger.LogError(
                    $"Invalid permissions when invoking '<b>{GetType().Name} {name}</b>' on '{parent.name}'." +
                    $"\nMaybe try enabling owner authority.", parent);
                return false;
            }

            return true;
        }

        protected abstract void InvokeLocal();
    }

    [Serializable]
    public class SyncEvent : SyncEventBase
    {
        [SerializeField] private UnityEvent _unityEvent = new UnityEvent();

        public SyncEvent(bool ownerAuth = false) : base(ownerAuth)
        {
        }

        /// <summary>
        /// Adds a listener to the event.
        /// </summary>
        /// <param name="listener">Listener to add</param>
        public void AddListener(UnityAction listener) => _unityEvent.AddListener(listener);

        /// <summary>
        /// Removes a listener from the event.
        /// </summary>
        /// <param name="listener">Listener to remove</param>
        public void RemoveListener(UnityAction listener) => _unityEvent.RemoveListener(listener);

        /// <summary>
        /// Invokes the event for all clients.
        /// </summary>
        public void Invoke()
        {
            if (!ValidateInvoke()) return;

            InvokeLocal();

            if (isSpawned)
            {
                if (isServer)
                    SendToAll();
                else SendToServer();
            }
        }

        protected override void InvokeLocal() => _unityEvent?.Invoke();

        [ServerRpc(Channel.ReliableOrdered, requireOwnership: true)]
        private void SendToServer()
        {
            if (!_ownerAuth) return;
            SendToOthers();
        }

        [ObserversRpc(Channel.ReliableOrdered, excludeOwner: true)]
        private void SendToOthers()
        {
            if (isServer && !isHost)
                return;
            InvokeLocal();
        }

        [ObserversRpc(Channel.ReliableOrdered)]
        private void SendToAll()
        {
            if (!isHost) InvokeLocal();
        }

        /// <summary>
        /// Removes all listeners from the event.
        /// </summary>
        /// <param name="sync">Whether to sync it or do it locally. By default, it's local</param>
        public void RemoveAllListeners(bool sync = false)
        {
            if (!sync)
            {
                _unityEvent.RemoveAllListeners();
                return;
            }

            _unityEvent.RemoveAllListeners();

            if (!isSpawned) return;

            if (isServer)
                RemoveAllListenersRpc();
            else
                RemoveAllListenersServer();
        }

        [ServerRpc(requireOwnership: true)]
        private void RemoveAllListenersServer()
        {
            if (!_ownerAuth) return;
            RemoveAllListenersRpc();
        }

        [ObserversRpc(runLocally: true, excludeOwner: true)]
        private void RemoveAllListenersRpc()
        {
            _unityEvent.RemoveAllListeners();
        }
    }

    [Serializable]
    public class SyncEvent<T> : SyncEventBase
    {
        [SerializeField] private SerializableSyncUnityEvent<T> unityEvent = new SerializableSyncUnityEvent<T>();
        private T _lastArg;

        public SyncEvent(bool ownerAuth = false) : base(ownerAuth)
        {
        }

        /// <summary>
        /// Adds a listener to the event.
        /// </summary>
        /// <param name="listener">Listener to add</param>
        public void AddListener(UnityAction<T> listener) => unityEvent.AddListener(listener);

        /// <summary>
        /// Removes a listener from the event.
        /// </summary>
        /// <param name="listener">Listener to remove</param>
        public void RemoveListener(UnityAction<T> listener) => unityEvent.RemoveListener(listener);

        /// <summary>
        /// Invokes the event for all clients.
        /// </summary>
        public void Invoke(T arg)
        {
            if (!ValidateInvoke()) return;

            _lastArg = arg;
            InvokeLocal();

            if (isSpawned)
            {
                if (isServer)
                    SendToAll(arg);
                else SendToServer(arg);
            }
        }

        protected override void InvokeLocal() => unityEvent?.Invoke(_lastArg);

        [ServerRpc(Channel.ReliableOrdered, requireOwnership: true)]
        private void SendToServer(T arg)
        {
            if (!_ownerAuth) return;
            SendToOthers(arg);
        }

        [ObserversRpc(Channel.ReliableOrdered, excludeOwner: true)]
        private void SendToOthers(T arg)
        {
            if (isServer && !isHost)
                return;

            _lastArg = arg;
            InvokeLocal();
        }

        [ObserversRpc(Channel.ReliableOrdered)]
        private void SendToAll(T arg)
        {
            if (!isHost)
            {
                _lastArg = arg;
                InvokeLocal();
            }
        }

        /// <summary>
        /// Removes all listeners from the event.
        /// </summary>
        /// <param name="sync">Whether to sync it or do it locally. By default, it's local</param>
        public void RemoveAllListeners(bool sync = false)
        {
            if (!sync)
            {
                unityEvent.RemoveAllListeners();
                return;
            }

            RemoveAllListenersRpc();
        }

        [ObserversRpc(runLocally: true)]
        private void RemoveAllListenersRpc()
        {
            unityEvent.RemoveAllListeners();
        }
    }

    [Serializable]
    public class SerializableSyncUnityEvent<T> : UnityEvent<T>
    {
    }

    [Serializable]
    public class SerializableSyncUnityEvent<T1, T2> : UnityEvent<T1, T2>
    {
    }

    [Serializable]
    public class SyncEvent<T1, T2> : SyncEventBase
    {
        [SerializeField]
        private SerializableSyncUnityEvent<T1, T2> unityEvent = new SerializableSyncUnityEvent<T1, T2>();

        private T1 _lastArg1;
        private T2 _lastArg2;

        public SyncEvent(bool ownerAuth = false) : base(ownerAuth)
        {
        }

        /// <summary>
        /// Adds a listener to the event.
        /// </summary>
        /// <param name="listener">Listener to add</param>
        public void AddListener(UnityAction<T1, T2> listener) => unityEvent.AddListener(listener);

        /// <summary>
        /// Removes a listener from the event.
        /// </summary>
        /// <param name="listener">Listener to remove</param>
        public void RemoveListener(UnityAction<T1, T2> listener) => unityEvent.RemoveListener(listener);

        /// <summary>
        /// Invokes the event for all clients.
        /// </summary>
        public void Invoke(T1 arg1, T2 arg2)
        {
            if (!ValidateInvoke()) return;

            _lastArg1 = arg1;
            _lastArg2 = arg2;
            InvokeLocal();

            if (isSpawned)
            {
                if (isServer)
                    SendToAll(arg1, arg2);
                else SendToServer(arg1, arg2);
            }
        }

        protected override void InvokeLocal() => unityEvent?.Invoke(_lastArg1, _lastArg2);

        [ServerRpc(Channel.ReliableOrdered, requireOwnership: true)]
        private void SendToServer(T1 arg1, T2 arg2)
        {
            if (!_ownerAuth) return;
            SendToOthers(arg1, arg2);
        }

        [ObserversRpc(Channel.ReliableOrdered, excludeOwner: true)]
        private void SendToOthers(T1 arg1, T2 arg2)
        {
            if (isServer && !isHost)
                return;

            _lastArg1 = arg1;
            _lastArg2 = arg2;
            InvokeLocal();
        }

        [ObserversRpc(Channel.ReliableOrdered)]
        private void SendToAll(T1 arg1, T2 arg2)
        {
            if (!isHost)
            {
                _lastArg1 = arg1;
                _lastArg2 = arg2;
                InvokeLocal();
            }
        }

        /// <summary>
        /// Removes all listeners from the event.
        /// </summary>
        /// <param name="sync">Whether to sync it or do it locally. By default, it's local</param>
        public void RemoveAllListeners(bool sync = false)
        {
            if (!sync)
            {
                unityEvent.RemoveAllListeners();
                return;
            }

            RemoveAllListenersRpc();
        }

        [ObserversRpc(runLocally: true)]
        private void RemoveAllListenersRpc()
        {
            unityEvent.RemoveAllListeners();
        }
    }
}