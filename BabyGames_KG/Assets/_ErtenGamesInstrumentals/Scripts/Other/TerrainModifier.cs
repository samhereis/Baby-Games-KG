using Helpers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Experimental
{
    public sealed class TerrainModifier : MonoBehaviour
    {
        [Required]
        [SerializeField] private Terrain _terrain;

        private float[,] _heights;
        private float[,] _initialHeights;

        private void Awake()
        {
            _initialHeights = _terrain.terrainData.GetHeights(0, 0, _terrain.terrainData.heightmapResolution, _terrain.terrainData.heightmapResolution);
            _heights = _terrain.terrainData.GetHeights(0, 0, _terrain.terrainData.heightmapResolution, _terrain.terrainData.heightmapResolution);
        }

        private void OnDisable()
        {
            _terrain.terrainData.SetHeights(0, 0, _initialHeights);
        }

        public async void Deform(Vector3 point)
        {
            point = point - transform.position;

            point.x = (point.x / _terrain.terrainData.size.x);
            point.y = (2 / _terrain.terrainData.size.y);
            point.z = (point.z / _terrain.terrainData.size.z);

            int mouseX = (int)(point.z * _terrain.terrainData.heightmapResolution);
            int mouseZ = (int)(point.x * _terrain.terrainData.heightmapResolution);

            try
            {
                _heights[mouseX, mouseZ] = point.y;
                _terrain.terrainData.SetHeights(0, 0, _heights);
            }
            catch { Debug.LogWarning("Ërrot: " + _heights.Length + "  " + mouseX + "  " + mouseZ + " " + _terrain.terrainData.heightmapResolution); }

            await AsyncHelper.Skip();
        }

        [Button]
        private void Setup()
        {
            if (_terrain == null) _terrain = GetComponent<Terrain>();
        }
    }
}