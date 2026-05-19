using Helpers;
using Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Sounds
{
    [Serializable]
    public class SoundQueue : ISound
    {
        [field: SerializeField] public List<AudioClip> _sounds = new();
        [field: SerializeField] public SoundData data { get; private set; } = new();

        public bool HasAnySound()
        {
            return _sounds.Count > 0;
        }

        public bool HasSoundInList(AudioClip clip)
        {
            return _sounds.Contains(clip);
        }

        public async Task<AudioClip> GetSound()
        {
            await Task.CompletedTask;

            _sounds.Shuffle_Original();
            return _sounds.GetRandom();
        }
    }
}