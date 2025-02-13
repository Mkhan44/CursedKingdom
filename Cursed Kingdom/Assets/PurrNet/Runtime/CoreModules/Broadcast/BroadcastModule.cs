using System;
using System.Collections.Generic;
using PurrNet.Logging;
using PurrNet.Packing;
using PurrNet.Transports;
using PurrNet.Utils;

namespace PurrNet.Modules
{
    public delegate void BroadcastDelegate<in T>(Connection conn, T data, bool asServer);
    
    internal interface IBroadcastCallback
    {
        bool IsSame(object callback);
        
        void TriggerCallback(Connection conn, object data, bool asServer);
    }

    internal readonly struct BroadcastCallback<T> : IBroadcastCallback
    {
        readonly BroadcastDelegate<T> callback;
        
        public BroadcastCallback(BroadcastDelegate<T> callback)
        {
            this.callback = callback;
        }

        public bool IsSame(object callbackToCmp)
        {
            return callbackToCmp is BroadcastDelegate<T> action && action == callback;
        }

        public void TriggerCallback(Connection conn, object data, bool asServer)
        {
            if (data is T value)
                callback?.Invoke(conn, value, asServer);
        }
    }
    
    public class BroadcastModule : INetworkModule, IDataListener
    {
        private readonly ITransport _transport;
        
        private readonly bool _asServer;

        private readonly Dictionary<uint, List<IBroadcastCallback>> _actions = new Dictionary<uint, List<IBroadcastCallback>>();
        
        internal event Action<Connection, uint, object> onRawDataReceived;
        
        public BroadcastModule(NetworkManager manager, bool asServer)
        {
            _transport = manager.transport.transport;
            _asServer = asServer;
        }

        public void Enable(bool asServer)
        {
        }

        public void Disable(bool asServer)
        {
        }

        void AssertIsServer(string message)
        {
            if (!_asServer)
                throw new InvalidOperationException(PurrLogger.FormatMessage(message));
        }

        public static ByteData GetImmediateData(object data)
        {
            using var stream = BitPackerPool.Get();
            Packer<uint>.Write(stream, Hasher.GetStableHashU32(data.GetType()));
            Packer.Write(stream, data);
            return stream.ToByteData();
        }

        private static ByteData GetData<T>(T data)
        {
            using var stream = BitPackerPool.Get();
            var typeId = Hasher.GetStableHashU32<T>();

            Packer<uint>.Write(stream, typeId);
            Packer<T>.Write(stream, data);

            return stream.ToByteData();
        }
        
        public void SendToAll<T>(T data, Channel method = Channel.ReliableOrdered)
        {
            AssertIsServer("Cannot send data to all clients from client.");

            var byteData = GetData(data);
            
            for (int i = 0; i < _transport.connections.Count; i++)
            {
                var conn = _transport.connections[i];
                _transport.SendToClient(conn, byteData, method);
            }
        }
        
        public void SendRaw(Connection conn, ByteData data, Channel method = Channel.ReliableOrdered)
        {
            AssertIsServer("Cannot send data to player from client.");
            _transport.SendToClient(conn, data, method);
        }
        
        public void Send<T>(Connection conn, T data, Channel method = Channel.ReliableOrdered)
        {
            AssertIsServer("Cannot send data to player from client.");
            
            var byteData = GetData(data);
            _transport.SendToClient(conn, byteData, method);
        }
        
        public void Send<T>(IEnumerable<Connection> conn, T data, Channel method = Channel.ReliableOrdered)
        {
            AssertIsServer("Cannot send data to player from client.");
            
            var byteData = GetData(data);
            
            foreach (var connection in conn)
                _transport.SendToClient(connection, byteData, method);
        }
        
        public void Send(IEnumerable<Connection> conn, ByteData byteData, Channel method = Channel.ReliableOrdered)
        {
            AssertIsServer("Cannot send data to player from client.");
            
            foreach (var connection in conn)
                _transport.SendToClient(connection, byteData, method);
        }
        
        public void SendToServer<T>(T data, Channel method = Channel.ReliableOrdered)
        {
            var byteData = GetData(data);

            if (_asServer)
            {
                _transport.RaiseDataReceived(default, byteData, true);
                return;
            }

            _transport.SendToServer(byteData, method);
        }
        
        public void OnDataReceived(Connection conn, ByteData data, bool asServer)
        {
            if (_asServer != asServer)
                return;
            
            using var stream = BitPackerPool.Get(data);
            
            uint typeId = default;
            
            Packer<uint>.Read(stream, ref typeId);

            if (!Hasher.TryGetType(typeId, out var typeInfo))
            {
                PurrLogger.LogError($"Cannot find type with id {typeId}; type must not have been registered properly.\nData: {data.ToString()}");
                return;
            }
            
            object instance = null;
            Packer.Read(stream, typeInfo, ref instance);
            TriggerCallback(conn, typeId, instance);
        }

        public void Subscribe<T>(BroadcastDelegate<T> callback)
        {
            var hash = Hasher.GetStableHashU32(typeof(T));

            if (_actions.TryGetValue(hash, out var actions))
            {
                actions.Add(new BroadcastCallback<T>(callback));
                return;
            }
            
            _actions.Add(hash, new List<IBroadcastCallback>
            {
                new BroadcastCallback<T>(callback)
            });
        }

        public void Unsubscribe<T>(BroadcastDelegate<T> callback)
        {
            var hash = Hasher.GetStableHashU32(typeof(T));
            if (!_actions.TryGetValue(hash, out var actions))
                return;
            
            object boxed = callback;

            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i].IsSame(boxed))
                {
                    actions.RemoveAt(i);
                    return;
                }
            }
        }

        private void TriggerCallback(Connection conn, uint hash, object instance)
        {
            if (_actions.TryGetValue(hash, out var actions))
            {
                for (int i = 0; i < actions.Count; i++)
                    actions[i].TriggerCallback(conn, instance, _asServer);
            }
            
            onRawDataReceived?.Invoke(conn, hash, instance);
        }
    }
    
    public struct KeepAlivePacket : IPackedAuto {}
}
