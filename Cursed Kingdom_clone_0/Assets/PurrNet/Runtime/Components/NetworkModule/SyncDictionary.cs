using UnityEngine;
using PurrNet.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using PurrNet.Transports;

namespace PurrNet
{
    /// <summary>
    /// The operation which has happened to the dictionary
    /// </summary>
    public enum SyncDictionaryOperation
    {
        Added,
        Removed,
        Set,
        Cleared
    }
    
    /// <summary>
    /// All the data relevant to the change that happened to the dictionary
    /// </summary>
    public struct SyncDictionaryChange<TKey, TValue>
    {
        public SyncDictionaryOperation operation;
        public TKey key;
        public TValue value;

        public SyncDictionaryChange(SyncDictionaryOperation operation, TKey key = default, TValue value = default)
        {
            this.operation = operation;
            this.key = key;
            this.value = value;
        }

        public override string ToString()
        {
            string valueStr = $"Key: {key} | Value: {value} | Operation: {operation}";
            return valueStr;
        }
    }

    [Serializable]
    public class SyncDictionary<TKey, TValue> : NetworkModule, IDictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private bool _ownerAuth;
        [SerializeField] private SerializableDictionary<TKey, TValue> _serializedDict = new SerializableDictionary<TKey, TValue>();
        private Dictionary<TKey, TValue> _dict = new Dictionary<TKey, TValue>();
        
        public delegate void SyncDictionaryChanged<Key, Value>(SyncDictionaryChange<Key, Value> change);
        
        /// <summary>
        /// Event that is invoked when the dictionary is changed
        /// </summary>
        public event SyncDictionaryChanged<TKey, TValue> onChanged;
        
        /// <summary>
        /// Whether it is the owner or the server that has the authority to modify the dictionary
        /// </summary>
        public bool ownerAuth => _ownerAuth;
        
        /// <summary>
        /// The amount of entries in the dictionary
        /// </summary>
        public int Count => _dict.Count;
        public bool IsReadOnly => false;
        public ICollection<TKey> Keys => _dict.Keys;
        public ICollection<TValue> Values => _dict.Values;

        public SyncDictionary(bool ownerAuth = false)
        {
            _ownerAuth = ownerAuth;

#if UNITY_EDITOR
            onChanged += UpdateSerializedDict;
#endif
        }

        public TValue this[TKey key]
        {
            get => _dict[key];
            set
            {
                ValidateAuthority();
                
                bool isNewKey = !_dict.ContainsKey(key);
                _dict[key] = value;
                
                var operation = isNewKey ? SyncDictionaryOperation.Added : SyncDictionaryOperation.Set;
                var change = new SyncDictionaryChange<TKey, TValue>(operation, key, value);
                InvokeChange(change);
                
                if (isSpawned)
                {
                    if (isServer)
                        SendSetToAll(key, value, isNewKey);
                    else 
                        SendSetToServer(key, value, isNewKey);
                }
            }
        }
        
        public void OnBeforeSerialize()
        {
            _serializedDict.FromDictionary(_dict);
        }

        public void OnAfterDeserialize()
        {
            _dict = _serializedDict.ToDictionary();
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
            
            if (!IsController(_ownerAuth)) return;
            
            if (isServer)
                SendInitialStateToAll(_dict);
            else SendInitialStateToServer(_dict);
        }

        public override void OnObserverAdded(PlayerID player)
        {
            HandleInitialStateTarget(player, _dict);
        }
        
        [TargetRpc(Channel.ReliableOrdered)]
        private void HandleInitialStateTarget(PlayerID player, Dictionary<TKey, TValue> initialState)
        {
            HandleInitialState(initialState);
        }
        
        [ObserversRpc(Channel.ReliableOrdered)]
        private void SendInitialStateToAll(Dictionary<TKey, TValue> initialState)
        {
            HandleInitialState(initialState);
        }
        
        [ServerRpc(Channel.ReliableOrdered, requireOwnership: true)]
        private void SendInitialStateToServer(Dictionary<TKey, TValue> initialState)
        {
            if (!_ownerAuth) return;
            SendInitialStateToOthers(initialState);
        }
        
