using _Project.Scripts.Services;
using Helpers;
using Services;
using UnityEngine;
using Zenject;

namespace _Project._Modes.Orchestra.Scripts
{
    [RequireComponent(typeof(AudioSource))]
    public class WaveSettings_Orchestra : MonoBehaviour
    {
        public Transform waveHolder;
        public AudioClip finishAudio;

        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private float _delay = 1;

        [Inject] private BackgroundMusicService _backgroundMusicService;

        private void Reset()
        {
            waveHolder = transform;
            _audioSource = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            DiService.Inject(this);
        }

        public void PlaySound(AudioClip audioClip)
        {
            if (audioClip == null) { return; }

            _audioSource.clip = audioClip;
            _audioSource.Play();
        }

        public async void PlayFinish()
        {
            _backgroundMusicService.ChangeVolume_External(0.25f);

            await AsyncHelper.DelayFloat(_delay);
            PlaySound(finishAudio);
            await AsyncHelper.WaitWhile(() => _audioSource.isPlaying, destroyCancellationToken);

            _backgroundMusicService.ChangeVolume_External(1);
        }
    }
}
