using UnityEngine;

namespace PurrNet
{
    public abstract class PurrMonoBehaviour : MonoBehaviour, IPurrEvents
    {
        NetworkManager _networkManager;

        private bool _isSubscribedClient;
        private bool _isSubscribedServer;

        public virtual void OnEnable()
        {
            NetworkManager.main.RegisterEvents(InternalSubscribe, InternalUnsubscribe);
        }

        public virtual void OnDisable()
        {
            if (_isSubscribedClient) InternalUnsubscribe(_networkManager, false);
            if (_isSubscribedServer) InternalUnsubscribe(_networkManager, true);
            NetworkManager.main.UnregisterEvents(InternalSubscribe, InternalUnsubscribe);
        }

        private void InternalSubscribe(NetworkManager manager, bool asServer)
        {
            _networkManager = manager;

            if (asServer)
                _isSubscribedServer = true;
            else _isSubscribedClient = true;
            Subscribe(manager, asServer);
        }

        private void InternalUnsubscribe(NetworkManager manager, bool asServer)
        {
            if (asServer)
                _isSubscribedServer = false;
            else _isSubscribedClient = false;
            Unsubscribe(manager, asServer);
        }

        public abstract void Subscribe(NetworkManager manager, bool asServer);

        public abstract void Unsubscribe(NetworkManager manager, bool asServer);
    }
}