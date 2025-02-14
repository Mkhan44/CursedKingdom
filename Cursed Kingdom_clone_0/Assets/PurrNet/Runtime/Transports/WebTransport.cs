using System;
using System.Collections.Generic;
using System.Security.Authentication;
using JamesFrowen.SimpleWeb;
using UnityEngine;

namespace PurrNet.Transports
{
    public class WebTransport : GenericTransport, ITransport
    {
        [Header("Server Settings")]
        [SerializeField] private ushort _serverPort = 5001;
        [SerializeField] private int _maxConnections = 100;

        [Header("Client Settings")]
        [SerializeField] private string _address = "127.0.0.1";
        [Tooltip("The query to add to the address.\nEx: 'name=John' results in ws://localhost:5001?name=John")]
        [SerializeField] private string _query = "";
        [Tooltip("The path to add to the address.\nEx: '/game' results in ws://localhost:5001/game")]
        [SerializeField] private string _path = "";
        
        [Header("SSL Settings")]
        [SerializeField] private bool _enableSSL;
        [SerializeField] private string _certPath;
        [SerializeField] private string _certPassword;
        [SerializeField] private SslProtocols _sslProtocols;
        
        public event OnConnected onConnected;
        public event OnDisconnected onDisconnected;
        public event OnDataReceived onDataReceived;
        public event OnDataSent onDataSent;
        public event OnConnectionState onConnectionState;

        public string address { get => _address; set => _address = value; }
        
        public string query { get => _query; set => _query = value; }
        
        public string path { get => _path; set => _path = value; }
        
        public ushort serverPort { get => _serverPort; set => _serverPort = value; }
        
        public int maxConnections { get => _maxConnections; set => _maxConnections = value; }
        
        public bool enableSSL { get => _enableSSL; set => _enableSSL = value; }
        
        public string certPath { get => _certPath; set => _certPath = value; }
        
        public string certPassword { get => _certPassword; set => _certPassword = value; }
        
        public SslProtocols sslProtocols { get => _sslProtocols; set => _sslProtocols = value; }

        public IReadOnlyList<Connection> connections => _connections;

        public ConnectionState listenerState { get; private set; } = ConnectionState.Disconnected;

        public ConnectionState clientState { get; private set; } = ConnectionState.Disconnected;
        
        private SimpleWebServer _server;
        private SimpleWebClient _client;

        public bool shouldClientSendKeepAlive => true;
        
        private readonly List<Connection> _connections = new List<Connection>();

        public override ITransport transport => this;
        
        public override bool isSupported => true;

        readonly TcpConfig _tcpConfig = new (noDelay: true, sendTimeout: 0, receiveTimeout: 0);

        private void Awake()
        {
            ReconstructServer();
            
            _client = SimpleWebClient.Create(ushort.MaxValue, 5000, _tcpConfig);
            _client.onConnect += OnClientConnected;
            _client.onDisconnect += OnClientDisconnected;
            _client.onData += OnClientReceivedData;
            _client.onError += OnClientError;
        }
        
        public void RaiseDataReceived(Connection conn, ByteData data, bool asServer)
        {
            onDataReceived?.Invoke(conn, data, asServer);
        }

        public void RaiseDataSent(Connection conn, ByteData data, bool asServer)
        {
            onDataSent?.Invoke(conn, data, asServer);
        }

        private void ReconstructServer()
        {
            _connections.Clear();

            var sslConfig = new SslConfig(_enableSSL, _certPath, _certPassword, _sslProtocols);

            if (_server != null)
            {
                if (_server.Active)
                    _server.Stop();
                
                _server.onConnect -= OnClientConnectedToServer;
                _server.onDisconnect -= OnClientDisconnectedFromServer;
                _server.onData -= OnServerReceivedData;
                _server.onError -= OnServerError;
            }

            _server = new SimpleWebServer(5000, _tcpConfig, ushort.MaxValue, 5000, sslConfig);
            _server.onConnect += OnClientConnectedToServer;
            _server.onDisconnect += OnClientDisconnectedFromServer;
            _server.onData += OnServerReceivedData;
            _server.onError += OnServerError;
        }

        private void OnClientReceivedData(ArraySegment<byte> data)
        {
            var byteData = new ByteData(data.Array, data.Offset, data.Count);
            onDataReceived?.Invoke(new Connection(0), byteData, false);
        }

        private void OnClientDisconnected()
        {
            var wasConnected = clientState == ConnectionState.Connected;
            
            clientState = ConnectionState.Disconnecting;
            TriggerConnectionStateEvent(false);

            if (wasConnected)
                onDisconnected?.Invoke(new Connection(0), DisconnectReason.ClientRequest, false);
            
            clientState = ConnectionState.Disconnected;
            TriggerConnectionStateEvent(false);
        }

