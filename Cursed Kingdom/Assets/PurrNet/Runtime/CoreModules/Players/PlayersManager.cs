using System;
using System.Collections.Generic;
using PurrNet.Authentication;
using PurrNet.Logging;
using PurrNet.Packing;
using PurrNet.Transports;

namespace PurrNet.Modules
{
    public struct ServerLoginResponse : IPackedAuto
    {
        public PlayerID playerId { get; }
        public NetworkID lastNidId { get; }

        public ServerLoginResponse(PlayerID playerId, NetworkID lastNidId)
        {
            this.playerId = playerId;
            this.lastNidId = lastNidId;
        }
    }

    public struct PlayerJoinedEvent : IPackedAuto
    {
        public PlayerID playerId { get; }
        public Connection connection { get; }

        public PlayerJoinedEvent(PlayerID playerId, Connection connection)
        {
            this.playerId = playerId;
            this.connection = connection;
        }
    }

    public struct PlayerLeftEvent : IPackedAuto
    {
        public PlayerID playerId { get; }

        public PlayerLeftEvent(PlayerID playerId)
        {
            this.playerId = playerId;
        }
    }

    public struct PlayerSnapshotEvent : IPackedAuto
    {
        public Dictionary<Connection, PlayerID> connectionToPlayerId { get; }

        public PlayerSnapshotEvent(Dictionary<Connection, PlayerID> connectionToPlayerId)
        {
            this.connectionToPlayerId = connectionToPlayerId;
        }
    }

    public delegate void OnPlayerJoinedEvent(PlayerID player, bool isReconnect, bool asServer);

    public delegate void OnPlayerLeftEvent(PlayerID player, bool asServer);

    public delegate void OnPlayerEvent(PlayerID player);

    public class PlayersManager : INetworkModule, IConnectionListener, IPlayerBroadcaster
    {
        private readonly AuthModule _authModule;
        private readonly BroadcastModule _broadcastModule;
        private readonly ITransport _transport;

        private readonly Dictionary<string, PlayerID> _cookieToPlayerId = new Dictionary<string, PlayerID>();
        private readonly Dictionary<PlayerID, string> _playerIdToCookie = new Dictionary<PlayerID, string>();
        private ushort _playerIdCounter;

        private readonly Dictionary<Connection, PlayerID>
            _connectionToPlayerId = new Dictionary<Connection, PlayerID>();

        private readonly Dictionary<PlayerID, Connection> _playerToConnection = new Dictionary<PlayerID, Connection>();

        private readonly List<PlayerID> _players = new List<PlayerID>();
        private readonly HashSet<PlayerID> _allSeenPlayers = new HashSet<PlayerID>();

        public IReadOnlyList<PlayerID> players => _players;

        public PlayerID? localPlayerId { get; private set; }

        /// <summary>
        /// First callback for whne a new player has joined
        /// </summary>
        public event OnPlayerJoinedEvent onPrePlayerJoined;

        /// <summary>
        /// Callback for when a new player has joined
        /// </summary>
        public event OnPlayerJoinedEvent onPlayerJoined;

        /// <summary>
        /// Last callback for when a new player has joined
        /// </summary>
        public event OnPlayerJoinedEvent onPostPlayerJoined;

        /// <summary>
        /// First callback for when a player has left
        /// </summary>
        public event OnPlayerLeftEvent onPrePlayerLeft;

        /// <summary>
        /// Callback for when a player has left
        /// </summary>
        public event OnPlayerLeftEvent onPlayerLeft;

        /// <summary>
        /// Last callback for when a player has left
        /// </summary>
        public event OnPlayerLeftEvent onPostPlayerLeft;

        /// <summary>
        /// Callback for when the local player has received their PlayerID
        /// </summary>
        public event OnPlayerEvent onLocalPlayerReceivedID;

        public event Action<NetworkID> onNetworkIDReceived;

        private bool _asServer;

        private PlayersBroadcaster _playerBroadcaster;

        internal void SetBroadcaster(PlayersBroadcaster broadcaster)
        {
            _playerBroadcaster = broadcaster;
        }

        public void SendRaw(PlayerID player, ByteData data, Channel method = Channel.ReliableOrdered)
            => _playerBroadcaster.SendRaw(player, data, method);

        public void SendRaw(IEnumerable<PlayerID> player, ByteData data, Channel method = Channel.ReliableOrdered)
            => _playerBroadcaster.SendRaw(player, data, method);

        public void Send<T>(PlayerID player, T data, Channel method = Channel.ReliableOrdered)
            => _playerBroadcaster.Send(player, data, method);

        public void Send<T>(IEnumerable<PlayerID> collection, T data, Channel method = Channel.ReliableOrdered)
            => _playerBroadcaster.Send(collection, data, method);

        public void SendToServer<T>(T data, Channel method = Channel.ReliableOrdered)
            => _playerBroadcaster.SendToServer(data, method);

        public void SendToAll<T>(T data, Channel method = Channel.ReliableOrdered)
            => _playerBroadcaster.SendToAll(data, method);

