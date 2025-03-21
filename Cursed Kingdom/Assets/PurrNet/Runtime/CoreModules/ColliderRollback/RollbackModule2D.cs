using UnityEngine;

namespace PurrNet.Modules
{
    public partial class RollbackModule
    {
        static readonly RaycastHit2D[] _raycastHits2D = new RaycastHit2D[1024];
        static readonly RaycastHit2D[] _raycastHits2DCache = new RaycastHit2D[1024];

        /// <summary>
        /// Casts a ray, from point origin, in direction direction, of length maxDistance, against all colliders in the scene.
        /// </summary>
        public int Raycast(double preciseTick, Ray2D ray, RaycastHit2D[] raycastHits,
            float maxDistance = float.PositiveInfinity,
            ContactFilter2D contactFilter = default)
        {
            if (!_physicsScene2D.IsValid())
                return 0;

            int hitCount = _physicsScene2D.Raycast(ray.origin, ray.direction, maxDistance, contactFilter, raycastHits);
            int colliderCount = _colliders2D.Count;

            // remove any colliders that we are handling manually
            hitCount = FilterColliders(hitCount, raycastHits);

            // handle raycast hits manually
            hitCount = DoManualRaycasts(ray, raycastHits, maxDistance, colliderCount, hitCount, preciseTick,
                contactFilter);

            return hitCount;
        }

        /// <summary>
        /// Casts a ray, from point origin, in direction direction, of length maxDistance, against all colliders in the scene.
        /// </summary>
        public bool Raycast(double preciseTick, Ray2D ray, out RaycastHit2D hit,
            float maxDistance = float.PositiveInfinity,
            ContactFilter2D contactFilter = default)
        {
            if (!_physicsScene2D.IsValid())
            {
                hit = default;
                return false;
            }

            int hitCount =
                _physicsScene2D.Raycast(ray.origin, ray.direction, maxDistance, contactFilter, _raycastHits2D);

            // return the closest hit
            if (hitCount > 0)
            {
                hit = _raycastHits2D[0];
                for (var i = 1; i < hitCount; i++)
                {
                    if (_raycastHits2D[i].distance < hit.distance)
                        hit = _raycastHits2D[i];
                }

                return true;
            }

            hit = default;
            return false;
        }

        private bool RaycastOnly(Collider2D target, Ray2D ray, out RaycastHit2D hit,
            float maxDistance = float.PositiveInfinity,
            ContactFilter2D contactFilter = default)
        {
            if (!_physicsScene2D.IsValid())
            {
                hit = default;
                return false;
            }

            int hitCount = _physicsScene2D.Raycast(ray.origin, ray.direction, maxDistance, contactFilter,
                _raycastHits2DCache);

            // return the closest hit
            if (hitCount > 0)
            {
                hit = _raycastHits2DCache[0];
                for (var i = 1; i < hitCount; i++)
                {
                    var result = _raycastHits2DCache[i];

                    if (result.collider != target)
                        continue;

                    if (result.distance < hit.distance)
                        hit = result;
                }

                return hit.collider == target;
            }

            hit = default;
            return false;
        }

        private int DoManualRaycasts(Ray2D ray, RaycastHit2D[] hits, float maxDistance, int colliderCount,
            int hitCount, double preciseTick, ContactFilter2D contactFilter)
        {
            for (var i = 0; i < colliderCount; i++)
            {
                if (hitCount >= hits.Length)
                    break;

                var col = _colliders2D[i];
                if (!col || !PassesFilters(col, contactFilter))
                    continue;

                if (!TryGetColliderState(preciseTick, col, out var state))
                    continue;

                var trs = col.transform;

                // Get the transform matrix for the historical position
                var rotation = Quaternion.Euler(0, 0, state.rotation);
                var historicalWorldMatrix = Matrix4x4.TRS(state.position, rotation, state.scale);
                var worldToHistorical = historicalWorldMatrix.inverse;

                // Transform world ray to historical local space
                var rayHistoricalLocal = new Ray2D(
                    worldToHistorical.MultiplyPoint3x4(ray.origin),
                    worldToHistorical.MultiplyVector(ray.direction)
                );

                // Transform historical local ray to current world space for the actual raycast
                var currentWorldMatrix = trs.localToWorldMatrix;
                var rayCurrentWorld = new Ray2D(
                    currentWorldMatrix.MultiplyPoint3x4(rayHistoricalLocal.origin),
                    currentWorldMatrix.MultiplyVector(rayHistoricalLocal.direction)
                );

                if (RaycastOnly(col, rayCurrentWorld, out var hit, maxDistance))
                    hits[hitCount++] = hit;
            }

            return hitCount;
        }

        static bool PassesFilters(Collider2D col, ContactFilter2D filter)
        {
            if (!filter.isFiltering)
                return true;

            return !filter.IsFilteringTrigger(col) &&
                   !filter.IsFilteringLayerMask(col.gameObject);
        }

        private int FilterColliders(int hitCount, RaycastHit2D[] hits)
        {
            for (var i = 0; i < hitCount; i++)
            {
                var col = hits[i].collider;
                if (col && _trackedColliders.Contains(col))
                    hits[i--] = hits[--hitCount];
            }

            return hitCount;
        }
    }
}
