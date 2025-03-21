using UnityEngine;
using PurrNet.Logging;
using PurrNet.Modules;
using System;
using JetBrains.Annotations;
using PurrNet.Transports;
using PurrNet.Utils;

namespace PurrNet
{
    [Serializable]
    public class SyncVar<T> : NetworkModule, ITick
    {
        private TickManager _tickManager;

        [SerializeField, PurrLock] private T _value;

        private bool _isDirty;

        [SerializeField, Space(-5), Header("Sync Settings"), PurrLock]
        private bool _ownerAuth;

        [SerializeField, Min(0)] private float _sendIntervalInSeconds;

        public bool ownerAuth => _ownerAuth;

        public float sendIntervalInSeconds
        {
            get => _sendIntervalInSeconds;
            set => _sendIntervalInSeconds = value;
        }

        public event Action<T> onChanged;

        public bool isControllingSyncVar => parent.IsController(_ownerAuth);

        public T value
        {
            get => _value;
            set
            {
                bool bothNull = value == null && _value == null;
                bool bothEqual = value != null && value.Equals(_value);

                if (bothNull || bothEqual)
                    return;

                if (isSpawned && !parent.IsController(_ownerAuth))
                {
                    PurrLogger.LogError(
                        $"Invalid permissions when setting '<b>SyncVar<{typeof(T).Name}> {name}</b>' on '{parent.name}'." +
                        $"\nMaybe try enabling owner authority.", parent);
                    return;
                }

                _value = value;
                _isDirty = true;

                onChanged?.Invoke(value);
            }
        }

        public override void OnOwnerChanged(PlayerID? oldOwner, PlayerID? newOwner, bool asServer)
        {
            if (_ownerAuth && asServer)
            {
                _id = 0;
                SendLatestStateToAll(_id, _value);
            }
        }

        public override void OnObserverAdded(PlayerID player)
        {
            SendLatestState(player, _id, _value);
        }

        public override void OnDespawned()
        {
            if (isControllingSyncVar)
            {
                _id += 1;
                FlushImmediately();
            }
        }

        private float _lastSendTime;

        private void ForceSendUnreliable()
        {
            if (isServer)
                SendToAll(_id++, _value);
            else SendToServer(_id++, _value);
        }

        private void ForceSendReliable()
        {
            if (isServer)
                SendToAllReliably(_id++, _value);
            else SendToServerReliably(_id++, _value);
        }

        public void FlushImmediately()
        {
            ForceSendReliable();
            _lastSendTime = Time.time;
            _wasLastDirty = false;
            _isDirty = false;
        }

        public void OnTick(float delta)
        {
            bool isControlling = parent.IsController(_ownerAuth);

            if (!isControlling)
                return;

            float timeSinceLastSend = Time.time - _lastSendTime;

            if (timeSinceLastSend < _sendIntervalInSeconds)
                return;

            if (_isDirty)
            {
                ForceSendUnreliable();

                _lastSendTime = Time.time;
                _wasLastDirty = true;
                _isDirty = false;
            }
            else
            {
                if (_wasLastDirty)
                {
                    ForceSendReliable();
                    _wasLastDirty = false;
                }
            }
        }

        private ushort _id;
        private bool _wasLastDirty;

        public SyncVar(T initialValue = default, float sendIntervalInSeconds = 0f, bool ownerAuth = false)
        {
            _value = initialValue;
            _sendIntervalInSeconds = sendIntervalInSeconds;
            _ownerAuth = ownerAuth;
        }

        [ObserversRpc, UsedImplicitly]
        private void SendLatestStateToAll(ushort packetId, T newValue)
        {
            if (isServer) return;

            _id = packetId;

            bool bothNull = _value == null && newValue == null;
            bool bothEqual = _value != null && _value.Equals(newValue);

            if (bothNull || bothEqual)
                return;

            _value = newValue;
            onChanged?.Invoke(value);
        }

        [TargetRpc, UsedImplicitly]
        private void SendLatestState(PlayerID player, ushort packetId, T newValue)
        {
            if (isServer) return;

            _id = packetId;

            bool bothNull = _value == null && newValue == null;
            bool bothEqual = _value != null && _value.Equals(newValue);

            if (bothNull || bothEqual)
                return;

            _value = newValue;
            onChanged?.Invoke(value);
        }

        [ServerRpc(Channel.Unreliable, requireOwnership: true)]
        private void SendToServer(ushort packetId, T newValue)
        {
            if (!_ownerAuth) return;

            OnReceivedValue(packetId, newValue);
            SendToOthers(packetId, newValue);
        }

        [ServerRpc(Channel.ReliableOrdered, requireOwnership: true)]
        private void SendToServerReliably(ushort packetId, T newValue)
        {
            if (!_ownerAuth) return;

            OnReceivedValue(packetId, newValue);
            SendToOthersReliably(packetId, newValue);
        }

        [ObserversRpc(Channel.Unreliable, excludeOwner: true)]
        private void SendToOthers(ushort packetId, T newValue)
        {
            if (!isServer) OnReceivedValue(packetId, newValue);
        }

        [ObserversRpc(Channel.ReliableOrdered, excludeOwner: true)]
        private void SendToOthersReliably(ushort packetId, T newValue)
        {
            if (!isHost) OnReceivedValue(packetId, newValue);
        }

        [ObserversRpc(Channel.Unreliable)]
        private void SendToAll(ushort packetId, T newValue)
        {
            if (!isHost) OnReceivedValue(packetId, newValue);
        }

        [ObserversRpc(Channel.ReliableOrdered)]
        private void SendToAllReliably(ushort packetId, T newValue)
        {
            if (!isHost) OnReceivedValue(packetId, newValue);
        }

        private void OnReceivedValue(ushort packetId, T newValue)
        {
            bool isControlling = parent.IsController(_ownerAuth);

            if (isControlling)
            {
                return;
            }

            if (packetId <= _id)
            {
                return;
            }

            _id = packetId;

            bool bothNull = _value == null && newValue == null;
            bool bothEqual = _value != null && _value.Equals(newValue);

            if (bothNull || bothEqual)
                return;

            _value = newValue;
            onChanged?.Invoke(value);
        }

        public static implicit operator T(SyncVar<T> syncVar)
        {
            return syncVar._value;
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }
}