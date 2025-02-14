using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using PurrNet.Transports;
using PurrNet.Logging;

namespace PurrNet
{
    [Serializable]
    public class SyncQueue<T> : NetworkModule, IReadOnlyCollection<T>
    {
        [SerializeField] private bool _ownerAuth;
        [SerializeField] private SerializableQueue<T> _serializedQueue = new SerializableQueue<T>();
        private Queue<T> _queue = new Queue<T>();
        
        public delegate void SyncQueueChanged<TYPE>(SyncQueueChange<TYPE> change);
        public event SyncQueueChanged<T> onChanged;
        
        public bool ownerAuth => _ownerAuth;
        public int Count => _queue.Count;

        public SyncQueue(bool ownerAuth = false)
        {
            _ownerAuth = ownerAuth;

#if UNITY_EDITOR
            onChanged += UpdateSerializedQueue;
#endif
        }

        public void OnBeforeSerialize()
        {
            _serializedQueue.FromQueue(_queue);
        }

        public void OnAfterDeserialize()
        {
            _queue = _serializedQueue.ToQueue();
        }

#if UNITY_EDITOR
        private void UpdateSerializedQueue(SyncQueueChange<T> _)
        {
            if (!UnityEditor.EditorApplication.isPlaying) return;
            _serializedQueue.FromQueue(_queue);
            UnityEditor.EditorUtility.SetDirty(parent);
        }
#endif

        public override void OnSpawn()
        {
            if (!IsController(_ownerAuth)) return;
            
            if (isServer)
                SendInitialStateToAll(_queue);
            else 
                SendInitialStateToServer(_queue);
        }

        public override void OnObserverAdded(PlayerID player)
        {
            SendInitialToTarget(player, _queue);
        }

        public T Peek()
        {
            if (_queue.Count == 0)
                throw new InvalidOperationException("Queue is empty");
                
            return _queue.Peek();
        }

        public bool TryPeek(out T result)
        {
            if (_queue.Count == 0)
            {
                result = default;
                return false;
            }
            
            result = _queue.Peek();
            return true;
        }

        public void Enqueue(T item)
        {
            ValidateAuthority();
            
            _queue.Enqueue(item);
            var change = new SyncQueueChange<T>(SyncQueueOperation.Enqueued, item);
            InvokeChange(change);
            
            if (isSpawned)
            {
                if (isServer)
                    SendEnqueueToAll(item);
                else
                    SendEnqueueToServer(item);
            }
        }

        public T Dequeue()
        {
            ValidateAuthority();
            
            if (_queue.Count == 0)
                throw new InvalidOperationException("Queue is empty");
                
            T item = _queue.Dequeue();
            var change = new SyncQueueChange<T>(SyncQueueOperation.Dequeued, item);
            InvokeChange(change);
            
            if (isSpawned)
            {
                if (isServer)
                    SendDequeueToAll();
                else
                    SendDequeueToServer();
            }
            
            return item;
        }

        public bool TryDequeue(out T result)
        {
            if (!IsController(_ownerAuth) || _queue.Count == 0)
            {
                result = default;
                return false;
            }
            
            result = Dequeue();
            return true;
        }

        public void Clear()
        {
            ValidateAuthority();
            
            _queue.Clear();
            var change = new SyncQueueChange<T>(SyncQueueOperation.Cleared);
            InvokeChange(change);
            
            if (isSpawned)
            {
                if (isServer)
                    SendClearToAll();
                else
                    SendClearToServer();
            }
        }

        public bool Contains(T item) => _queue.Contains(item);
        
        public void CopyTo(T[] array, int arrayIndex) => _queue.CopyTo(array, arrayIndex);
        
        public IEnumerator<T> GetEnumerator() => _queue.GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private void ValidateAuthority()
        {
            if (!isSpawned) return;

            bool isController = parent.IsController(_ownerAuth);
            if (!isController)
            {
                PurrLogger.LogError(
                    $"Invalid permissions when modifying '<b>SyncQueue<{typeof(T).Name}> {name}</b>' on '{parent.name}'." +
                    $"\nMaybe try enabling owner authority.", parent);
                throw new InvalidOperationException("Invalid permissions");
            }
        }

        private void InvokeChange(SyncQueueChange<T> change)
        {
            onChanged?.Invoke(change);
        }

        #region Initial State Handling
        [TargetRpc(Channel.ReliableOrdered)]
        private void SendInitialToTarget(PlayerID player, Queue<T> items)
        {
            HandleInitialState(items);
        }

        [ObserversRpc(Channel.ReliableOrdered)]
        private void SendInitialStateToAll(Queue<T> items)
        {
            HandleInitialState(items);
        }
        
        private void HandleInitialState(Queue<T> items)
        {
            if (!isHost)
            {
                if (items == null)
                    return;
                    
                _queue.Clear();
                foreach (var item in items)
                {
                    _queue.Enqueue(item);
                }
                
                InvokeChange(new SyncQueueChange<T>(SyncQueueOperation.Cleared));
                foreach (var item in items)
                {
                    InvokeChange(new SyncQueueChange<T>(SyncQueueOperation.Enqueued, item));
                }
            }
        }
        
