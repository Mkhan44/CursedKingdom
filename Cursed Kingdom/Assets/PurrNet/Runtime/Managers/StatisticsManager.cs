using System;
using System.Collections.Generic;
using System.Linq;
using PurrNet.Logging;
using PurrNet.Modules;
using PurrNet.Transports;
using UnityEngine;

namespace PurrNet
{
    public class StatisticsManager : MonoBehaviour
    {
        [Range(0.05f, 1f)] public float checkInterval = 0.33f;
        [SerializeField] private StatisticsPlacement placement = StatisticsPlacement.None;
        [SerializeField] private StatisticsDisplayType displayType = StatisticsDisplayType.All;
        [SerializeField] private float fontSize = 13f;
        [SerializeField] private Color textColor = Color.white;

        public int ping { get; private set; }
        public int jitter { get; private set; }
        public int packetLoss { get; private set; }
        public float upload { get; private set; }
        public float download { get; private set; }

        private NetworkManager _networkManager;
        private PlayersBroadcaster _playersClientBroadcaster;
        private PlayersBroadcaster _playersServerBroadcaster;
        private TickManager _tickManager;
        private GUIStyle _labelStyle;
        private const int PADDING = 10;
        private float LineHeight => fontSize * 1.25f;

        public bool connectedServer { get; private set; }

        public bool connectedClient { get; private set; }

        // Ping stuff
        private readonly Queue<float> _pingHistory = new Queue<float>();
        private readonly Queue<int> _pingStats = new Queue<int>();
        private uint _lastPingSendTick;

        // Packet loss stuff
        private int _packetsToSendPerSec = 10;
        private readonly List<float> _receivedPacketTimes = new List<float>();
        private uint _lastPacketSendTick;

        // Download stuff
        private float _totalDataReceived;
        private float _totalDataSent;
        private float _lastDataCheckTime;

        private void Awake()
        {
            _networkManager = NetworkManager.main;
            _networkManager.onServerConnectionState += OnServerConnectionState;
            _networkManager.onClientConnectionState += OnClientConnectionState;
        }

        private void Start()
        {
            if (!_networkManager)
            {
                PurrLogger.LogError($"StatisticsManager failed to find a NetworkManager in the scene. Disabling...");
                enabled = false;
                return;
            }

            _labelStyle = new GUIStyle
            {
                fontSize = Mathf.RoundToInt(fontSize),
                normal = { textColor = textColor },
                alignment = (placement == StatisticsPlacement.TopRight || placement == StatisticsPlacement.BottomRight)
                    ? TextAnchor.UpperRight
                    : TextAnchor.UpperLeft
            };
        }

        private void OnValidate()
        {
            _labelStyle = new GUIStyle
            {
                fontSize = Mathf.RoundToInt(fontSize),
                normal = { textColor = textColor },
                alignment = (placement == StatisticsPlacement.TopRight || placement == StatisticsPlacement.BottomRight)
                    ? TextAnchor.UpperRight
                    : TextAnchor.UpperLeft
            };
        }

        private void OnDestroy()
        {
            if (_networkManager)
            {
                _networkManager.transport.transport.onDataReceived -= OnDataReceived;
                _networkManager.transport.transport.onDataSent -= OnDataSent;
            }

            if (_playersServerBroadcaster != null)
            {
                _playersServerBroadcaster.Unsubscribe<PingMessage>(ReceivePing);
                _playersServerBroadcaster.Unsubscribe<PacketMessage>(ReceivePacket);
            }

            if (_playersClientBroadcaster != null)
            {
                if (_networkManager.TryGetModule(out TickManager tm, false))
                    tm.onTick -= OnClientTick;

                _playersClientBroadcaster.Unsubscribe<PingMessage>(ReceivePing);
                _playersClientBroadcaster.Unsubscribe<PacketMessage>(ReceivePacket);
            }
        }

