using DataClasses;
using Sirenix.OdinInspector;
using Spine.Unity;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Modes.Coloring
{
    [RequireComponent(typeof(Spine_Identifier))]
    [DisallowMultipleComponent]
    public class _Override_SkeletonPartsRenderer : OverrideBase
    {
        public List<KeyedObject<string, string>> layer = new();
        public List<KeyedObject<string, int>> sortOrder = new();
        private Spine_Identifier _spine_Identifier;

        public override async Task Initialize()
        {
            await base.Initialize();

            _spine_Identifier = GetComponent<Spine_Identifier>();
            List<SkeletonPartsRenderer> all = _spine_Identifier.TryGetAll_List<SkeletonPartsRenderer>();

            foreach (var item in layer)
            {
                SkeletonPartsRenderer skeletonPartsRenderer = all.Find(x => x.name == item.key);
                if (skeletonPartsRenderer != null) { skeletonPartsRenderer.MeshRenderer.sortingLayerName = item.value; }
            }

            foreach (var item in sortOrder)
            {
                SkeletonPartsRenderer skeletonPartsRenderer = all.Find(x => x.name == item.key);
                if (skeletonPartsRenderer != null) { skeletonPartsRenderer.MeshRenderer.sortingOrder = item.value; }
            }
        }
    }
}