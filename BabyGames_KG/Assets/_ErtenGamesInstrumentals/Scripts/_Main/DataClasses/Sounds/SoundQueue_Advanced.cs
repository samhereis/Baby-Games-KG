using DataClasses.AssetReferences;
using Helpers;
using Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Sounds
{
    [Serializable]
    public class SoundQueue_Advanced : ISound
    {
        [field: SerializeField] public List<ExternalAssetReference<AudioClip>> sounds = new();
        [field: SerializeField] public SoundData data { get; private set; } = new();

        public bool HasAnySound()
        {
            return sounds.Count > 0;
        }

        public bool HasSoundInList(AudioClip clip)
        {
            return true;
        }

        public async Task<AudioClip> GetSound()
        {
            sounds.Shuffle_Original();
            return await sounds.GetRandom().GetAssetAsync();
        }
    }
}