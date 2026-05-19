using DataClasses.AssetReferences;
using DG.Tweening;
using Helpers;
using ScriptableObjects;
using Services;
using SO;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Services
{
    public class BackgroundMusicService : MonoBehaviour
    {
        public AudioSource _audioSource;

        [SerializeField] private ExternalAssetReference<BackgroundMusic_Data> _backgroundMusicData;

        private string _current;

        [Inject] private GameSavableSettings _gameSavableSettings;

        private void OnEnable()
        {
            DiService.Inject(this);
            _gameSavableSettings.backgroundMusic.onValueChanged += OnBackgroundMusicStatusChanged;
        }

        private void OnDisable()
        {
            _gameSavableSettings.backgroundMusic.onValueChanged -= OnBackgroundMusicStatusChanged;
        }

        public async void PlayMusicFor(string musicName)
        {
            _current = musicName;
            if (_gameSavableSettings.backgroundMusic.currentValue == false)
            {
                return;
            }

            var underwaterWorldData = await _backgroundMusicData.GetAssetAsync();
            var sound = underwaterWorldData?.backgroundMusic.Find(x => x.key == musicName);
            var clip = await sound.value.GetSound();
            if (_audioSource.clip == clip && _audioSource.volume > 0)
            {
                return;
            }

            _audioSource.DOFade(0, 1);

            if (sound != null)
            {
                await AsyncHelper.DelayFloat(1f);

                _audioSource.clip = clip;
                _audioSource.Play();
                _audioSource.DOFade(1, 1);

                _audioSource.loop = true;
            }
        }

        private async void OnBackgroundMusicStatusChanged(bool obj)
        {
            _audioSource.DOKill();

            if (obj == false)
            {
                await _audioSource.DOFade(0, 1).AsyncWaitForCompletion();
                _audioSource.Stop();
            }
            else
            {
                PlayMusicFor(_current);
            }
        }

        public void ChangeVolume_External(float volume)
        {
            if (_gameSavableSettings.backgroundMusic.currentValue == false)
            {
                return;
            }

            _audioSource.DOFade(volume, 1f);
        }
    }
}