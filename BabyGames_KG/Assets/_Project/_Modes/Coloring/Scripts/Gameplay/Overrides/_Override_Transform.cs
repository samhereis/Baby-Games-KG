using DataClasses;
using Sirenix.OdinInspector;
using Spine.Unity;
using System.Collections.Generic;
using UnityEngine;

namespace Modes.Coloring
{
    [RequireComponent(typeof(Spine_Identifier))]
    [DisallowMultipleComponent]
    public class _Override_Transform : OverrideBase
    {
        [SerializeField] private List<KeyedObject<string, Vector3>> _position = new();
        [SerializeField] private List<KeyedObject<string, Vector3>> _rotation = new();
        [SerializeField] private List<KeyedObject<string, Vector3>> _scale = new();
        private Spine_Identifier _spine_Identifier;

        [Button]
        public void SetEyes()
        {
            _spine_Identifier = GetComponent<Spine_Identifier>();
            List<SkeletonPartsRenderer> all = _spine_Identifier.TryGetAll_List<SkeletonPartsRenderer>();

            foreach (var item in _position)
            {
                SkeletonPartsRenderer skeletonPartsRenderer = all.Find(x => x.name == item.key);
                if (skeletonPartsRenderer != null) { skeletonPartsRenderer.transform.localPosition = item.value; }
            }

            foreach (var item in _rotation)
            {
                SkeletonPartsRenderer skeletonPartsRenderer = all.Find(x => x.name == item.key);
                if (skeletonPartsRenderer != null) { skeletonPartsRenderer.transform.localEulerAngles = item.value; }
            }

            foreach (var item in _scale)
            {
                SkeletonPartsRenderer skeletonPartsRenderer = all.Find(x => x.name == item.key);
                if (skeletonPartsRenderer != null) { skeletonPartsRenderer.transform.localScale = item.value; }
            }
        }
    }
}