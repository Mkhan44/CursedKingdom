using System.Collections.Generic;
using LiteNetLib;
using UnityEngine;

namespace PurrNet.Transports
{
    [DefaultExecutionOrder(-100)]
    public class UDPTransport : GenericTransport, ITransport, INetLogger
    {
        [Header("Server Settings")]
        [Tooltip("The port which the server will start on, and clients connect.")]
        [SerializeField] private ushort _serverPort = 5000;
        [Tooltip("The max amount of client connections allowed.")]
        [SerializeField] private int _maxConnections = 100;

        [Header("Client Settings")]
        [Tooltip("This is the IP the client will use to connect to the server.")]
        [SerializeField] private string _address = "127.0.0.1";
        
        public event OnConnected onConnected;
        public event OnDisconnected onDisconnected;
        public event OnDataReceived onDataReceived;
        public event OnDataSent onDataSent;
        public event OnConnectionState onConnectionState;

        public string address { get => _address; set => _address = value; }
        
        public ushort serverPort { get => _serverPort; set => _serverPort = value; }
        
        public int maxConnections { get => _maxConnections; set => _maxConnections = value; }
        
        public IReadOnlyList<Connection> connections => _connections;

        private EventBasedNetListener _clientListener;
        private EventBasedNetListener _serverListener;
        
        private NetManager _client;
        private NetManager _server;

        public ConnectionState clientState { get; private set; } = ConnectionState.Disconnected;
        
        public ConnectionState listenerState { get; private set; } = ConnectionState.Disconnected;

        readonly List<Connection> _connections = new List<Connection>();

        public override bool isSupported => Application.platform != RuntimePlatform.WebGLPlayer;
        
        public override ITransport transport => this;

        private void Awake()
        {
            NetDebug.Logger = this;
        }

        private void OnEnable()
        {
            _clientListener = new EventBasedNetListener();
            _serverListener = new EventBasedNetListener();
            
            _client = new NetManager(_clientListener)
            {
                UnconnectedMessagesEnabled = true,
                PingInterval = 900,
                AutoRecycle = true,
                EnableStatistics = false,
            };
            
            _server = new NetManager(_serverListener)
            {
                UnconnectedMessagesEnabled = true,
                PingInterval = 900,
                AutoRecycle = true,
                EnableStatistics = false,
                
            };

            _clientListener.PeerConnectedEvent += OnClientConnected;
            _clientListener.PeerDisconnectedEvent += OnClientDisconnected;
            _clientListener.NetworkReceiveEvent += OnClientData;
            
            _serverListener.ConnectionRequestEvent += OnServerConnectionRequest;
            _serverListener.PeerConnectedEvent += OnServerConnected;
            _serverListener.PeerDisconnectedEvent += OnServerDisconnected;
            _serverListener.NetworkReceiveEvent += OnServerData;
        }

        public void RaiseDataReceived(Connection conn, ByteData data, bool asServer)
        {
            onDataReceived?.Invoke(conn, data, asServer);
        }

        public void RaiseDataSent(Connection conn, ByteData data, bool asServer)
        {
            onDataSent?.Invoke(conn, data, asServer);
        }

        private void OnServerData(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliverymethod)
        {
            var data = new ByteData(reader.RawData, reader.UserDataOffset, reader.UserDataSize);
            onDataReceived?.Invoke(new Connection(peer.Id), data, true);
            reader.Recycle();
        }

        private void OnClientData(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliverymethod)
        {
            var data = new ByteData(reader.RawData, reader.UserDataOffset, reader.UserDataSize);
            onDataReceived?.Invoke(new Connection(peer.Id), data, false);
            reader.Recycle();
        }

        private void OnServerConnectionRequest(ConnectionRequest request)
        {
            if (_server.ConnectedPeersCount < _maxConnections)
                 request.AcceptIfKey("PurrNet");
            else request.Reject();
        }

        protected override void StartServerInternal()
        {
            Listen(_serverPort);
        }

        protected override void StartClientInternal()
        {
            Connect(_address, _serverPort);
        }

        private void OnClientDisconnected(NetPeer peer, DisconnectInfo disconnectinfo)
        {
            clientState = ConnectionState.Disconnected;
            TriggerConnectionStateEvent(false);
            onDisconnected?.Invoke(new Connection(peer.Id),DisconnectReason.Timeout, false);
        }

        private Connection? _clientToServerConn;

        private void OnClientConnected(NetPeer peer)
        {
            var conn = new Connection(peer.Id);
            _clientToServerConn = conn;
            clientState = ConnectionState.Connected;
            TriggerConnectionStateEvent(false);
            onConnected?.Invoke(conn, false);
        }
        
        private void OnServerDisconnected(NetPeer peer, DisconnectInfo disconnectinfo)
        {
            var conn = new Connection(peer.Id);
            
            for (int i = 0; i < _connections.Count; i++)
            {
                if (_connections[i] == conn)
                {
                    _connections.RemoveAt(i);
                    break;
                }
            }
            
            onDisconnected?.Invoke(conn, DisconnectReason.Timeout, true);
            _clientToServerConn = null;
        }

        private void OnServerConnected(NetPeer peer)
        {
            var conn = new Connection(peer.Id);
            _connections.Add(conn);
            onConnected?.Invoke(conn, true);
        }