        private void OnGUI()
        {
            if (placement == StatisticsPlacement.None || !connectedClient)
                return;

            var position = GetPosition();
            var currentY = position.y;
            var labelWidth = 200;

            if (displayType == StatisticsDisplayType.All || displayType == StatisticsDisplayType.Ping)
            {
                var pingRect = new Rect(position.x, currentY, labelWidth, LineHeight);
                GUI.Label(pingRect, $"Ping: {ping}ms", _labelStyle);
                currentY += LineHeight;

                var jitterRect = new Rect(position.x, currentY, labelWidth, LineHeight);
                GUI.Label(jitterRect, $"Jitter: {jitter}ms", _labelStyle);
                currentY += LineHeight;

                var packetRect = new Rect(position.x, currentY, labelWidth, LineHeight);
                GUI.Label(packetRect, $"Packet Loss: {packetLoss}%", _labelStyle);
                currentY += LineHeight;
            }

            if (displayType == StatisticsDisplayType.All || displayType == StatisticsDisplayType.Usage)
            {
                if (displayType == StatisticsDisplayType.All)
                    currentY += LineHeight / 2;

                var uploadRect = new Rect(position.x, currentY, labelWidth, LineHeight);
                GUI.Label(uploadRect, $"Upload: {upload}KB/s", _labelStyle);
                currentY += LineHeight;

                var downloadRect = new Rect(position.x, currentY, labelWidth, LineHeight);
                GUI.Label(downloadRect, $"Download: {download}KB/s", _labelStyle);
            }
        }

        private Vector2 GetPosition()
        {
            var x = placement switch
            {
                StatisticsPlacement.TopLeft or StatisticsPlacement.BottomLeft => PADDING,
                _ => Screen.width - 200 - PADDING
            };

            var y = placement switch
            {
                StatisticsPlacement.TopLeft or StatisticsPlacement.TopRight => PADDING,
                _ => Screen.height - GetStatsHeight() - PADDING
            };

            return new Vector2(x, y);
        }

        private int GetStatsHeight()
        {
            return displayType switch
            {
                StatisticsDisplayType.Ping => (int)LineHeight * 3,
                StatisticsDisplayType.Usage => (int)LineHeight * 2,
                StatisticsDisplayType.All => (int)LineHeight * 6,
                _ => 0
            };
        }

        private void Update()
        {
            if (Time.time - _lastDataCheckTime >= 1f)
            {
                download = Mathf.Round((_totalDataReceived / 1024f) * 1000f) / 1000f;
                upload = Mathf.Round((_totalDataSent / 1024f) * 1000f) / 1000f;
                _totalDataReceived = 0;
                _totalDataSent = 0;
                _lastDataCheckTime = Time.time;
            }
        }

        private void OnServerConnectionState(ConnectionState state)
        {
            _playersServerBroadcaster = _networkManager.GetModule<PlayersBroadcaster>(true);

            connectedServer = state == ConnectionState.Connected;

            if (state != ConnectionState.Connected)
            {
                _playersServerBroadcaster.Unsubscribe<PingMessage>(ReceivePing);
                _playersServerBroadcaster.Unsubscribe<PacketMessage>(ReceivePacket);
                _networkManager.transport.transport.onDataReceived -= OnDataReceived;
                _networkManager.transport.transport.onDataSent -= OnDataSent;
                return;
            }

            _playersServerBroadcaster.Subscribe<PingMessage>(ReceivePing);
            _playersServerBroadcaster.Subscribe<PacketMessage>(ReceivePacket);
            _networkManager.transport.transport.onDataReceived += OnDataReceived;
            _networkManager.transport.transport.onDataSent += OnDataSent;
        }

        private void OnClientConnectionState(ConnectionState state)
        {
            _tickManager = _networkManager.GetModule<TickManager>(false);
            _playersClientBroadcaster = _networkManager.GetModule<PlayersBroadcaster>(false);

            connectedClient = state == ConnectionState.Connected;

            if (state != ConnectionState.Connected)
            {
                _playersClientBroadcaster.Unsubscribe<PingMessage>(ReceivePing);
                _playersClientBroadcaster.Unsubscribe<PacketMessage>(ReceivePacket);
                _tickManager.onTick -= OnClientTick;
                if (!connectedServer)
                {
                    _networkManager.transport.transport.onDataReceived -= OnDataReceived;
                    _networkManager.transport.transport.onDataSent -= OnDataSent;
                }
                return;
            }

            _playersClientBroadcaster.Subscribe<PingMessage>(ReceivePing);
            _playersClientBroadcaster.Subscribe<PacketMessage>(ReceivePacket);
            _tickManager.onTick += OnClientTick;

            if (!connectedServer)
            {
                _networkManager.transport.transport.onDataReceived += OnDataReceived;
                _networkManager.transport.transport.onDataSent += OnDataSent;
            }

            if (_tickManager.tickRate < _packetsToSendPerSec)
                _packetsToSendPerSec = _tickManager.tickRate;
        }

