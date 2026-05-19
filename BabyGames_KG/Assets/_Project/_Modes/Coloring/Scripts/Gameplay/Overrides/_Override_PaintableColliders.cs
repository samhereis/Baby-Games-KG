using DataClasses;
using Helpers;
using Sirenix.OdinInspector;
using Spine.Unity;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Modes.Coloring
{
    public class _Override_PaintableColliders : OverrideBase
    {
        [SerializeField] public List<KeyedObject<string, Vector3>> offsets = new();
        [SerializeField] public List<KeyedObject<string, Vector3>> scales = new();
        [SerializeField] public List<string> ignores = new();
        [SerializeField] public List<string> deleteCollider = new();
        [SerializeField] public bool ignoreAll;

#if UNITY_EDITOR
        [ShowInInspector] public List<SkeletonPartsRenderer> toAddToIgnores { get; private set; } = new();

        [Button]
        public void AddToSnapshottables()
        {
            toAddToIgnores = toAddToIgnores.RemoveDuplicates();

            ignores = toAddToIgnores.Select(x => x.gameObject.name).ToList();
            ignores = ignores.RemoveDuplicates();
        }

        [Button]
        public void AddToScales(List<SkeletonPartsRenderer> toAdd, float scale)
        {
            toAdd = toAdd.RemoveDuplicates();

            scales.Clear();

            foreach (var item in toAdd)
            {
                scales.Add(new KeyedObject<string, Vector3>(item.name, Vector3.one * scale));
            }
        }

        [Button]
        public void UpdateColliders()
        {
            foreach (var item in GetComponentsInChildren<Paintable_Identifier_SlotSeparation>())
            {
                item.UpdateColliders();
            }
        }
#endif
    }
}