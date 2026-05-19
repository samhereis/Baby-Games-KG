using DataClasses.AssetReferences;
using Interfaces;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Sounds
{
    [Serializable]
    public class SoundAdvanced : ISound
    {
        [field: SerializeField] private ExternalAssetReference<AudioClip> _audioClipReference = new();
        [field: SerializeField] public SoundData data { get; private set; }
        private AudioClip _audioClip;

        public async Task<AudioClip> GetSound()
        {
            if (_audioClip == null) { _audioClip = await _audioClipReference.GetAssetAsync(); }
            return _audioClip;
        }

        public void SetSound(AudioClip audioClip)
        {
            _audioClip = audioClip;
        }

        public bool HasAnySound()
        {
            return _audioClip != null || _audioClipReference != null;
        }

        public bool HasSoundInList(AudioClip clip)
        {
            return _audioClip == clip || _audioClipReference != null;
        }
    }
}