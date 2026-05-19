using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Helpers
{
    [DisallowMultipleComponent]
    public class GraphicHelper : MonoBehaviour
    {
        [SerializeField] private List<MeshRenderer> _meshRenderers = new List<MeshRenderer>();

        [ContextMenu(nameof(FindAllMeshRenderers))]
        public void FindAllMeshRenderers()
        {
            _meshRenderers = GetComponentsInChildren<MeshRenderer>(true).ToList();
            this.TrySetDirty();
        }

        [ContextMenu(nameof(DisableAllShadows))]
        public async void DisableAllShadows()
        {
            foreach (MeshRenderer meshRenderer in _meshRenderers)
            {
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                meshRenderer.TrySetDirty();
                await AsyncHelper.Skip();
            }
        }

        [ContextMenu(nameof(DisableAllRecieveShadows))]
        public async void DisableAllRecieveShadows()
        {
            foreach (MeshRenderer meshRenderer in _meshRenderers)
            {
                meshRenderer.receiveShadows = false;
                meshRenderer.TrySetDirty();
                await AsyncHelper.Skip();
            }
        }

        [ContextMenu(nameof(DisableAllColliders))]
        public async void DisableAllColliders()
        {
            foreach (Transform transformObject in GetComponentsInChildren<Transform>(true))
            {
                var collider = transformObject.GetComponent<Collider>();

                if (collider == null) { continue; }

                DestroyImmediate(collider);
                transform.TrySetDirty();

                await AsyncHelper.Skip();
            }
        }
    }
}