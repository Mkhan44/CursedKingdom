using System.Collections.Generic;
using JetBrains.Annotations;
using PurrNet.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PurrNet.Modules
{
    public partial class RollbackModule : INetworkModule
    {
        PhysicsScene _physicsScene;
        PhysicsScene2D _physicsScene2D;

        readonly TickManager _tickManager;
        readonly HashSet<Component> _trackedColliders = new();

        private readonly List<Collider> _colliders3D = new();
        private readonly List<Collider2D> _colliders2D = new();

        readonly Dictionary<Collider, SimpleHistory<Collider3DState>> _collider3DStates = new();
        readonly Dictionary<Collider2D, SimpleHistory<Collider2DState>> _collider2DStates = new();

        public RollbackModule(TickManager tick, Scene scene)
        {
            _tickManager = tick;
            _physicsScene = scene.GetPhysicsScene();
            _physicsScene2D = scene.GetPhysicsScene2D();
        }

        public void Enable(bool asServer)
        {
        }

        public void Disable(bool asServer)
        {
        }

        /// <summary>
        /// Tries to get the state of a collider at a precise tick in the past.
        /// </summary>
        [UsedImplicitly]
        public bool TryGetColliderState(double preciseTick, Collider2D collider, out Collider2DState state)
        {
            if (_collider2DStates.TryGetValue(collider, out var history))
            {
                uint tick = (uint)preciseTick;
                uint tickNext = tick + 1;
                float tickFraction = (float)(preciseTick - tick);

                bool hasStateA = history.TryGet(tick, out var stateA);
                bool hasStateB = history.TryGet(tickNext, out var stateB);

                switch (hasStateA)
                {
                    case true when hasStateB:
                        stateA = stateA.Interpolate(stateB, tickFraction);
                        break;
                    case false when hasStateB:
                        stateA = stateB;
                        break;
                    case false:
                    {
                        state = default;
                        return false;
                    }
                    case true:
                        break;
                }

                state = stateA;
                return true;
            }

            state = default;
            return false;
        }

        /// <summary>
        /// Tries to get the state of a collider at a precise tick in the past.
        /// </summary>
        [UsedImplicitly]
        public bool TryGetColliderState(double preciseTick, Collider collider, out Collider3DState state)
        {
            if (_collider3DStates.TryGetValue(collider, out var history))
            {
                uint tick = (uint)preciseTick;
                uint tickNext = tick + 1;
                float tickFraction = (float)(preciseTick - tick);

                bool hasStateA = history.TryGet(tick, out var stateA);
                bool hasStateB = history.TryGet(tickNext, out var stateB);

                switch (hasStateA)
                {
                    case true when hasStateB:
                        stateA = stateA.Interpolate(stateB, tickFraction);
                        break;
                    case false when hasStateB:
                        stateA = stateB;
                        break;
                    case false:
                    {
                        state = default;
                        return false;
                    }
                    case true:
                        break;
                }

                state = stateA;
                return true;
            }

            state = default;
            return false;
        }

        public void OnPostTick()
        {
            for (var i = 0; i < _colliders3D.Count; i++)
            {
                var col = _colliders3D[i];

                if (!col) continue;

                if (!_collider3DStates.TryGetValue(col, out var history))
                {
                    PurrLogger.LogWarning($"Collider '{col.name}' not found in history, " +
                                          $"make sure only one ColliderRollback acts on this collider.", col);
                    continue;
                }

                history.Write(_tickManager.localTick, new Collider3DState(col));
            }

            for (var i = 0; i < _colliders2D.Count; i++)
            {
                var col = _colliders2D[i];

                if (!col) continue;

                if (!_collider2DStates.TryGetValue(col, out var history))
                {
                    PurrLogger.LogWarning($"Collider '{col.name}' not found in history, " +
                                          $"make sure only one ColliderRollback acts on this collider.", col);
                    continue;
                }

                history.Write(_tickManager.localTick, new Collider2DState(col));
            }
        }

        public void Register(ColliderRollback component)
        {
            var colliders3d = component.colliders3D;
            var colliders2d = component.colliders2D;

            int maxEntries = Mathf.CeilToInt(_tickManager.tickRate * component.storeHistoryInSeconds);

            if (colliders3d != null)
            {
                for (var i = 0; i < colliders3d.Length; i++)
                {
                    var collider = colliders3d[i];
                    if (collider == null)
                        continue;

                    _trackedColliders.Add(collider);
                    _collider3DStates.Add(collider, new SimpleHistory<Collider3DState>(maxEntries));
                    _colliders3D.Add(collider);
                }
            }

            if (colliders2d != null)
            {
                for (var i = 0; i < colliders2d.Length; i++)
                {
                    var collider = colliders2d[i];
                    if (collider == null)
                        continue;

                    _trackedColliders.Add(collider);
                    _collider2DStates.Add(collider, new SimpleHistory<Collider2DState>(maxEntries));
                    _colliders2D.Add(collider);
                }
            }
        }

        public void Unregister(ColliderRollback component)
        {
            var colliders3d = component.colliders3D;
            var colliders2d = component.colliders2D;

            if (colliders3d != null)
            {
                for (var i = 0; i < colliders3d.Length; i++)
                {
                    var collider = colliders3d[i];
                    if (collider == null)
                        continue;

                    _trackedColliders.Remove(collider);
                    _collider3DStates.Remove(collider);
                    _colliders3D.Remove(collider);
                }
            }

            if (colliders2d != null)
            {
                for (var i = 0; i < colliders2d.Length; i++)
                {
                    var collider = colliders2d[i];
                    if (collider == null)
                        continue;

                    _trackedColliders.Remove(collider);
                    _collider2DStates.Remove(collider);
                    _colliders2D.Remove(collider);
                }
            }
        }
    }
}