        private static void OnClientError(Exception exception)
        {
            Debug.LogException(exception);
        }

        private void OnClientConnected()
        {
            clientState = ConnectionState.Connected;
            TriggerConnectionStateEvent(false);
            
            onConnected?.Invoke(new Connection(0), false);
        }
        
        public void TickUpdate(float delta)
        {
            
        }

        public void UnityUpdate(float delta)
        {
            _server.ProcessMessageQueue();
            _client.ProcessMessageQueue();
        }

        public void Listen(ushort port)
        {
            if (listenerState is ConnectionState.Connected or ConnectionState.Connecting)
                return;

            listenerState = ConnectionState.Connecting;
            TriggerConnectionStateEvent(true);

            _server.Start(port);
            
            listenerState = ConnectionState.Connected;
            TriggerConnectionStateEvent(true);
        }

        protected override void StartServerInternal()
        {
            Listen(_serverPort);
        }

        private static void OnServerError(int clientId, Exception exception)
        {
            Debug.LogException(exception);
        }

        private void OnServerReceivedData(int clientId, ArraySegment<byte> data)
        {
            var byteData = new ByteData(data.Array, data.Offset, data.Count);
            var conn = new Connection(clientId);
            onDataReceived?.Invoke(conn, byteData, true);
        }

        private void OnClientDisconnectedFromServer(int clientId)
        {
            var conn = new Connection(clientId);
            
            for (int i = 0; i < _connections.Count; i++)
            {
                if (_connections[i] == conn)
                {
                    _connections.RemoveAt(i);
                    break;
                }
            }
            
            onDisconnected?.Invoke(conn, DisconnectReason.ClientRequest,true);
        }

        private void OnClientConnectedToServer(int clientId)
        {
            if (_connections.Count >= _maxConnections)
            {
                Debug.LogWarning("Max connections reached. Kicking client.");
                _server.KickClient(clientId);
                return;
            }
            
            var conn = new Connection(clientId);
            _connections.Add(conn);
            onConnected?.Invoke(conn, true);
        }

        public void StopListening()
        {
            if (listenerState is ConnectionState.Disconnecting or ConnectionState.Disconnected)
                return;
            
            listenerState = ConnectionState.Disconnecting;
            TriggerConnectionStateEvent(true);
            
            _server.Stop();
            
            listenerState = ConnectionState.Disconnected;
            TriggerConnectionStateEvent(true);
            
            ReconstructServer();
        }

        public void Connect(string ip, ushort port)
        {
            if (clientState is ConnectionState.Connecting or ConnectionState.Connected)
                return;
            
            var builder = new UriBuilder
            {
                Scheme = _enableSSL ? "wss" : "ws",
                Host = ip,
                Port = port,
                Query = _query,
                Path = _path
            };

            clientState = ConnectionState.Connecting;
            TriggerConnectionStateEvent(false);
            
            _client.Connect(builder.Uri);
            
            clientState = _client.ConnectionState switch
            {
                ClientState.Connected => ConnectionState.Connected,
                ClientState.Connecting => ConnectionState.Connecting,
                ClientState.Disconnecting => ConnectionState.Disconnecting,
                _ => ConnectionState.Disconnected
            };
            
            TriggerConnectionStateEvent(false);
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

        protected override void StartClientInternal()
        {
            Connect(_address, _serverPort);
        }

        public void Disconnect()
        {
            if (clientState is ConnectionState.Disconnecting or ConnectionState.Disconnected)
                return;
            
            _client.Disconnect();
            TriggerConnectionStateEvent(false);
        }

        private void OnDisable()
        {
            StopListening();
            Disconnect();
        }

        public void SendToClient(Connection target, ByteData data, Channel method = Channel.ReliableOrdered)
        {
            if (listenerState != ConnectionState.Connected)
                return;
            
            if (!target.isValid)
                return;
            
            _server.SendOne(target.connectionId, new ArraySegment<byte>(data.data, data.offset, data.length));
            RaiseDataSent(target, data, true);
        }

        public void SendToServer(ByteData data, Channel method = Channel.ReliableOrdered)
        {
            if (clientState != ConnectionState.Connected)
                return;

            _client.Send(new ArraySegment<byte>(data.data, data.offset, data.length));
            RaiseDataSent(default, data, false);
        }

        public void CloseConnection(Connection conn)
        {
            if (listenerState != ConnectionState.Connected)
                return;
            
            if (!conn.isValid)
                return;
            
            _server.KickClient(conn.connectionId);
        }
    }
}
