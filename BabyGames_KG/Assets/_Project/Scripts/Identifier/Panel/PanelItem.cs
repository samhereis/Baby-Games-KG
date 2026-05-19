using Gameplay;
using Helpers;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;

namespace Identifiers
{
    public class PanelItem : IdentifierBase
    {
        public Transform holder;
        public Vector3 holderScale = Vector3.one;
        public Vector3 initialPosition = new(-1, -2, -3);

        [Button]
        public void Validate()
        {
            holder = transform;
            holderScale = transform.localScale;
            initialPosition = transform.position;
            this.TrySetDirty();
        }

        [Button]
        public void ValidateComponents()
        {
            foreach (var skeletonPartsRenderer in TryGetAll_List<SkeletonPartsRenderer>())
            {
                skeletonPartsRenderer.MeshRenderer.sortingLayerName = Get<MeshRenderer>().sortingLayerName;
            }
        }
    }
}