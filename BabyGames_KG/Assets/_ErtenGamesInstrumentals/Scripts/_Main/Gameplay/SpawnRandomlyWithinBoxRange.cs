using Sirenix.OdinInspector;
using UnityEngine;

namespace Helpers
{
    public class SpawnRandomlyWithinRange : MonoBehaviour
    {
        [Required]
        [SerializeField] private Collider[] _colliders;

        private void OnEnable()
        {
            Spawn();
        }

        private async void Spawn()
        {
            await AsyncHelper.DelayFloat(1f);

            Bounds bounds = _colliders[NumberHelper.GetRandom(_colliders.Length)].bounds;

            float offsetX = Random.Range(-bounds.extents.x, bounds.extents.x);
            float offsetY = Random.Range(-bounds.extents.y, bounds.extents.y);
            float offsetZ = Random.Range(-bounds.extents.z, bounds.extents.z);

            transform.position = bounds.center + new Vector3(offsetX, offsetY, offsetZ);
        }

#if UNITY_EDITOR
        [Button]
        private void Setup()
        {
            _colliders = GetComponents<BoxCollider>();
        }
#endif
    }
}