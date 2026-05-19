using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay
{
    public class PlacesHolder : MonoBehaviour
    {
        public Transform main;
        public List<Transform> secondary;
        public Transform lasNearestPosition;

        public Vector3 position
        {
            get
            {
                return GetNearestPosition(transform.position);
            }
        }

        public Vector3 GetNearestPosition(Vector3 position)
        {
            lasNearestPosition = secondary[0];

            float minSqrDistance = (lasNearestPosition.position - position).sqrMagnitude;

            for (int i = 1; i < secondary.Count; i++)
            {
                float sqrDist = (secondary[i].position - position).sqrMagnitude;
                if (sqrDist < minSqrDistance)
                {
                    minSqrDistance = sqrDist;
                    lasNearestPosition = secondary[i];
                }
            }
            return lasNearestPosition.position;
        }

        [Button]
        private void Setup()
        {
            var list = transform.GetComponentsInChildren<Transform>(true).ToList();
            list.RemoveAt(0);
            secondary = list;
        }
    }
}