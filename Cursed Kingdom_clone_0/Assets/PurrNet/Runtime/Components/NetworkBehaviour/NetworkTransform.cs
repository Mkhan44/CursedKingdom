using System;
using JetBrains.Annotations;
using PurrNet.Modules;
using PurrNet.Transports;
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

    [Serializable]
    public struct Tolerances
    {
        [Tooltip("How far the position can be from the target position before it is considered out of sync.")]
        [Min(0)] public float positionTolerance;
        [Tooltip("How far the rotation angle can be from the target rotation angle before it is considered out of sync.")]
        [Min(0)] public float rotationAngleTolerance;
        [Tooltip("How far the scale can be from the target scale before it is considered out of sync.")]
        [Min(0)] public float scaleTolerance;
    }
    
    public sealed class NetworkTransform : NetworkIdentity, ITick
    {
        [Header("What to Sync")]
        [Tooltip("Whether to sync the position of the transform. And if so, in what space.")]
        [SerializeField] private SyncMode _syncPosition = SyncMode.World;
        [Tooltip("Whether to sync the rotation of the transform. And if so, in what space.")]
        [SerializeField] private SyncMode _syncRotation = SyncMode.World;
        [Tooltip("Whether to sync the scale of the transform.")]
        [SerializeField] private bool _syncScale = true;
        [Tooltip("Whether to sync the parent of the transform. Only works if the parent is a NetworkIdentiy.")]
        [SerializeField, PurrLock] private bool _syncParent = true;
        
        [Header("How to Sync")]
        [Tooltip("What to interpolate when syncing the transform.")]
        [SerializeField, PurrLock] private TransformSyncMode _interpolateSettings = 
            TransformSyncMode.Position | TransformSyncMode.Rotation | TransformSyncMode.Scale;

        [Header("When to Sync")]
        [FormerlySerializedAs("_clientAuth")]
        [Tooltip("If true, the client can send transform data to the server. If false, the client can't send transform data to the server.")]
        [SerializeField, PurrLock] private bool _ownerAuth = true;
        
        [Tooltip("The interval in ticks to send the transform data. 0 means send every tick.")]
        [SerializeField, Min(0)] private int _sendIntervalInTicks;

        [Tooltip("Will enforce the character controller getting enabled and disabled when attempting to sync the transform - CAUTION - Physics events can/will be called multiple times")] 
        [SerializeField] private bool _characterControllerPatch;

        [SerializeField] private Tolerances _tolerances = new Tolerances()
        {
            rotationAngleTolerance = 1,
            positionTolerance = 0.1f,
            scaleTolerance = 0.05f
        };

        /// <summary>
        /// How far the position can be from the target position before it is considered out of sync and sent over the network.
        /// </summary>
        public Tolerances tolerances
        {
            get => _tolerances;
            set => _tolerances = value;
        }

        /// <summary>
        /// Whether to sync the parent of the transform. Only works if the parent is a NetworkIdentiy.
        /// </summary>
        public bool syncParent => _syncParent;
        
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

        /// <summary>
        /// The interval in ticks to send the transform data. 0 means send every tick, 1 means send every other tick, etc.
        /// </summary>
        public int sendIntervalInTicks
        {
            get => _sendIntervalInTicks;
            set => _sendIntervalInTicks = value;
        }
        
        private bool _isResettingParent;
        private bool _isFirstTransform = true;

        Interpolated<Vector3> _position;
        Interpolated<Quaternion> _rotation;
        Interpolated<Vector3> _scale;

        private Transform _trs;
        private Rigidbody _rb;
        private CharacterController _controller;
        
        private bool _prevWasController;

        static Vector3 NoInterpolation(Vector3 a, Vector3 b, float t) => b;
        
        static Quaternion NoInterpolation(Quaternion a, Quaternion b, float t) => b;

        private void Awake()
        {
            _trs = transform;
            _rb = GetComponent<Rigidbody>();
            _controller = GetComponent<CharacterController>();
        }

        protected override void OnEarlySpawn()
        {
            _trs = transform;
            
            float sendDelta = (_sendIntervalInTicks + 1) * networkManager.tickModule.tickDelta;

            if (syncPosition)
            {
                _position = new Interpolated<Vector3>(interpolatePosition ? Vector3.Lerp : NoInterpolation, sendDelta, 
                    _syncPosition == SyncMode.World ? _trs.position : _trs.localPosition);
            }
            
            if (syncRotation)
                _rotation = new Interpolated<Quaternion>(interpolateRotation ? Quaternion.Lerp : NoInterpolation, sendDelta, 
                    _syncRotation == SyncMode.World ? _trs.rotation : _trs.localRotation);
            
            if (syncScale)
                _scale = new Interpolated<Vector3>(interpolateScale ? Vector3.Lerp : NoInterpolation, sendDelta, _trs.localScale);
        }

        protected override void OnSpawned()
        {
            _isFirstTransform = true;
            int ticksPerSec = networkManager.tickModule.tickRate;
            int ticksPerBuffer = Mathf.CeilToInt(ticksPerSec * 0.15f) * 2;
            
            if (syncPosition) _position.maxBufferSize = ticksPerBuffer;
            if (syncRotation) _rotation.maxBufferSize = ticksPerBuffer;
            if (syncScale) _scale.maxBufferSize = ticksPerBuffer;
        }
        protected override void OnObserverAdded(PlayerID player)
        {
            SendLatestTransform(player, GetCurrentTransformData());
        }

        protected override void OnOwnerReconnected(PlayerID ownerId)
        {
            ReconcileTickId(ownerId, _id);
        }

        protected override void OnOwnerChanged(PlayerID? oldOwner, PlayerID? newOwner, bool asServer)
        {
            if (IsController(_ownerAuth))
                ReconcileTickIdToOthers();
            
            if (!_trs)
                _trs = transform;
            
            _position?.Teleport(_syncPosition == SyncMode.World ? _trs.position : _trs.localPosition);
            _rotation?.Teleport(_syncRotation == SyncMode.World ? _trs.rotation : _trs.localRotation);
            _scale?.Teleport(_trs.localScale);
            
            ApplyLerpedPosition();
        }

        private int _ticksSinceLastSend;
        private bool _wasLastDirty;
        private NetworkTransformData _lastSentData;

        private void ReconcileTickIdToOthers()
        {
            if (!IsController(_ownerAuth))
                return;
            
            if (isServer)
                 ReconcileTickId(_id);
            else ReconcileTickId_Server(_id);
        }
        
        [ServerRpc]
        private void ReconcileTickId_Server(ushort id)
        {
            _id = id;
            
            ReconcileTickId(id);
        }
        
        [ObserversRpc]
        private void ReconcileTickId(ushort id)
        {
            _id = id;
        }
        
        [TargetRpc]
        private void ReconcileTickId([UsedImplicitly] PlayerID player, ushort id)
        {
            _id = id;
        }
        
        private bool _forceNextReliable;
        
        public void OnTick(float delta)
        {
            if (_parentChanged)
            {
                OnTransformParentChangedDelayed();
                _id++;
                _parentChanged = false;
            }
            
            if (IsController(_ownerAuth))
            {
                if (_ticksSinceLastSend >= _sendIntervalInTicks)
                {
                    _ticksSinceLastSend = 0;

                    var data = GetCurrentTransformData();
                    
                    if (_wasLastDirty ? !data.Equals(_lastSentData) : !data.Equals(_lastSentData, _tolerances))
                    {
                        if (_forceNextReliable)
                        {
                            if (isServer)
                                 SendToAllReliable(data, true);
                            else SendTransformToServerReliably(data, true);
                            
                            _forceNextReliable = false;
                        }
                        else
                        {
                            if (isServer)
                                 SendToAll(data, false);
                            else SendTransformToServer(data, false);
                        }

                        _wasLastDirty = true;
                        _lastSentData = data;
                    }
                    else if (_wasLastDirty)
                    {
                        if (isServer)
                             SendToAllReliable(data, false);
                        else SendTransformToServerReliably(data, false);
                        
                        _wasLastDirty = false;
                    }
                }
                else
                {
                    _ticksSinceLastSend++;
                }
            }
        }

        private void FixedUpdate()
        {
            if (_rb && !IsController(_ownerAuth))
                _rb.Sleep();
        }

        private void Update()
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

        private ushort _id;
        
        private NetworkTransformData GetCurrentTransformData()
        {
            var pos = _syncPosition == SyncMode.World ? _trs.position : _trs.localPosition;
            var rot = _syncRotation == SyncMode.World ? _trs.rotation : _trs.localRotation;
            return new NetworkTransformData(_id++, pos, rot, _trs.localScale);
        }
        
        [ServerRpc(Channel.Unreliable, requireOwnership: true)]
        private void SendTransformToServer(NetworkTransformData data, bool resetInterpolation)
        {
            // If clientAuth is disabled, the client can't send transform data to the server
            if (!_ownerAuth) return;
            
            // Apply the transform data to the server
            ReceiveTransform_Internal(data, resetInterpolation);
            
            // Send the transform data to others expect the owner
            SendToOthers(data, resetInterpolation);
        }
        
        [ServerRpc(Channel.ReliableUnordered, requireOwnership: true)]
        private void SendTransformToServerReliably(NetworkTransformData data, bool resetInterpolation)
        {
            // If clientAuth is disabled, the client can't send transform data to the server
            if (!_ownerAuth) return;
            
            // Apply the transform data to the server
            ReceiveTransform_Internal(data, resetInterpolation);
            
            // Send the transform data to others expect the owner
            SendToOthersReliably(data, resetInterpolation);
        }
        
        [ObserversRpc(Channel.ReliableUnordered, excludeOwner: true)]
        private void SendToOthersReliably(NetworkTransformData data, bool resetInterpolation)
        {
            if (isHost) return;
            
            ReceiveTransform_Internal(data, resetInterpolation);
        }
        
        [ObserversRpc(Channel.Unreliable, excludeOwner: true)]
        private void SendToOthers(NetworkTransformData data, bool resetInterpolation)
        {
            if (isHost) return;
            
            ReceiveTransform_Internal(data, resetInterpolation);
        }
        
        [ObserversRpc(Channel.ReliableUnordered)]
        private void SendToAllReliable(NetworkTransformData data, bool resetInterpolation)
        {
            if (isHost) return;

            ReceiveTransform_Internal(data, resetInterpolation);
        }

        [ObserversRpc(Channel.Unreliable)]
        private void SendToAll(NetworkTransformData data, bool resetInterpolation)
        {
            if (isHost) return;

            ReceiveTransform_Internal(data, resetInterpolation);
        }
        
        private void ReceiveTransform_Internal(NetworkTransformData data, bool resetInterpolation)
        {
            if (IsController(_ownerAuth))
                return;

            // if we receive an old transform, ignore it
            if (data.id <= _id)
            {
                return;
            }

            if (resetInterpolation)
            {
                _position?.Teleport(data.position);
                _rotation?.Teleport(data.rotation);
                _scale?.Teleport(data.scale);
            }
            
            // update the last received id in case we switch to a new owner
            // that way the new owner will send the latest transform
            _id = data.id;
            
            if (_isFirstTransform)
            {
                _isFirstTransform = false;
                ApplyTransformData(data, true);
                ApplyLerpedPosition();
            }
            else
            {
                ApplyTransformData(data, false);
            }
        }

        private void ApplyTransformData(NetworkTransformData data, bool teleport)
        {
            if (syncPosition)
            {
                if (teleport) _position.Teleport(data.position);
                else _position.Add(data.position);
            }

            if (syncRotation)
            {
                if (teleport) _rotation.Teleport(data.rotation);
                else _rotation.Add(data.rotation);
            }

            if (syncScale)
            {
                if (teleport) _scale.Teleport(data.scale);
                else _scale.Add(data.scale);
            }
        }

        [TargetRpc]
        private void SendLatestTransform([UsedImplicitly] PlayerID player, NetworkTransformData data)
        {
            if (isServer)
                return;
            
            _id = data.id;
            
            ApplyTransformData(data, true);
            ApplyLerpedPosition();
        }
        
        private bool _parentChanged;

        void OnTransformParentChanged()
        {
            if (!isSpawned)
                return;
            
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
            {
                _forceNextReliable = true;
                HandleParentChanged(_trs.parent);
            }
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
    }
}