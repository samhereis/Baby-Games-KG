using System.Collections.Generic;
using UnityEngine;

namespace Helpers
{
    public static class TransformHelper
    {
        public static Transform FindClosestTransform(this List<Transform> transforms, Vector3 position)
        {
            Transform closest = null;
            float minDistanceSqr = float.MaxValue;

            foreach (var transform in transforms)
            {
                float distanceSqr = (transform.position - position).sqrMagnitude;
                if (distanceSqr < minDistanceSqr)
                {
                    minDistanceSqr = distanceSqr;
                    closest = transform;
                }
            }

            return closest;
        }

        public static Vector3 FindClosestPosition(this List<Vector3> positions, Transform transform)
        {
            Vector3 closest = Vector3.zero;
            float minDistanceSqr = float.MaxValue;

            foreach (var pos in positions)
            {
                float distanceSqr = (pos - transform.position).sqrMagnitude;
                if (distanceSqr < minDistanceSqr)
                {
                    minDistanceSqr = distanceSqr;
                    closest = pos;
                }
            }

            return closest;
        }
    }
}