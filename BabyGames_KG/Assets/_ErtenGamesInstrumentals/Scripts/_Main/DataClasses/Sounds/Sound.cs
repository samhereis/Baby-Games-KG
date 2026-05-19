using Helpers;
using Interfaces;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Sounds
{
    [Serializable]
    public class Sound : ISound
    {
        [SerializeField] private AudioClip _audioClip;
        [field: SerializeField] public SoundData data { get; private set; }

        public async Task<AudioClip> GetSound()
        {
            await AsyncHelper.NextFrame();
            return _audioClip;
        }

        public void SetSound(AudioClip audioClip)
        {
            _audioClip = audioClip;
        }

        public bool HasAnySound()
        {
            return _audioClip != null;
        }

        public bool HasSoundInList(AudioClip clip)
        {
            return _audioClip == clip;
        }
    }
}