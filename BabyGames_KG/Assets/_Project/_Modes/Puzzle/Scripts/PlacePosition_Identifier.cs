using UnityEngine;

namespace Modes.Puzzle
{
    public class PlacePosition_Identifier : MonoBehaviour
    {
        [SerializeField] private Transform minPosition;
        [SerializeField] private Transform maxPosition;

        public Vector3 GetPosition()
        {
            Vector3 position = transform.position;
            position.x = Random.Range(minPosition.position.x, maxPosition.position.x);

            return position;
        }
    }
}