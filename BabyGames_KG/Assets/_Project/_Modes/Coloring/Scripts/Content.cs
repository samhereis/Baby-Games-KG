using System.Collections.Generic;
using DataClasses.AssetReferences;
using Helpers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Modes.Coloring
{
    [CreateAssetMenu(fileName = "Content", menuName = "ScriptableObjects/Content")]
    public class Content : ScriptableObject
    {
        [field: FoldoutGroup("Materials and Shaders"), SerializeField] public Material glitterMaterial_Drawable_Background { get; private set; }
        [field: FoldoutGroup("Materials and Shaders"), SerializeField] public Material glitterMaterial_SlotSeparation { get; private set; }
        [field: FoldoutGroup("Particles"), SerializeField] public ParticleSystem completeParticle { get; private set; }
        [field: FoldoutGroup("Particles"), SerializeField] public List<ExternalAssetReference_HasComponent<ParticleSystem>> mediumConfetti { get; private set; } = new();
        [field: FoldoutGroup("Particles"), SerializeField] public ExternalAssetReference_HasComponent<ParticleSystem> dragParticle_Coloring { get; private set; }
        [field: FoldoutGroup("Particles"), SerializeField] public ExternalAssetReference_HasComponent<ParticleSystem> dragParticle_General { get; private set; }

        [Button]
        public async void Validate()
        {
#if UNITY_EDITOR
            foreach (var item in mediumConfetti)
            {
                var obj = await item.GetAssetAsync();
                foreach (var particleSystem in obj.GetComponentsInChildren<ParticleSystem>())
                {
                    particleSystem.GetComponent<Renderer>().sortingLayerName = "AlwaysOnTop";
                    particleSystem.TrySetDirty();
                }
            }
#endif
        }
    }
}