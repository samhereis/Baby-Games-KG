using Sounds;
using System.Threading.Tasks;
using UnityEngine;

namespace Interfaces
{
    public interface ISound
    {
        public SoundData data { get; }

        public bool HasAnySound();
        public bool HasSoundInList(AudioClip clip);
        public Task<AudioClip> GetSound();
    }
}