        /// In this mode you should use ManualReceive (without PollEvents) for receive packets
        /// and ManualUpdate(...) for update and send packets
        public void TickUpdate(float delta)
        {
            var dInMs = Mathf.FloorToInt(delta * 1000);
            
            if (_server.IsRunning)
            {
                _server.ManualUpdate(dInMs);
                _server.PollEvents();
            }

            if (_client.IsRunning)
            {
                _client.ManualUpdate(dInMs);
                _client.PollEvents();
            }
        }
        
        public void UnityUpdate(float delta)
        {
            if (_server.IsRunning) _server.PollEvents();
            if (_client.IsRunning) _client.PollEvents();
        }

        public void Connect(string ip, ushort port)
        {
            if (clientState == ConnectionState.Connected)
                return;
            
            clientState = ConnectionState.Connecting;
            
            TriggerConnectionStateEvent(false);

            _client.StartInManualMode(0);
            _client.Connect(ip, port, "PurrNet");
            TriggerConnectionStateEvent(false);
        }

        public void Disconnect()
        {
            if (clientState is not (ConnectionState.Connected or ConnectionState.Connecting))
                return;
            
            clientState = ConnectionState.Disconnecting;
            TriggerConnectionStateEvent(false);

            if (_clientToServerConn.HasValue)
            {
                onDisconnected?.Invoke(_clientToServerConn.Value, DisconnectReason.ClientRequest, false);
                _clientToServerConn = null;
            }

            _client.DisconnectAll();
            _client.Stop();
            
            clientState = ConnectionState.Disconnected;
            TriggerConnectionStateEvent(false);
        }

        public void Listen(ushort port)
        {
            NetDebug.Logger = this;

            if (listenerState is ConnectionState.Disconnected or ConnectionState.Disconnecting)
            {
                listenerState = ConnectionState.Connecting;
                TriggerConnectionStateEvent(true);

                _server.StartInManualMode(port);
                
                listenerState = ConnectionState.Connected;
                TriggerConnectionStateEvent(true);
            }
        }

        public void StopListening()
        {
            if (listenerState is ConnectionState.Connected or ConnectionState.Connecting)
            {
                listenerState = ConnectionState.Disconnecting;
                TriggerConnectionStateEvent(true);
                
                _server.Stop();
                
                listenerState = ConnectionState.Disconnected;
                TriggerConnectionStateEvent(true);
                
                _connections.Clear();
            }
        }

        DeliveryMethod ToDeliveryMethod(Channel channel)
        {
            return channel switch
            {
                Channel.ReliableUnordered => DeliveryMethod.ReliableUnordered,
                Channel.ReliableBatched => DeliveryMethod.ReliableUnordered,
                Channel.UnreliableSequenced => DeliveryMethod.Sequenced,
                Channel.ReliableOrdered => DeliveryMethod.ReliableOrdered,
                Channel.Unreliable => DeliveryMethod.Unreliable,
                Channel.UnreliableBatched => DeliveryMethod.Unreliable,
                _ => DeliveryMethod.Unreliable
            };
        }

        public void SendToClient(Connection target, ByteData data, Channel method = Channel.Unreliable)
        {
            if (listenerState is not ConnectionState.Connected)
                return;
            
            if (!target.isValid)
                return;
            
            var deliveryMethod = ToDeliveryMethod(method);
            var peer = _server.GetPeerById(target.connectionId);
            peer?.Send(data.data, data.offset, data.length, deliveryMethod);
            RaiseDataSent(target, data, true);
        }
        
        public void SendToServer(ByteData data, Channel method = Channel.Unreliable)
        {
            if (clientState != ConnectionState.Connected)
                return;
            
            var deliveryMethod = ToDeliveryMethod(method);
            _client.SendToAll(data.data, data.offset, data.length, deliveryMethod);
            RaiseDataSent(default, data, false);
        }

        public void CloseConnection(Connection conn)
        {
            var peer = _server.GetPeerById(conn.connectionId);
            peer?.Disconnect();
        }

        private void OnDisable()
        {
            _client.Stop();
            _server.Stop();
            
            listenerState = ConnectionState.Disconnected;
            clientState = ConnectionState.Disconnected;

            TriggerConnectionStateEvent(true);
            TriggerConnectionStateEvent(false);

            _connections.Clear();
        }

        public void WriteNet(NetLogLevel level, string str, params object[] args)
        {
            switch (level)
            {
                case NetLogLevel.Trace:
                    Debug.LogFormat(str, args);
                    break;
                case NetLogLevel.Info:
                    Debug.LogFormat(str, args);
                    break;
                case NetLogLevel.Warning:
                    Debug.LogWarningFormat(str, args);
                    break;
                case NetLogLevel.Error:
                    Debug.LogErrorFormat(str, args);
                    break;
            }
        }
        
        ConnectionState _prevClientState = ConnectionState.Disconnected;
        ConnectionState _prevServerState = ConnectionState.Disconnected;
        
        private void TriggerConnectionStateEvent(bool asServer)
        {
            if (asServer)
            {
                if (_prevServerState != listenerState)
                {
                    onConnectionState?.Invoke(listenerState, true);
                    _prevServerState = listenerState;
                }
            }
            else
            {
                if (_prevClientState != clientState)
                {
                    onConnectionState?.Invoke(clientState, false);
                    _prevClientState = clientState;
                }
            }
        }
    }
}