        [ObserversRpc(Channel.ReliableOrdered, excludeOwner: true)]
        private void SendInitialStateToOthers(Dictionary<TKey, TValue> initialState)
        {
            if (!isServer || isHost)
            {
                _dict = initialState;
                
                InvokeChange(new SyncDictionaryChange<TKey, TValue>(SyncDictionaryOperation.Cleared));

                foreach (var kvp in _dict)
                {
                    InvokeChange(new SyncDictionaryChange<TKey, TValue>(SyncDictionaryOperation.Added, kvp.Key, kvp.Value));
                }
            }
        }

        private void HandleInitialState(Dictionary<TKey, TValue> initialState)
        {
            if (!isHost)
            {
                if(initialState == null)
                    return;
                
                _dict = initialState;
                
                InvokeChange(new SyncDictionaryChange<TKey, TValue>(SyncDictionaryOperation.Cleared));

                foreach (var kvp in _dict)
                {
                    InvokeChange(new SyncDictionaryChange<TKey, TValue>(SyncDictionaryOperation.Added, kvp.Key, kvp.Value));
                }
            }
        }
        
#if UNITY_EDITOR
        private void UpdateSerializedDict(SyncDictionaryChange<TKey, TValue> _)
        {
            if (!UnityEditor.EditorApplication.isPlaying) return;
            _serializedDict.FromDictionary(_dict);
            UnityEditor.EditorUtility.SetDirty(parent);
        }
#endif

