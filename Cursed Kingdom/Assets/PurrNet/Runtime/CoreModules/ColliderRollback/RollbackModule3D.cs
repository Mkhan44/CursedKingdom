using UnityEngine;

namespace PurrNet.Modules
{
    public partial class RollbackModule
    {
        static readonly RaycastHit[] _raycastHits = new RaycastHit[1024];

        /// <summary>
        /// Casts a ray, from point origin, in direction direction, of length maxDistance, against all colliders in the scene.
        /// </summary>
        public int Raycast(double preciseTick, Ray ray, RaycastHit[] raycastHits,
            float maxDistance = float.PositiveInfinity,
            int layerMask = Physics.AllLayers,
            QueryTriggerInteraction queryTriggers = QueryTriggerInteraction.UseGlobal)
        {
            if (!_physicsScene.IsValid())
                return 0;

            int hitCount = _physicsScene.Raycast(ray.origin, ray.direction, raycastHits, maxDistance, layerMask,
                queryTriggers);
            int colliderCount = _colliders3D.Count;

            // remove any colliders that we are handling manually
            hitCount = FilterColliders(hitCount, raycastHits);

            // handle raycast hits manually
            hitCount = DoManualRaycasts(ray, raycastHits, maxDistance, layerMask, colliderCount, hitCount, preciseTick,
                queryTriggers);

            return hitCount;
        }

        /// <summary>
        /// Casts a ray, from point origin, in direction direction, of length maxDistance, against all colliders in the scene.
        /// </summary>
        public bool Raycast(double preciseTick, Ray ray, out RaycastHit hit,
            float maxDistance = float.PositiveInfinity,
            int layerMask = Physics.AllLayers,
            QueryTriggerInteraction queryTriggers = QueryTriggerInteraction.UseGlobal)
        {
            if (!_physicsScene.IsValid())
            {
                hit = default;
                return false;
            }

            int hitCount = Raycast(preciseTick, ray, _raycastHits, maxDistance, layerMask, queryTriggers);

            // return the closest hit
            if (hitCount > 0)
            {
                hit = _raycastHits[0];
                for (var i = 1; i < hitCount; i++)
                {
                    if (_raycastHits[i].distance < hit.distance)
                        hit = _raycastHits[i];
                }

                return true;
            }

            hit = default;
            return false;
        }

        private int DoManualRaycasts(Ray ray, RaycastHit[] hits, float maxDistance, int layerMask, int colliderCount,
            int hitCount, double preciseTick, QueryTriggerInteraction queryTriggers)
        {
            if (queryTriggers == QueryTriggerInteraction.UseGlobal)
                queryTriggers = Physics.queriesHitTriggers
                    ? QueryTriggerInteraction.Collide
                    : QueryTriggerInteraction.Ignore;

            for (var i = 0; i < colliderCount; i++)
            {
                if (hitCount >= hits.Length)
                    break;

                var col = _colliders3D[i];

                if (!col)
                    continue;

                if (col.isTrigger && queryTriggers == QueryTriggerInteraction.Ignore)
                    continue;

                bool isPartOfLayerMask = ((1 << col.gameObject.layer) & layerMask) != 0;

                if (!isPartOfLayerMask)
                    continue;

                if (!TryGetColliderState(preciseTick, col, out var state))
                    continue;

                var trs = col.transform;

                // Get the transform matrix for the historical position
                var historicalWorldMatrix = Matrix4x4.TRS(state.position, state.rotation, state.scale);
                var worldToHistorical = historicalWorldMatrix.inverse;

                // Transform world ray to historical local space
                var rayHistoricalLocal = new Ray(
                    worldToHistorical.MultiplyPoint3x4(ray.origin),
                    worldToHistorical.MultiplyVector(ray.direction)
                );

                // Transform historical local ray to current world space for the actual raycast
                var currentWorldMatrix = trs.localToWorldMatrix;
                var rayCurrentWorld = new Ray(
                    currentWorldMatrix.MultiplyPoint3x4(rayHistoricalLocal.origin),
                    currentWorldMatrix.MultiplyVector(rayHistoricalLocal.direction)
                );

                if (col.Raycast(rayCurrentWorld, out var hit, maxDistance))
                {
                    // Transform hit from current world space to current local space
                    var currentToLocal = trs.worldToLocalMatrix;
                    hit.point = currentToLocal.MultiplyPoint3x4(hit.point);
                    hit.normal = currentToLocal.MultiplyVector(hit.normal);

                    // Transform hit from current local space to historical world space
                    hit.point = historicalWorldMatrix.MultiplyPoint3x4(hit.point);
                    hit.normal = historicalWorldMatrix.MultiplyVector(hit.normal);

                    hits[hitCount++] = hit;
                }
            }

            return hitCount;
        }

        private int FilterColliders(int hitCount, RaycastHit[] hits)
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
