using _Project.Scripts.Sound;
using CustomAttributes;
using DataClasses.AssetReferences;
using Loggers;
using Modes.Sorting;
using System;
using UnityEngine;

namespace _Project._Modes.Orchestra.Scripts.SO
{
    [CreateAssetMenu(fileName = nameof(Orchestra_Data), menuName = "Scriptables/ModeData/" + nameof(Orchestra_Data))]
    public class Orchestra_Data : ScriptableObject
    {
        [field: SerializeField] public ExternalAssetReference_HasComponent<DropZone_Identifier> dropZone_Identifier;

        [field: SerializeField] public ExternalAssetReference_HasComponent<ParticleSystem> confetti;

        [field: SerializeField] public ExternalAssetReference_HasComponent<ParticleSystem> transition;

        [field: SerializeField, Fg_Se] public float farmOrchestra_delayBeforeFinalAnimation { get; private set; } = 1.5f;

        public async void MakeConfetti()
        {
            try
            {
                Sound_FX.Play_Static(Sound_Effect.Success);
                var confetti = await transition.InstantiateAsync();
                confetti.transform.position = Vector3.zero;
                confetti.Play();
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }
        }
    }
}