        private void OnClientTick()
        {
            if (!gameObject.activeInHierarchy)
                return;

            HandlePingCheck();
            HandlePacketCheck();
        }

        private void HandlePingCheck()
        {
            if (_lastPingSendTick + _tickManager.TimeToTick(checkInterval) > _tickManager.localTick)
                return;

            _pingHistory.Enqueue(Time.time);
            SendPingCheck();
        }

        private void SendPingCheck()
        {
            _playersClientBroadcaster.SendToServer(
                new PingMessage {
                    sendTime = _tickManager.localTick,
                    realSendTime = Time.time
                },
                Channel.ReliableUnordered);
            _lastPingSendTick = _tickManager.localTick;
        }

        private void ReceivePing(PlayerID sender, PingMessage msg, bool asServer)
        {
            if (asServer)
            {
                _playersServerBroadcaster.Send(sender,
                    new PingMessage {
                        sendTime = msg.sendTime,
                        realSendTime = msg.realSendTime
                    },
                    Channel.ReliableUnordered);
                return;
            }

            if (_pingHistory.Count > 0)
            {
                float sentTime = msg.realSendTime;
                int currentPing = Mathf.Max(0, Mathf.FloorToInt((Time.time - sentTime) * 1000));
                currentPing -= Mathf.Min(currentPing, Mathf.RoundToInt((_tickManager.tickDelta * 3) * 1000));

                _pingHistory.Dequeue();

                if (_pingStats.Count >= 10)
                    _pingStats.Dequeue();

                _pingStats.Enqueue(currentPing);

                ping = (int)_pingStats.Average();

                if (_pingStats.Count > 1)
                    jitter = Mathf.RoundToInt(_pingStats.Select(p => Mathf.Abs(p - (float)ping)).Average());
                else
                    jitter = 0;
            }
        }

        private int _suspiciousLowPingCount;
        private int _previousValidPing;

        private uint _packetSequence;
        private int _packetsSent = 0;
        private int _packetsReceived = 0;
        private float _packetLossCalculationTime = 0f;

        private void HandlePacketCheck()
        {
            float currentTime = Time.time;

            int removeCount = 0;
            foreach (float packetTime in _receivedPacketTimes)
            {
                if (packetTime < currentTime - 1)
                    removeCount++;
                else
                    break;
            }

            if (removeCount > 0)
                _receivedPacketTimes.RemoveRange(0, removeCount);

            if (_lastPacketSendTick + _tickManager.TimeToTick(1f / _packetsToSendPerSec) > _tickManager.localTick)
                return;

            _lastPacketSendTick = _tickManager.localTick;
            _packetsSent++;
            _playersClientBroadcaster.SendToServer(new PacketMessage { sequenceId = _packetSequence++ }, Channel.Unreliable);

            if (currentTime - _packetLossCalculationTime >= 1f)
            {
                if (_packetsSent > 0)
                {
                    packetLoss = 100 - Mathf.FloorToInt((_packetsReceived / (float)_packetsSent) * 100);

                    if (_tickManager.localTick < 5 * _tickManager.tickRate)
                        packetLoss = 0;
                }

                _packetsSent = 0;
                _packetsReceived = 0;
                _packetLossCalculationTime = currentTime;
            }
        }

        private void ReceivePacket(PlayerID sender, PacketMessage msg, bool asServer)
        {
            if (asServer)
            {
                _playersServerBroadcaster.Send(sender, new PacketMessage { sequenceId = msg.sequenceId }, Channel.Unreliable);
                return;
            }

            _receivedPacketTimes.Add(Time.time);
            _packetsReceived++;
        }

        private void OnDataReceived(Connection conn, ByteData data, bool asServer)
        {
            _totalDataReceived += data.length;
        }

        private void OnDataSent(Connection conn, ByteData data, bool asServer)
        {
            _totalDataSent += data.length;
        }

        public struct PingMessage : Packing.IPackedAuto
        {
            public uint sendTime;
            public float realSendTime;
        }

        public struct PacketMessage : Packing.IPackedAuto
        {
            public uint sequenceId;
        }

        public enum StatisticsPlacement
        {
            None,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        public enum StatisticsDisplayType
        {
            Ping,
            Usage,
            All
        }
    }
}
