using System;
using System.Collections.Generic;

namespace PurrNet.Transports
{
    public delegate void OnConnectionState(ConnectionState state, bool asServer);
    public delegate void OnDataReceived(Connection conn, ByteData data, bool asServer);
    public delegate void OnDataSent(Connection conn, ByteData data, bool asServer); //Cannot send from clients
    public delegate void OnConnected(Connection conn, bool asServer);
    public delegate void OnDisconnected(Connection conn, DisconnectReason reason, bool asServer);
    
    public enum ConnectionState
    {
        Connecting,
        Connected,
        
        Disconnected,
        Disconnecting
    }

    public readonly struct ByteData
    {
        public readonly byte[] data;
        public readonly int length;
        public readonly int offset;
        
        public ReadOnlySpan<byte> span => new (data, offset, length);
        
        public static readonly ByteData empty = new (Array.Empty<byte>(), 0, 0);

        public ByteData(byte[] data, int offset, int length)
        {
            this.data = data;
            this.offset = offset;
            this.length = length;
        }

        public override string ToString()
        {
            string str = $"LENGTH: {length} DATA: ";
            for (int i = 0; i < length; i++)
                str += data[i + offset].ToString("X2") + " ";
            return str;
        }
    }
    
    public enum Channel : byte
    {
        /// <summary>
        /// It ensures that the data is received but the order is not guaranteed.
        /// </summary>
        ReliableUnordered,
        
        /// <summary>
        /// It ensures that the data is received but the order is not guaranteed.
        /// They are batched together at the end of the tick.
        /// </summary>
        ReliableBatched,
        
        /// <summary>
        /// Packets are guaranteed to be in order but not guaranteed to be received.
        /// </summary>
        UnreliableSequenced,
        
        /// <summary>
        /// Packets are guaranteed to be received in order.
        /// </summary>
        ReliableOrdered,
        
        /// <summary>
        /// Packets are not guaranteed to be received nor in order.
        /// </summary>
        Unreliable,
        
        /// <summary>
        /// Packets are not guaranteed to be received nor in order.
        /// They are batched together at the end of the tick.
        /// </summary>
        UnreliableBatched,
    }
    
    public interface IConnectable
    {
        ConnectionState clientState { get; }
        
        void Connect(string ip, ushort port);
        
        void Disconnect();
    }
    
    public interface IListener
    {
        ConnectionState listenerState { get; }

        void Listen(ushort port);
        
        void StopListening();
    }
    
    public interface ITransport : IListener, IConnectable
    {
        event OnConnected onConnected;
        event OnDisconnected onDisconnected;
        event OnDataReceived onDataReceived;
        event OnDataSent onDataSent;
        event OnConnectionState onConnectionState;
        
        public IReadOnlyList<Connection> connections { get; }
        
        bool shouldServerSendKeepAlive => false;

        bool shouldClientSendKeepAlive => false;

        void SendServerKeepAlive()
        {
        }

        void RaiseDataReceived(Connection conn, ByteData data, bool asServer);
        
        void RaiseDataSent(Connection conn, ByteData data, bool asServer);
        
        void SendToClient(Connection target, ByteData data, Channel method = Channel.ReliableOrdered);
        
        void SendToServer(ByteData data, Channel method = Channel.ReliableOrdered);
        
        void CloseConnection(Connection conn);

        void TickUpdate(float delta);
        
        void UnityUpdate(float delta) {}
    }

    public enum DisconnectReason
    {
        Timeout,
        ClientRequest,
        ServerRequest,
    }
}