        [ServerRpc(Channel.ReliableOrdered, requireOwnership: true)]
        private void SendInitialStateToServer(Queue<T> items)
        {
            if (!_ownerAuth) return;
            SendInitialStateToOthers(items);
        }

        [ObserversRpc(Channel.ReliableOrdered, excludeOwner: true)]
        private void SendInitialStateToOthers(Queue<T> items)
        {
            if (!isServer || isHost)
            {
                _queue.Clear();
                foreach (var item in items)
                {
                    _queue.Enqueue(item);
                }
                
                InvokeChange(new SyncQueueChange<T>(SyncQueueOperation.Cleared));
                foreach (var item in items)
                {
                    InvokeChange(new SyncQueueChange<T>(SyncQueueOperation.Enqueued, item));
                }
            }
        }
        #endregion

        #region RPCs
        [ServerRpc(Channel.ReliableOrdered, requireOwnership: true)]
        private void SendEnqueueToServer(T item)
        {
            if (!_ownerAuth) return;
            SendEnqueueToOthers(item);
        }

        [ObserversRpc(Channel.ReliableOrdered, excludeOwner: true)]
        private void SendEnqueueToOthers(T item)
        {
            if (!isServer || isHost)
            {
                _queue.Enqueue(item);
                InvokeChange(new SyncQueueChange<T>(SyncQueueOperation.Enqueued, item));
            }
        }

        [ObserversRpc(Channel.ReliableOrdered)]
        private void SendEnqueueToAll(T item)
        {
            if (!isHost)
            {
                _queue.Enqueue(item);
                InvokeChange(new SyncQueueChange<T>(SyncQueueOperation.Enqueued, item));
            }
        }

        [ServerRpc(Channel.ReliableOrdered, requireOwnership: true)]
        private void SendDequeueToServer()
        {
            if (!_ownerAuth) return;
            SendDequeueToOthers();
        }

        [ObserversRpc(Channel.ReliableOrdered, excludeOwner: true)]
        private void SendDequeueToOthers()
        {
            if (!isServer || isHost)
            {
                if (_queue.Count > 0)
                {
                    T item = _queue.Dequeue();
                    InvokeChange(new SyncQueueChange<T>(SyncQueueOperation.Dequeued, item));
                }
            }
        }

        [ObserversRpc(Channel.ReliableOrdered)]
        private void SendDequeueToAll()
        {
            if (!isHost)
            {
                if (_queue.Count > 0)
                {
                    T item = _queue.Dequeue();
                    InvokeChange(new SyncQueueChange<T>(SyncQueueOperation.Dequeued, item));
                }
            }
        }

        [ServerRpc(Channel.ReliableOrdered, requireOwnership: true)]
        private void SendClearToServer()
        {
            if (!_ownerAuth) return;
            SendClearToOthers();
        }

        [ObserversRpc(Channel.ReliableOrdered, excludeOwner: true)]
        private void SendClearToOthers()
        {
            if (!isServer || isHost)
            {
                _queue.Clear();
                InvokeChange(new SyncQueueChange<T>(SyncQueueOperation.Cleared));
            }
        }

        [ObserversRpc(Channel.ReliableOrdered)]
        private void SendClearToAll()
        {
            if (!isHost)
            {
                _queue.Clear();
                InvokeChange(new SyncQueueChange<T>(SyncQueueOperation.Cleared));
            }
        }
        #endregion
    }
    
    public enum SyncQueueOperation
    {
        Enqueued,
        Dequeued,
        Cleared
    }

    public struct SyncQueueChange<T>
    {
        public SyncQueueOperation operation;
        public T value;

        public SyncQueueChange(SyncQueueOperation operation, T value = default)
        {
            this.operation = operation;
            this.value = value;
        }

        public override string ToString()
        {
            return $"Value: {value} | Operation: {operation}";
        }
    }
    
    [Serializable]
    public class SerializableQueue<T>
    {
        [SerializeField] private List<T> _values = new List<T>();
        [SerializeField] private List<string> _stringValues = new List<string>();
        
        private bool _isValueSerializable;

        public SerializableQueue()
        {
            _isValueSerializable = typeof(T).IsSerializable || typeof(UnityEngine.Object).IsAssignableFrom(typeof(T));
        }

        public Queue<T> ToQueue()
        {
            var queue = new Queue<T>();
    
            if (_isValueSerializable)
            {
                foreach (var value in _values)
                {
                    queue.Enqueue(value);
                }
            }
            else
            {
                foreach (var _ in _stringValues)
                {
                    queue.Enqueue(default);
                }
            }
    
            return queue;
        }

        public void FromQueue(Queue<T> queue)
        {
            _values.Clear();
            _stringValues.Clear();

            foreach (var value in queue)
            {
                if (_isValueSerializable)
                {
                    _values.Add(value);
                }
                else
                {
                    _stringValues.Add(value?.ToString() ?? "null");
                }
            }
        }

        public bool IsSerializable => _isValueSerializable;
        public int Count => _isValueSerializable ? _values.Count : _stringValues.Count;
        public string GetDisplayValue(int index) => _isValueSerializable ? 
            (_values[index]?.ToString() ?? "null") : 
            (_stringValues[index] ?? "null");
    }
}