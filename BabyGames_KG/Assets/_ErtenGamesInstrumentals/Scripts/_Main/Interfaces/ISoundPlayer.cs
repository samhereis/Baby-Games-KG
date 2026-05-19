using Interfaces;
using System;
using UnityEngine;

namespace DataClasses
{
    public interface ISoundPlayer
    {
        public event Action<ISound> onCompletedPlaying;

        public void TryPlay(ISound sound, AudioClip audioClip = null);
        public void Stop(ISound sound, AudioClip audioClip = null);
        public void Pause(ISound sound, AudioClip audioClip = null);
        public void Resume(ISound sound, AudioClip audioClip = null);
    }
}