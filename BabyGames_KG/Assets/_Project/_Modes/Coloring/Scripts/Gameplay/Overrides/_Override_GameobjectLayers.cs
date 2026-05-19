using Helpers;
using Sirenix.OdinInspector;
using Spine.Unity;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Modes.Coloring
{
    [DisallowMultipleComponent]
    public class _Override_GameobjectLayers : OverrideBase
    {
        public bool makeAllSnapshottable;
        public List<string> snapshottables;

#if UNITY_EDITOR
        [ShowInInspector] public List<SkeletonPartsRenderer> toAddToSpapshottables { get; private set; }

        [Button]
        public void AddToSnapshottables()
        {
            toAddToSpapshottables = toAddToSpapshottables.RemoveDuplicates();

            snapshottables = toAddToSpapshottables.Select(x => x.gameObject.name).ToList();
            snapshottables = snapshottables.RemoveDuplicates();
        }
#endif
    }
}