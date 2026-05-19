using _Project.Scripts.SO.Configs;
using DataClasses;
using Interfaces;
using Loggers;
using Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Sound
{
    public class Sound_FX : MonoBehaviour
    {
        public static ISoundPlayer audioPlayer
        {
            get
            {
                if (_audioPlayer == null) { _audioPlayer = DiService.Get<ISoundPlayer>(); }
                return _audioPlayer;
            }
            set
            {
                _audioPlayer = value;
            }
        }

        public static SoundConfig_SO soundConfig
        {
            get
            {
                if (_soundConfig == null) { _soundConfig = DiService.Get<SoundConfig_SO>(); }
                return _soundConfig;
            }
            set
            {
                _soundConfig = value;
            }
        }


        private static SoundConfig_SO _soundConfig;
        private static ISoundPlayer _audioPlayer;

        [SerializeField] private List<KeyedObject<Sound_Effect, Sounds.Sound>> _forceSound = new();
        [SerializeField] private string _additionalData = "Default";

        private void OnEnable()
        {
            try
            {
                if (SceneManager.GetActiveScene().name.Contains("Cooking"))
                {
                    _additionalData = "Cooking";
                }
            }
            catch (Exception e)
            {
                CustomLogger.instance?.LogException(e);
            }
        }

        public async void Play(Sound_Effect soundType, string additionalData = null)
        {
            await PlayAsync(soundType, additionalData);
        }

        public async Task<AudioClip> PlayAsync(Sound_Effect soundType, string additionalData = null)
        {
            AudioClip audioClip = null;

            try
            {
                if (_forceSound.Count > 0 && _forceSound.Exists(x => x.key == soundType))
                {
                    var audio = _forceSound.Find(x => x.key == soundType);
                    audioClip = await audio?.value?.GetSound();
                    if (audioClip != null)
                    {
                        audioPlayer.TryPlay(audio.value, audioClip);
                    }
                    else
                    {
                        var sound = soundConfig.GetSound(soundType, additionalData);
                        audioClip = await sound.GetSound();
                    }
                }
                else
                {
                    audioClip = await PlayAsync_Static(soundType, additionalData);
                }
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }

            return audioClip;
        }

        public static async void Play_Static(Sound_Effect soundType, string additionalData = null)
        {
            try
            {
                if (SceneManager.GetActiveScene().name.Contains("Cooking"))
                {
                    additionalData = "Cooking";
                }
            }
            catch (Exception e)
            {
                CustomLogger.instance?.LogException(e);
            }

            AudioClip audioClip = null;

            try
            {
                var sound = soundConfig.GetSound(soundType, additionalData);
                audioClip = await sound.GetSound();
                audioPlayer.TryPlay(sound, audioClip);
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }
        }

        public static async Task<AudioClip> PlayAsync_Static(Sound_Effect soundType, string additionalData = null)
        {
            AudioClip audioClip = null;

            try
            {
                var sound = soundConfig.GetSound(soundType, additionalData);
                audioClip = await sound.GetSound();
                audioPlayer.TryPlay(sound, audioClip);
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }

            return audioClip;
        }

        public static async Task<AudioClip> PlayAsync_Static(ISound sound)
        {
            AudioClip audioClip = null;

            try
            {
                audioClip = await sound.GetSound();
                audioPlayer.TryPlay(sound, audioClip);
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }

            return audioClip;
        }
    }
}