        public void Unsubscribe<T>(PlayerBroadcastDelegate<T> callback) where T : new()
            => _playerBroadcaster.Unsubscribe(callback);

        public void Subscribe<T>(PlayerBroadcastDelegate<T> callback) where T : new()
            => _playerBroadcaster.Subscribe(callback);

        public PlayersManager(NetworkManager nm, AuthModule auth, BroadcastModule broadcaster)
        {
            _transport = nm.transport.transport;
            _authModule = auth;
            _broadcastModule = broadcaster;
        }

        /// <summary>
        /// Try to get the connection of a playerId.
        /// For bots, this will always return false.
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="conn"></param>
        /// <returns>The network connection tied to this player</returns>
        public bool TryGetConnection(PlayerID playerId, out Connection conn)
        {
            if (playerId.isBot)
            {
                conn = default;
                return false;
            }

            return _playerToConnection.TryGetValue(playerId, out conn);
        }

        /// <summary>
        /// Check if a playerId is connected to the server.
        /// </summary>
        /// <param name="playerId">PlayerID to check</param>
        /// <returns>Whether the player is connected</returns>
        public bool IsPlayerConnected(PlayerID playerId)
        {
            return _playerToConnection.ContainsKey(playerId);
        }

        /// <summary>
        /// Try to get the playerId of a connection.
        /// </summary>
        public bool TryGetPlayer(Connection conn, out PlayerID playerId)
        {
            return _connectionToPlayerId.TryGetValue(conn, out playerId);
        }

        /// <summary>
        /// Check if a playerId is the local player.
        /// </summary>
        public bool IsLocalPlayer(PlayerID playerId)
        {
            return localPlayerId == playerId;
        }

        /// <summary>
        /// Check if a playerId is the local player.
        /// </summary>
        public bool IsLocalPlayer(PlayerID? playerId)
        {
            return localPlayerId == playerId;
        }

        /// <summary>
        /// Check if a playerId is a valid player.
        /// A valid player is a player that is connected to the server.
        /// </summary>
        public bool IsValidPlayer(PlayerID playerId)
        {
            return _players.Contains(playerId);
        }

        /// <summary>
        /// Check if a playerId is a valid player.
        /// A valid player is a player that is connected to the server.
        /// </summary>
        public bool IsValidPlayer(PlayerID? playerId)
        {
            if (!playerId.HasValue)
                return false;
            return _players.Contains(playerId.Value);
        }

        /// <summary>
        /// Create a new bot player and add it to the connected players list.
        /// </summary>
        /// <returns>The playerId of the new bot player</returns>
        public PlayerID CreateBot()
        {
            if (!_asServer)
            {
                throw new InvalidOperationException(PurrLogger.FormatMessage("Cannot create a bot from a client."));
            }

            var playerId = new PlayerID(++_playerIdCounter, true);
            if (RegisterPlayer(default, playerId, out var isReconnect))
            {
                SendNewUserToAllClients(default, playerId);
                TriggerOnJoinedEvent(playerId, isReconnect);
            }

            return playerId;
        }

        /// <summary>
        /// Kick a player from the server.
        /// If the user has a connection, it will be closed.
        /// </summary>
        /// <param name="playerId"></param>
        public void KickPlayer(PlayerID playerId)
        {
            if (_playerToConnection.TryGetValue(playerId, out var conn))
                _transport.CloseConnection(conn);
            UnregisterPlayer(playerId);
            SendUserLeftToAllClients(playerId);
        }

        public void Enable(bool asServer)
        {
            _asServer = asServer;

            if (asServer)
            {
                _authModule.onConnection += OnClientAuthed;
            }
            else
            {
                _broadcastModule.Subscribe<ServerLoginResponse>(OnClientLoginResponse);
                _broadcastModule.Subscribe<PlayerSnapshotEvent>(OnPlayerSnapshotEvent);
                _broadcastModule.Subscribe<PlayerJoinedEvent>(OnPlayerJoinedEvent);
                _broadcastModule.Subscribe<PlayerLeftEvent>(OnPlayerLeftEvent);
            }
        }

        /// <summary>
        /// Try to get the cookie of a playerId.
        /// Good for session management.
        /// </summary>
        public bool TryGetCookie(PlayerID playerId, out string cookie)
        {
            return _playerIdToCookie.TryGetValue(playerId, out cookie);
        }

        private void OnClientAuthed(Connection conn, AuthenticationResponse data)
        {
            if (data.cookie == null || !_cookieToPlayerId.TryGetValue(data.cookie, out var playerId))
            {
                playerId = new PlayerID(++_playerIdCounter, false);

                if (data.cookie != null)
                {
                    _cookieToPlayerId.Add(data.cookie, playerId);
                    _playerIdToCookie.Add(playerId, data.cookie);
                }
            }

            if (_players.Contains(playerId))
            {
                // Player is already connected?
                _transport.CloseConnection(conn);
                PurrLogger.LogError(
                    "Client connected using a cookie from an already connected player; closing their connection.");
                return;
            }

            var lastNidId = new NetworkID(0, playerId);
            if (_lastNidId.TryGetValue(playerId, out var lastNid))
                lastNidId = lastNid;

            _broadcastModule.Send(conn, new ServerLoginResponse(playerId, lastNidId));

            SendSnapshotToClient(conn);
            if (RegisterPlayer(conn, playerId, out var isReconnect))
            {
                SendNewUserToAllClients(conn, playerId);
                TriggerOnJoinedEvent(playerId, isReconnect);
            }
        }

