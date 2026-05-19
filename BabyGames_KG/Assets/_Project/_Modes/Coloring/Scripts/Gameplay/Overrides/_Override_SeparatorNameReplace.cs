using DataClasses;
using Identifiers;
using Spine.Unity;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Modes.Coloring
{
    [RequireComponent(typeof(Spine_Identifier))]
    [DisallowMultipleComponent]
    public class _Override_SeparatorNameReplace : OverrideBase
    {
        [SerializeField] private List<KeyedObject<string, string>> _replaceData = new();
        private Spine_Identifier _spine_Identifier;

        public override Task Initialize()
        {
            _spine_Identifier = GetComponent<Spine_Identifier>();
            List<SkeletonPartsRenderer> all = _spine_Identifier.TryGetAll_List<SkeletonPartsRenderer>();

            foreach (var item in _replaceData)
            {
                SkeletonPartsRenderer skeletonPartsRenderer = all.Find(x => x.name == item.key);
                if (skeletonPartsRenderer != null) { skeletonPartsRenderer.name = item.value; }
            }

            return base.Initialize();
        }
    }
}