using DataClasses;
using Helpers;
using Interfaces;
using Loggers;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace Sounds
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class SoundPlayer : MonoBehaviour, ISoundPlayer, ISelfValidator
    {
        public event Action<ISound> onCompletedPlaying;

        [Required]
        [FoldoutGroup("Componenets"), SerializeField] private AudioSource _mainAudioSource;

        [Required]
        [FoldoutGroup("Componenets"), SerializeField] private List<AudioSource> _audioSourcePool = new List<AudioSource>();

        [Header("Settings")]
        [FoldoutGroup("Settings"), SerializeField] private int _auioSourcePoolCount = 2;
        [FoldoutGroup("Settings"), SerializeField] private bool _isGlobal;

        public void Validate(SelfValidationResult result)
        {
            Validate();
        }

        public void Validate()
        {
            if (_mainAudioSource == null)
            {
                _mainAudioSource = GetComponent<AudioSource>();
            }

            _audioSourcePool.RemoveNulls();
            if (_audioSourcePool.Count == 0)
            {
                FillAudioSoursePool();
            }
        }

        private void Awake()
        {
            Validate();
        }

        public void TryPlay(ISound sound, AudioClip audioClip = null)
        {
            try
            {
                Validate();
                if (sound == null) return;
                if (sound.HasAnySound() == false) return;

                if (sound.data.isMain)
                {
                    TryPlayMain(sound, audioClip);
                }
                else
                {
                    TryPlayPool(sound, audioClip);
                }
            } catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }
        }

        public async void TryPlayMain(ISound sound, AudioClip audioClip = null)
        {
            if (sound.HasSoundInList(_mainAudioSource.clip) && _mainAudioSource.isPlaying) { return; }

            if (audioClip == null) { audioClip = await sound.GetSound(); }

            StopMain();
            _mainAudioSource.clip = audioClip;
            _mainAudioSource.volume = sound.data.volume;

            _mainAudioSource.maxDistance = sound.data.maxDistance;
            _mainAudioSource.minDistance = 0;

            _mainAudioSource.loop = sound.data.loop;
            _mainAudioSource.Play();

            if (sound.data.disableOthers) StopPool();
        }

        public async void TryPlayPool(ISound sound, AudioClip audioClip = null)
        {
            var freeAudioSurce = _audioSourcePool.Find(x => x.isPlaying == false);
            if (freeAudioSurce == null) freeAudioSurce = _audioSourcePool[0];

            if (audioClip == null) { audioClip = await sound.GetSound(); }

            freeAudioSurce.Stop();
            freeAudioSurce.clip = audioClip;
            freeAudioSurce.volume = sound.data.volume;

            _mainAudioSource.maxDistance = sound.data.maxDistance;
            _mainAudioSource.minDistance = 0;

            freeAudioSurce.loop = sound.data.loop;
            freeAudioSurce.Play();
        }

        public void SetPauseMain(bool pause)
        {
            if (pause == true)
            {
                _mainAudioSource.Pause();
            }
            else
            {
                _mainAudioSource.UnPause();
            }
        }

        public void SetPausePool(bool pause)
        {
            if (pause == true)
            {
                foreach (AudioSource audioSource in _audioSourcePool) audioSource.Pause();
            }
            else
            {
                foreach (AudioSource audioSource in _audioSourcePool) audioSource.UnPause();
            }
        }

        public async void Stop(ISound sound, AudioClip audioClip = null, float fadeDuration = 0f)
        {
            try
            {
                if (sound == null) return;

                foreach (AudioSource audioSource in _audioSourcePool)
                {
                    if (sound.HasSoundInList(audioSource.clip))
                    {
                        if (fadeDuration > 0) { await audioSource.DOFade(0, fadeDuration).AsyncWaitForCompletion(); }
                        audioSource.Stop();
                    }
                }
            } catch (Exception e)
            {
                CustomLogger.instance?.LogException(e);
            }
        }

        public void Pause(ISound sound, AudioClip audioClip = null)
        {
            if (sound.HasSoundInList(_mainAudioSource.clip)) { _mainAudioSource.Pause(); }

            foreach (AudioSource audioSource in _audioSourcePool)
            {
                if (sound.HasSoundInList(audioSource.clip))
                {
                    audioSource.Pause();
                }
            }
        }

        public void Resume(ISound sound, AudioClip audioClip = null)
        {
            if (sound.HasSoundInList(_mainAudioSource.clip)) { _mainAudioSource.UnPause(); }

            foreach (AudioSource audioSource in _audioSourcePool)
            {
                if (sound.HasSoundInList(audioSource.clip))
                {
                    audioSource.UnPause();
                }
            }
        }

        public void StopMain()
        {
            _mainAudioSource.Stop();
        }

        public void StopPool()
        {
            foreach (AudioSource audioSource in _audioSourcePool) audioSource.Stop();
        }

        public async void StopAllInstancesOf(Sound sound)
        {
            if (_mainAudioSource.clip == await sound.GetSound()) { StopMain(); }
            ;

            foreach (AudioSource audioSource in _audioSourcePool)
            {
                if (audioSource.clip == await sound.GetSound())
                {
                    audioSource.Stop();
                }
            }
        }

        [Button]
        private void FillAudioSoursePool()
        {
            if (_auioSourcePoolCount == 0) { _auioSourcePoolCount = 2; }

            _audioSourcePool = GetComponentsInChildren<AudioSource>().ToList();
            _audioSourcePool.RemoveAll(x => x == null || x.gameObject == gameObject);

            if (_audioSourcePool.Count != _auioSourcePoolCount)
            {
                foreach (AudioSource audioSource in _audioSourcePool)
                {
                    if (Application.isPlaying == false)
                    {
                        DestroyImmediate(audioSource.gameObject);
                    }
                    else
                    {
                        Destroy(audioSource.gameObject);
                    }
                }

                _audioSourcePool.Clear();

                for (int i = 0; i < _auioSourcePoolCount; i++)
                {
                    var obj = new GameObject("A sound");
                    obj.transform.parent = transform;
                    obj.AddComponent<AudioSource>();

                    _audioSourcePool.Add(obj.GetComponent<AudioSource>());
                }
            }
        }
    }
}