        private void OnPlayerJoinedEvent(Connection conn, PlayerJoinedEvent data, bool asServer)
        {
            if (RegisterPlayer(data.connection, data.playerId, out var isReconnect))
                TriggerOnJoinedEvent(data.playerId, isReconnect);
        }

        private void OnPlayerLeftEvent(Connection conn, PlayerLeftEvent data, bool asServer)
        {
            UnregisterPlayer(data.playerId);
        }

        private void OnPlayerSnapshotEvent(Connection conn, PlayerSnapshotEvent data, bool asServer)
        {
            foreach (var (key, pid) in data.connectionToPlayerId)
            {
                if (RegisterPlayer(key, pid, out var isReconnect))
                    TriggerOnJoinedEvent(pid, isReconnect);
            }
        }

        private void OnClientLoginResponse(Connection conn, ServerLoginResponse data, bool asServer)
        {
            localPlayerId = data.playerId;
            onLocalPlayerReceivedID?.Invoke(data.playerId);
            onNetworkIDReceived?.Invoke(data.lastNidId);
        }

        private void SendNewUserToAllClients(Connection conn, PlayerID playerId)
        {
            _broadcastModule.SendToAll(new PlayerJoinedEvent(playerId, conn));
        }

        private void SendUserLeftToAllClients(PlayerID playerId)
        {
            _broadcastModule.SendToAll(new PlayerLeftEvent(playerId));
        }

        private void SendSnapshotToClient(Connection conn)
        {
            _broadcastModule.Send(conn, new PlayerSnapshotEvent(_connectionToPlayerId));
        }

        private bool RegisterPlayer(Connection conn, PlayerID player, out bool isReconnect)
        {
            if (_connectionToPlayerId.ContainsKey(conn))
            {
                isReconnect = false;
                return false;
            }

            _players.Add(player);

            if (conn.isValid)
            {
                _connectionToPlayerId.Add(conn, player);
                _playerToConnection.Add(player, conn);
            }

            isReconnect = !_allSeenPlayers.Add(player);
            return true;
        }

        private void TriggerOnJoinedEvent(PlayerID player, bool isReconnect)
        {
            onPrePlayerJoined?.Invoke(player, isReconnect, _asServer);
            onPlayerJoined?.Invoke(player, isReconnect, _asServer);
            onPostPlayerJoined?.Invoke(player, isReconnect, _asServer);
        }

        private void UnregisterPlayer(Connection conn)
        {
            if (!_connectionToPlayerId.TryGetValue(conn, out var player))
                return;

            _players.Remove(player);
            _playerToConnection.Remove(player);
            _connectionToPlayerId.Remove(conn);

            onPrePlayerLeft?.Invoke(player, _asServer);
            onPlayerLeft?.Invoke(player, _asServer);
            onPostPlayerLeft?.Invoke(player, _asServer);
        }

        private void UnregisterPlayer(PlayerID playerId)
        {
            if (_playerToConnection.TryGetValue(playerId, out var conn))
                _connectionToPlayerId.Remove(conn);
            _players.Remove(playerId);
            _playerToConnection.Remove(playerId);

            onPrePlayerLeft?.Invoke(playerId, _asServer);
            onPlayerLeft?.Invoke(playerId, _asServer);
            onPostPlayerLeft?.Invoke(playerId, _asServer);
        }

        public void Disable(bool asServer)
        {
            if (asServer)
            {
                _authModule.onConnection -= OnClientAuthed;
            }
            else
            {
                _broadcastModule.Unsubscribe<ServerLoginResponse>(OnClientLoginResponse);
                _broadcastModule.Unsubscribe<PlayerSnapshotEvent>(OnPlayerSnapshotEvent);
                _broadcastModule.Unsubscribe<PlayerJoinedEvent>(OnPlayerJoinedEvent);
                _broadcastModule.Unsubscribe<PlayerLeftEvent>(OnPlayerLeftEvent);
            }
        }

        public void OnConnected(Connection conn, bool asServer)
        {
        }

        public void OnDisconnected(Connection conn, bool asServer)
        {
            if (!asServer) return;

            if (_connectionToPlayerId.TryGetValue(conn, out var playerId))
                SendUserLeftToAllClients(playerId);

            UnregisterPlayer(conn);
        }

        readonly Dictionary<PlayerID, NetworkID> _lastNidId = new Dictionary<PlayerID, NetworkID>();

        public void RegisterClientLastId(PlayerID player, NetworkID lastNidID)
        {
            _lastNidId[player] = lastNidID;
        }
    }
}