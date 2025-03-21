using System;
using JetBrains.Annotations;
using PurrNet.Logging;
using PurrNet.Modules;
using PurrNet.Packing;
using PurrNet.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace PurrNet
{
    [Flags]
    [Serializable]
    public enum TransformSyncMode : byte
    {
        [UsedImplicitly] None,
        Position = 1,
        Rotation = 2,
        Scale = 4
    }

    public enum SyncMode : byte
    {
        No,
        World,
        [UsedImplicitly] Local
    }

    public enum InterpolationTiming
    {
        Update,
        LateUpdate
    }

    public sealed class NetworkTransform : NetworkIdentity, ITick
    {
        [Header("What to Sync")]
        [Tooltip("Whether to sync the position of the transform. And if so, in what space.")]
        [SerializeField, PurrLock]
        private SyncMode _syncPosition = SyncMode.World;

        [Tooltip("Whether to sync the rotation of the transform. And if so, in what space.")] [SerializeField, PurrLock]
        private SyncMode _syncRotation = SyncMode.World;

        [Tooltip("Whether to sync the scale of the transform.")] [SerializeField, PurrLock]
        private bool _syncScale = true;

        [Tooltip("Whether to sync the parent of the transform. Only works if the parent is a NetworkIdentiy.")]
        [SerializeField, PurrLock]
        private bool _syncParent = true;

        [Header("How to Sync")] [Tooltip("What to interpolate when syncing the transform.")] [SerializeField, PurrLock]
        private TransformSyncMode _interpolateSettings =
            TransformSyncMode.Position | TransformSyncMode.Rotation | TransformSyncMode.Scale;
        [Tooltip("The minimum amount of buffered ticks to store.\nThis is used for interpolation.")]
        [SerializeField, PurrLock, Min(1)] private int _minBufferSize = 1;
        [Tooltip("The maximum amount of buffered ticks to store.\nThis is used for interpolation.")]
        [SerializeField, PurrLock, Min(1)] private int _maxBufferSize = 2;
        [Tooltip("Will enforce the character controller getting enabled and disabled when attempting to sync the transform - CAUTION - Physics events can/will be called multiple times")]
        [SerializeField]
        private bool _characterControllerPatch;

        [Header("When to Sync")]
        [FormerlySerializedAs("_clientAuth")]
        [Tooltip(
            "If true, the client can send transform data to the server. If false, the client can't send transform data to the server.")]
        [SerializeField, PurrLock]
        private bool _ownerAuth = true;

        [SerializeField]
        private InterpolationTiming _interpolationTiming = InterpolationTiming.Update;

        /// <summary>
        /// Whether to sync the parent of the transform. Only works if the parent is a NetworkIdentiy.
        /// </summary>
        public bool syncParent => _syncParent;

        public int ticksBehind
        {
            get
            {
                if (syncPosition)
                    return _position.bufferSize;
                if (syncRotation)
                    return _rotation.bufferSize;
                if (syncScale)
                    return _scale.bufferSize;
                return 0;
            }
        }

        /// <summary>
        /// Whether to sync the position of the transform.
        /// </summary>
        public bool syncPosition => _syncPosition != SyncMode.No;

        /// <summary>
        /// Whether to sync the rotation of the transform.
        /// </summary>
        public bool syncRotation => _syncRotation != SyncMode.No;

        /// <summary>
        /// Whether to sync the scale of the transform.
        /// </summary>
        public bool syncScale => _syncScale;

        /// <summary>
        /// Whether to interpolate the position of the transform.
        /// </summary>
        public bool interpolatePosition => _interpolateSettings.HasFlag(TransformSyncMode.Position);

        /// <summary>
        /// Whether to interpolate the rotation of the transform.
        /// </summary>
        public bool interpolateRotation => _interpolateSettings.HasFlag(TransformSyncMode.Rotation);

        /// <summary>
        /// Whether to interpolate the scale of the transform.
        /// </summary>
        public bool interpolateScale => _interpolateSettings.HasFlag(TransformSyncMode.Scale);

        /// <summary>
        /// Whether the client controls the transform if they are the owner.
        /// </summary>
        public bool ownerAuth => _ownerAuth;

        private bool _isResettingParent;

        Interpolated<Vector3> _position;
        Interpolated<Quaternion> _rotation;
        Interpolated<Vector3> _scale;

        private Transform _trs;
        private Rigidbody _rb;
        private Rigidbody2D _rb2d;
        private CharacterController _controller;

        private bool _prevWasController;

        static Vector3 NoInterpolation(Vector3 a, Vector3 b, float t) => b;

        static Quaternion NoInterpolation(Quaternion a, Quaternion b, float t) => b;

        private void Awake()
        {
            _trs = transform;
            _rb = GetComponent<Rigidbody>();
            _rb2d = GetComponent<Rigidbody2D>();
            _controller = GetComponent<CharacterController>();
        }

        protected override void OnEarlySpawn(bool asServer)
        {
            _currentData = GetCurrentTransformData();
            _lastSentDelta = _currentData;
        }

        protected override void OnOwnerReconnected(PlayerID ownerId)
        {
            OnOwnerChanged(ownerId, ownerId, isServer);
        }

        protected override void OnOwnerChanged(PlayerID? oldOwner, PlayerID? newOwner, bool asServer)
        {
            if (!_ownerAuth)
                return;

            if (asServer)
            {
                if (newOwner.HasValue && newOwner != localPlayer)
                    SendLatestState(newOwner.Value, _currentData);

                if (oldOwner.HasValue && newOwner != oldOwner && oldOwner != localPlayer)
                    SendLatestState(oldOwner.Value, _currentData);
            }
            else if (newOwner == localPlayer && !isServer)
            {
                _currentData = GetCurrentTransformData();
                SendLatestStateToServer(_currentData);
                _lastSentDelta = _currentData;
            }
        }

        protected override void OnSpawned(bool asServer)
        {
            if (!networkManager.TryGetModule<NetworkTransformFactory>(asServer, out var factory))
            {
                PurrLogger.LogError("NetworkTransformFactory not found");
                return;
            }

            if (!factory.TryGetModule(sceneId, out var ntModule))
                return;

            if (!asServer && !isServer && IsController(localPlayerForced, _ownerAuth, false))
                SendLatestStateToServer(_currentData);

            ntModule.Register(this);
        }

        protected override void OnDespawned(bool asServer)
        {
            if (!networkManager.TryGetModule<NetworkTransformFactory>(asServer, out var factory))
                return;

            if (!factory.TryGetModule(sceneId, out var ntModule))
                return;

            ntModule.Unregister(this);
        }

        protected override void OnEarlySpawn()
        {
            _trs = transform;

            float sendDelta = networkManager.tickModule.tickDelta;

            if (syncPosition)
            {
                _position = new Interpolated<Vector3>(interpolatePosition ? Vector3.Lerp : NoInterpolation, sendDelta,
                    _syncPosition == SyncMode.World ? _trs.position : _trs.localPosition, _maxBufferSize, _minBufferSize);
            }

            if (syncRotation)
                _rotation = new Interpolated<Quaternion>(interpolateRotation ? Quaternion.Lerp : NoInterpolation,
                    sendDelta,
                    _syncRotation == SyncMode.World ? _trs.rotation : _trs.localRotation, _maxBufferSize, _minBufferSize);

            if (syncScale)
                _scale = new Interpolated<Vector3>(interpolateScale ? Vector3.Lerp : NoInterpolation, sendDelta,
                    _trs.localScale, _maxBufferSize, _minBufferSize);
        }

        protected override void OnSpawned()
        {
            int ticksPerSec = networkManager.tickModule.tickRate;
            int ticksPerBuffer = Mathf.CeilToInt(ticksPerSec * 0.15f) * 2;

            if (syncPosition) _position.maxBufferSize = ticksPerBuffer;
            if (syncRotation) _rotation.maxBufferSize = ticksPerBuffer;
            if (syncScale) _scale.maxBufferSize = ticksPerBuffer;
        }

        protected override void OnObserverAdded(PlayerID player)
        {
            if (player == localPlayer)
                return;

            if (!_ownerAuth || player != owner)
                SendLatestState(player, _currentData);
        }

        [ServerRpc]
        private void SendLatestStateToServer(NetworkTransformData data)
        {
            _lastReadData = data;
            _currentData = data;
            TeleportToData(data);
            ApplyLerpedPosition();
        }

        [TargetRpc]
        private void SendLatestState(PlayerID player, NetworkTransformData data)
        {
            _lastReadData = data;
            _currentData = data;
            TeleportToData(data);
            ApplyLerpedPosition();
        }

        public void OnTick(float delta)
        {
            if (_parentChanged)
            {
                OnTransformParentChangedDelayed();
                _parentChanged = false;
            }
        }

        private void FixedUpdate()
        {
            bool isNotController = !IsController(_ownerAuth);

            if (_rb && isNotController)
                _rb.Sleep();

            if (_rb2d && isNotController)
                _rb2d.Sleep();
        }

        private void Update()
        {
            if (_interpolationTiming == InterpolationTiming.Update)
                Interpolate();
        }

        private void LateUpdate()
        {
            if (_interpolationTiming == InterpolationTiming.LateUpdate)
                Interpolate();
        }

        private void Interpolate()
        {
            if (!isSpawned)
                return;

            bool isLocalController = IsController(_ownerAuth);

            if (!isLocalController)
            {
                ApplyLerpedPosition();
            }

            if (_prevWasController != isLocalController)
            {
                if (isLocalController && _rb)
                    _rb.WakeUp();
                if (isLocalController && _rb2d)
                    _rb2d.WakeUp();

                _prevWasController = isLocalController;
            }
        }

        private void ApplyLerpedPosition()
        {
            bool disableController = _controller && _controller.enabled;

            if (disableController && _characterControllerPatch)
                _controller.enabled = false;

            if (syncPosition)
            {
                if (_syncPosition == SyncMode.World)
                    _trs.position = _position.Advance(Time.deltaTime);
                else _trs.localPosition = _position.Advance(Time.deltaTime);
            }

            if (syncRotation)
            {
                if (_syncRotation == SyncMode.World)
                    _trs.rotation = _rotation.Advance(Time.deltaTime);
                else _trs.localRotation = _rotation.Advance(Time.deltaTime);
            }

            if (syncScale)
                _trs.localScale = _scale.Advance(Time.deltaTime);
            if (disableController && _characterControllerPatch)
                _controller.enabled = true;
        }

        private NetworkTransformData GetCurrentTransformData()
        {
            var pos = _syncPosition switch
            {
                SyncMode.World => _trs.position,
                SyncMode.Local => _trs.localPosition,
                _ => Vector3.zero
            };

            var rot = _syncRotation switch
            {
                SyncMode.World => _trs.rotation,
                SyncMode.Local => _trs.localRotation,
                _ => Quaternion.identity
            };

            var scale = _syncScale ? _trs.localScale : default;

            return new NetworkTransformData(pos, rot, scale);
        }

        private bool _parentChanged;

        void OnTransformParentChanged()
        {
            if (!isSpawned)
                return;

            var data = GetCurrentTransformData();
            TeleportLocalToData(data);

            if (_isIgnoringParentChanges)
                return;

            _parentChanged = true;
        }

        void OnTransformParentChangedDelayed()
        {
            if (_isIgnoringParentChanges)
                return;

            if (ApplicationContext.isQuitting)
                return;

            if (!isSpawned)
                return;

            if (!_trs)
                return;

            if (!_isResettingParent && _syncParent)
                HandleParentChanged(_trs.parent);
        }

        private void HandleParentChanged(Transform parent)
        {
            if (networkManager.TryGetModule<HierarchyFactory>(isServer, out var factory) &&
                factory.TryGetHierarchy(sceneId, out var hierarchy))
            {
                hierarchy.OnParentChanged(this, parent);
            }
        }

        private bool _isIgnoringParentChanges;

        public void StartIgnoringParentChanges()
        {
            _isIgnoringParentChanges = true;
        }

        public void StopIgnoringParentChanges()
        {
            _isIgnoringParentChanges = false;
        }

        private void TeleportLocalToData(NetworkTransformData data)
        {
            if (_syncPosition == SyncMode.Local)
                _position.Teleport(data.position);

            if (_syncRotation == SyncMode.Local)
                _rotation.Teleport(data.rotation);

            if (syncScale)
                _scale.Teleport(data.scale);
        }

        private void TeleportToData(NetworkTransformData data)
        {
            if (syncPosition)
                _position.Teleport(data.position);

            if (syncRotation)
                _rotation.Teleport(data.rotation);

            if (syncScale)
                _scale.Teleport(data.scale);
        }

        private void ApplyData(NetworkTransformData data)
        {
            if (syncPosition)
                _position.Add(data.position);

            if (syncRotation)
                _rotation.Add(data.rotation);

            if (syncScale)
                _scale.Add(data.scale);
        }

        private NetworkTransformData _currentData;
        private NetworkTransformData _lastReadData;
        private NetworkTransformData _lastSentDelta;

        public bool DeltaWrite(BitPacker packer)
        {
            int flagPos = packer.AdvanceBits(1);
            bool hasChanged = false;

            if (syncPosition)
                hasChanged = DeltaPacker<Vector3>.Write(packer, _lastSentDelta.position, _currentData.position);

            if (syncRotation)
                hasChanged = DeltaPacker<Quaternion>.Write(packer, _lastSentDelta.rotation, _currentData.rotation) ||
                             hasChanged;

            if (syncScale)
                hasChanged = DeltaPacker<Vector3>.Write(packer, _lastSentDelta.scale, _currentData.scale) || hasChanged;

            packer.WriteAt(flagPos, hasChanged);

            if (!hasChanged)
                packer.SetBitPosition(flagPos + 1);

            return hasChanged;
        }

        public void DeltaRead(BitPacker packet)
        {
            _lastReadData = DeltaRead(packet, _lastReadData);
            ApplyData(_lastReadData);
        }

        NetworkTransformData DeltaRead(BitPacker packet, NetworkTransformData oldValue)
        {
            bool hasChanged = default;

            Packer<bool>.Read(packet, ref hasChanged);

            if (hasChanged)
            {
                var pos = oldValue.position;
                var rot = oldValue.rotation;
                var scale = oldValue.scale;

                if (syncPosition)
                    DeltaPacker<Vector3>.Read(packet, pos, ref oldValue.position);

                if (syncRotation)
                    DeltaPacker<Quaternion>.Read(packet, rot, ref oldValue.rotation);

                if (syncScale)
                    DeltaPacker<Vector3>.Read(packet, scale, ref oldValue.scale);
            }

            return oldValue;
        }

        public void GatherState()
        {
            _currentData = GetCurrentTransformData();

            if (IsController(_ownerAuth))
                TeleportToData(_currentData);
        }

        public void DeltaSave()
        {
            using var packer = BitPackerPool.Get(false);
            DeltaWrite(packer);
            packer.ResetPositionAndMode(true);
            _lastSentDelta = DeltaRead(packer, _lastSentDelta);
        }

        public bool IsControlling(PlayerID player, bool asServer)
        {
            return IsController(player, _ownerAuth, asServer);
        }
    }
}
