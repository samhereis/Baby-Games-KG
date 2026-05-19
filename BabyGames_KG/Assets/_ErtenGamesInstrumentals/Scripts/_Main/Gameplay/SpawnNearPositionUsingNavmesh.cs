#if UNITY_2023_2_OR_NEWER

#else
using System.Threading.Tasks;
#endif

using UnityEngine;
using UnityEngine.AI;

namespace Helpers
{
    public class SpawnNearPositionUsingNavmesh
    {
        public static Vector3 GetNearPosition(Vector3 position, float minRadius, float maxRadius)
        {
            float radius = Random.Range(minRadius, maxRadius);

            Vector3 randomDirection = Random.insideUnitSphere * radius;
            randomDirection += position;

            NavMesh.SamplePosition(randomDirection, out var hit, radius, 1);
            Vector3 finalPosition = hit.position;

            return finalPosition;
        }

        public static async
#if UNITY_2023_2_OR_NEWER
            Awaitable<Vector3>
#else
            Task<Vector3>
#endif
            TryGetNearPositionWithAccess(Vector3 position, float minRadius, float maxRadius, int areaMask = 0, int numberOfTries = 20)
        {
            bool foundAccessablePosition = false;
            Vector3 accessablePosition = position;
            int tryCount = 0;

            while (foundAccessablePosition == false)
            {
                var randomNearPosition = GetNearPosition(position, minRadius, maxRadius);

                NavMeshPath navMeshPath = new NavMeshPath();
                if (NavMesh.CalculatePath(randomNearPosition, position, areaMask, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete)
                {
                    accessablePosition = randomNearPosition;
                    foundAccessablePosition = true;

                    Debug.Log("Found accessable target at try: " + tryCount);
                }

                tryCount++;
                if (tryCount >= numberOfTries) { break; }

                await AsyncHelper.Skip();
            }

            Debug.Log("Found accessable target: " + foundAccessablePosition);

            return accessablePosition;
        }
    }
}