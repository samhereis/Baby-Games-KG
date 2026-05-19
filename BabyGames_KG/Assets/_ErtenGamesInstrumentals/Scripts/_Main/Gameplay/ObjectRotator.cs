using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay
{
    public class ObjectRotator : MonoBehaviour
    {
        [Header("Rotation")]
        [SerializeField] private Vector3 _rotationValue;

        [Required]
        [SerializeField] private Transform _transformToRotate;

        private void Update()
        {
            _transformToRotate.eulerAngles += _rotationValue * Time.deltaTime;
        }
    }
}