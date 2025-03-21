using System.Collections.Generic;
using PurrNet.Packing;
using PurrNet.Transports;

namespace PurrNet.Modules
{
    public struct NetworkTransformDelta : IPackedAuto
    {
        public SceneID scene;
        public readonly ByteData packet;

        public NetworkTransformDelta(SceneID context, BitPacker packer)
        {
            scene = context;
            packet = packer.ToByteData();
        }
    }

    public class NetworkTransformModule : INetworkModule, IFixedUpdate
    {
        private readonly List<NetworkTransform> _networkTransforms = new();
        private readonly ScenePlayersModule _scenePlayers;
        private readonly PlayersBroadcaster _broadcaster;
        private readonly NetworkManager _manager;
        private readonly SceneID _scene;
        private bool _asServer;

        public NetworkTransformModule(NetworkManager manager, PlayersBroadcaster broadcaster,
            ScenePlayersModule scenePlayers, SceneID scene)
        {
            _manager = manager;
            _scenePlayers = scenePlayers;
            _broadcaster = broadcaster;
            _scene = scene;
        }

        public void Enable(bool asServer)
        {
            _asServer = asServer;
            _broadcaster.Subscribe<NetworkTransformDelta>(OnNetworkTransformDelta);
        }

        public void Disable(bool asServer)
        {
            _broadcaster.Unsubscribe<NetworkTransformDelta>(OnNetworkTransformDelta);
        }

        private void OnNetworkTransformDelta(PlayerID player, NetworkTransformDelta data, bool asServer)
        {
            if (data.scene != _scene)
                return;

            using var packet = BitPackerPool.Get(data.packet);

            packet.ResetPositionAndMode(true);

            int ntCount = _networkTransforms.Count;

            if (asServer)
            {
                for (var i = 0; i < ntCount; i++)
                {
                    var nt = _networkTransforms[i];
                    if (nt.IsControlling(player, false))
                        nt.DeltaRead(packet);
                }
            }
            else
            {
                for (var i = 0; i < ntCount; i++)
                {
                    var nt = _networkTransforms[i];
                    if (!nt.IsControlling(nt.localPlayerForced, false))
                        nt.DeltaRead(packet);
                }
            }
        }

        public PlayerID GetLocalPlayer()
        {
            if (_manager.TryGetModule<PlayersManager>(false, out var _players))
                return _players.localPlayerId.GetValueOrDefault();
            return PlayerID.Server;
        }

        private bool PrepareDeltaState(BitPacker packer, PlayerID player)
        {
            var localPlayer = GetLocalPlayer();
            int ntCount = _networkTransforms.Count;
            bool anyWritten = false;

            if (player == PlayerID.Server)
            {
                for (var i = 0; i < ntCount; i++)
                {
                    var nt = _networkTransforms[i];
                    if (nt.IsControlling(localPlayer, false))
                        anyWritten = nt.DeltaWrite(packer) || anyWritten;
                }
            }
            else
            {
                for (var i = 0; i < ntCount; i++)
                {
                    var nt = _networkTransforms[i];
                    if (!nt.IsControlling(player, false) && nt.observers.Contains(player))
                        anyWritten = nt.DeltaWrite(packer) || anyWritten;
                }
            }

            return anyWritten;
        }

        public void Register(NetworkTransform networkTransform)
        {
            if (!networkTransform.id.HasValue)
                return;

            int insertPos = 0;
            var b = networkTransform.id.Value;

            for (var i = 0; i < _networkTransforms.Count; i++)
            {
                if (!_networkTransforms[i].id.HasValue)
                    break;

                var a = _networkTransforms[i].id.Value;
                if (a.scope.id < b.scope.id && a.id < b.id)
                {
                    insertPos = i + 1;
                }
            }

            _networkTransforms.Insert(insertPos, networkTransform);
        }

        public void Unregister(NetworkTransform networkTransform)
        {
            _networkTransforms.Remove(networkTransform);
        }

        public void FixedUpdate()
        {
            var localPlayer = GetLocalPlayer();
            int ntCount = _networkTransforms.Count;

            for (var i = 0; i < ntCount; i++)
            {
                var nt = _networkTransforms[i];
                if (nt.IsControlling(localPlayer, _asServer))
                    nt.GatherState();
            }

            if (!_asServer)
            {
                using var packer = BitPackerPool.Get();

                if (PrepareDeltaState(packer, PlayerID.Server) && packer.positionInBits > 0)
                    _broadcaster.SendToServer(new NetworkTransformDelta(_scene, packer));
            }
            else if (_scenePlayers.TryGetPlayersInScene(_scene, out var players))
            {
                foreach (var player in players)
                {
                    if (player == localPlayer)
                        continue;

                    using var packer = BitPackerPool.Get();

                    if (PrepareDeltaState(packer, player) && packer.positionInBits > 0)
                        _broadcaster.Send(player, new NetworkTransformDelta(_scene, packer));
                }
            }

            for (var i = 0; i < ntCount; i++)
            {
                var nt = _networkTransforms[i];
                if (nt.IsControlling(localPlayer, _asServer))
                    nt.DeltaSave();
            }
        }
    }
}