        /// <summary>
        /// Adds an entry to the dictionary and syncs the change
        /// </summary>
        public void Add(TKey key, TValue value)
        {
            ValidateAuthority();
            
            _dict.Add(key, value);
            var change = new SyncDictionaryChange<TKey, TValue>(SyncDictionaryOperation.Added, key, value);
            InvokeChange(change);
            
            if (isSpawned)
            {
                if (isServer)
                    SendAddToAll(key, value);
                else
                    SendAddToServer(key, value);
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        /// <summary>
        /// Removes an entry from the dictionary and syncs the change
        /// </summary>
        public bool Remove(TKey key)
        {
            ValidateAuthority();
            
            if (!_dict.TryGetValue(key, out TValue value))
                return false;
                
            _dict.Remove(key);
            var change = new SyncDictionaryChange<TKey, TValue>(SyncDictionaryOperation.Removed, key, value);
            InvokeChange(change);
            
            if (isSpawned)
            {
                if (isServer)
                    SendRemoveToAll(key);
                else
                    SendRemoveToServer(key);
            }
            
            return true;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (!Contains(item))
                return false;
                
            return Remove(item.Key);
        }

        /// <summary>
        /// Clears the dictionary and syncs the change
        /// </summary>
        public void Clear()
        {
            ValidateAuthority();
            
            _dict.Clear();
            var change = new SyncDictionaryChange<TKey, TValue>(SyncDictionaryOperation.Cleared);
            InvokeChange(change);
            
            if (isSpawned)
            {
                if (isServer)
                    SendClearToAll();
                else
                    SendClearToServer();
            }
        }
        
        /// <summary>
        /// Creates a new Dictionary from the SyncDictionary
        /// </summary>
        /// <returns>A new Dictionary containing all key-value pairs from this SyncDictionary</returns>
        public Dictionary<TKey, TValue> ToDictionary()
        {
            return new Dictionary<TKey, TValue>(_dict);
        }

        public bool ContainsKey(TKey key) => _dict.ContainsKey(key);
        public bool TryGetValue(TKey key, out TValue value) => _dict.TryGetValue(key, out value);
        public bool Contains(KeyValuePair<TKey, TValue> item) => (_dict as IDictionary<TKey, TValue>).Contains(item);
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => (_dict as IDictionary<TKey, TValue>).CopyTo(array, arrayIndex);
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dict.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private void ValidateAuthority()
        {
            if (!isSpawned) return;

            bool isController = parent.IsController(_ownerAuth);
            if (!isController)
            {
                PurrLogger.LogError(
                    $"Invalid permissions when modifying '<b>SyncDictionary<{typeof(TKey).Name}, {typeof(TValue).Name}> {name}</b>' on '{parent.name}'." +
                    $"\nMaybe try enabling owner authority.", parent);
                throw new InvalidOperationException("Invalid permissions");
            }
        }

        private void InvokeChange(SyncDictionaryChange<TKey, TValue> change)
        {
            onChanged?.Invoke(change);
        }

        #region RPCs
        [ServerRpc(Channel.ReliableOrdered, requireOwnership: true)]
        private void SendAddToServer(TKey key, TValue value)
        {
            if (!_ownerAuth) return;
            SendAddToOthers(key, value);
        }

        [ObserversRpc(Channel.ReliableOrdered, excludeOwner: true)]
        private void SendAddToOthers(TKey key, TValue value)
        {
            if (!isServer || isHost)
            {
                _dict[key] = value;
                InvokeChange(new SyncDictionaryChange<TKey, TValue>(SyncDictionaryOperation.Added, key, value));
            }
        }

        [ObserversRpc(Channel.ReliableOrdered)]
        private void SendAddToAll(TKey key, TValue value)
        {
            if (!isHost)
            {
                _dict[key] = value;
                InvokeChange(new SyncDictionaryChange<TKey, TValue>(SyncDictionaryOperation.Added, key, value));
            }
        }

        [ServerRpc(Channel.ReliableOrdered, requireOwnership: true)]
        private void SendRemoveToServer(TKey key)
        {
            if (!_ownerAuth) return;
            SendRemoveToOthers(key);
        }

        [ObserversRpc(Channel.ReliableOrdered, excludeOwner: true)]
        private void SendRemoveToOthers(TKey key)
        {
            if (!isServer || isHost)
            {
                if (_dict.TryGetValue(key, out TValue value))
                {
                    _dict.Remove(key);
                    InvokeChange(new SyncDictionaryChange<TKey, TValue>(SyncDictionaryOperation.Removed, key, value));
                }
            }
        }

        [ObserversRpc(Channel.ReliableOrdered)]
        private void SendRemoveToAll(TKey key)
        {
            if (!isHost)
            {
                if (_dict.Remove(key, out TValue value))
                {
                    InvokeChange(new SyncDictionaryChange<TKey, TValue>(SyncDictionaryOperation.Removed, key, value));
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
                _dict.Clear();
                InvokeChange(new SyncDictionaryChange<TKey, TValue>(SyncDictionaryOperation.Cleared));
            }
        }

        [ObserversRpc(Channel.ReliableOrdered)]
        private void SendClearToAll()
        {
            if (!isHost)
            {
                _dict.Clear();
                InvokeChange(new SyncDictionaryChange<TKey, TValue>(SyncDictionaryOperation.Cleared));
            }
        }

        [ServerRpc(Channel.ReliableOrdered, requireOwnership: true)]
        private void SendSetToServer(TKey key, TValue value, bool isNewKey)
        {
            if (!_ownerAuth) return;
            SendSetToOthers(key, value, isNewKey);
        }

        [ObserversRpc(Channel.ReliableOrdered, excludeOwner: true)]
        private void SendSetToOthers(TKey key, TValue value, bool isNewKey)
        {
            if (!isServer || isHost)
            {
                _dict[key] = value;
                var operation = isNewKey ? SyncDictionaryOperation.Added : SyncDictionaryOperation.Set;
                InvokeChange(new SyncDictionaryChange<TKey, TValue>(operation, key, value));
            }
        }

        [ObserversRpc(Channel.ReliableOrdered)]
        private void SendSetToAll(TKey key, TValue value, bool isNewKey)
        {
            if (!isHost)
            {
                _dict[key] = value;
                var operation = isNewKey ? SyncDictionaryOperation.Added : SyncDictionaryOperation.Set;
                InvokeChange(new SyncDictionaryChange<TKey, TValue>(operation, key, value));
            }
        }
        
        /// <summary>
        /// Forces the dictionary to be synced again at the given key. Good for when you modify something inside a value
        /// </summary>
        /// <param name="key">Key of the value to set dirty</param>
        public void SetDirty(TKey key)
        {
            if (!isSpawned) return;
    
            ValidateAuthority();
    
            if (!_dict.TryGetValue(key, out TValue value))
            {
                PurrLogger.LogError($"Key {key} not found in SyncDictionary when trying to SetDirty", parent);
                return;
            }
    
            InvokeChange(new SyncDictionaryChange<TKey, TValue>(SyncDictionaryOperation.Set, key, value));
    
            if (isServer)
                SendSetDirtyToAll(key, value);
            else
                SendSetDirtyToServer(key, value);
        }

        [ServerRpc(Channel.ReliableOrdered, requireOwnership: true)]
        private void SendSetDirtyToServer(TKey key, TValue value)
        {
            if (!_ownerAuth) return;
            SendSetDirtyToOthers(key, value);
        }

        [ObserversRpc(Channel.ReliableOrdered, excludeOwner: true)]
        private void SendSetDirtyToOthers(TKey key, TValue value)
        {
            if (!isServer || isHost)
            {
                if (_dict.ContainsKey(key))
                {
                    _dict[key] = value;
                    InvokeChange(new SyncDictionaryChange<TKey, TValue>(SyncDictionaryOperation.Set, key, value));
                }
            }
        }

        [ObserversRpc(Channel.ReliableOrdered)]
        private void SendSetDirtyToAll(TKey key, TValue value)
        {
            if (!isHost)
            {
                if (_dict.ContainsKey(key))
                {
                    _dict[key] = value;
                    InvokeChange(new SyncDictionaryChange<TKey, TValue>(SyncDictionaryOperation.Set, key, value));
                }
            }
        }
        #endregion
    }
    
    [Serializable]
    public class SerializableDictionary<TKey, TValue>
    {
        [SerializeField] private List<TKey> keys = new List<TKey>();
        [SerializeField] private List<TValue> values = new List<TValue>();
        
        [SerializeField] private List<string> stringKeys = new List<string>();
        [SerializeField] private List<string> stringValues = new List<string>();
        
        private bool isKeySerializable;
        private bool isValueSerializable;

        public SerializableDictionary()
        {
            isKeySerializable = typeof(TKey).IsSerializable || typeof(UnityEngine.Object).IsAssignableFrom(typeof(TKey));
            isValueSerializable = typeof(TValue).IsSerializable || typeof(UnityEngine.Object).IsAssignableFrom(typeof(TValue));
        }

        public Dictionary<TKey, TValue> ToDictionary()
        {
            var dict = new Dictionary<TKey, TValue>();
    
            if (isKeySerializable && isValueSerializable)
            {
                int count = Mathf.Min(keys.Count, values.Count);
                for (int i = 0; i < count; i++)
                {
                    if (keys[i] != null && !dict.ContainsKey(keys[i]))
                        dict.Add(keys[i], values[i]);
                }
            }
            else
            {
                var count = Mathf.Min(stringKeys.Count, stringValues.Count);
                for (int i = 0; i < count; i++)
                {
                    if (stringKeys[i] != null && !dict.ContainsKey(default(TKey)))
                        dict.Add(default(TKey), default(TValue));
                }
            }
    
            return dict;
        }

        public void FromDictionary(Dictionary<TKey, TValue> dict)
        {
            keys.Clear();
            values.Clear();
            stringKeys.Clear();
            stringValues.Clear();

            foreach (var kvp in dict)
            {
                if (isKeySerializable && isValueSerializable)
                {
                    keys.Add(kvp.Key);
                    values.Add(kvp.Value);
                }
                else
                {
                    stringKeys.Add(kvp.Key?.ToString() ?? "null");
                    stringValues.Add(kvp.Value?.ToString() ?? "null");
                }
            }
        }
        public bool IsSerializable => isKeySerializable && isValueSerializable;
        public int Count => isKeySerializable ? keys.Count : stringKeys.Count;
        public string GetDisplayKey(int index) => isKeySerializable ? 
            (keys[index]?.ToString() ?? "null") : 
            (stringKeys[index] ?? "null");
        public string GetDisplayValue(int index) => isValueSerializable ? 
            (values[index]?.ToString() ?? "null") : 
            (stringValues[index] ?? "null");
    }
}