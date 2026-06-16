using System.Threading.Tasks;
using CustomAttributes;
using Helpers;
using Modes.Coloring;
using Services;
using Sounds;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace _Project._Modes.Coloring.Scripts.Gameplay
{
    [DisallowMultipleComponent]
    public class DrawSounds : MonoBehaviour
    {
        [Re_Fg_Ref, SerializeField] private SoundAdvanced _onStartPaintingReference;
        [Re_Fg_Ref, SerializeField] private SoundAdvanced _onPaintingReference;

        [Space]
        [Re_Fg_Co, SerializeField] private AudioSource _startAudioSource;
        [Re_Fg_Co, SerializeField] private AudioSource _mainAudioSource;

        [Fg_De, SerializeField] private bool _isPlayingAnimation;

        [Inject] private GameController _gameController;

        private void Awake()
        {
            DiService.Inject(this);
        }

        private void OnEnable()
        {
            _gameController.model.onAnimationStarting += OnAnimationStarted;
            _gameController.model.onAnimationEnded += OnAnimationEnded;
        }

        private void OnDisable()
        {
            _gameController.model.onAnimationStarting -= OnAnimationStarted;
            _gameController.model.onAnimationEnded -= OnAnimationEnded;

            Stop();
        }

        private void Update()
        {
            if (_mainAudioSource == null || _startAudioSource == null) { return; }

            var pointer = Pointer.current;
            if (pointer == null) { return; }

            if (pointer.press.wasReleasedThisFrame)
            {
                Stop();
            }
            else if (pointer.press.wasPressedThisFrame)
            {
                if (UIHelper.IsPointOverUI() == false)
                {
                    Play();
                }
                else
                {
                    Stop();
                }
            }
        }

        private Task OnAnimationStarted()
        {
            _isPlayingAnimation = true;
            Stop();
            return Task.CompletedTask;
        }

        private void OnAnimationEnded()
        {
            _isPlayingAnimation = false;
        }

        private async void Play()
        {
            if (_isPlayingAnimation) { return; }

            if (_onStartPaintingReference.HasAnySound())
            {
                _startAudioSource.enabled = true;
                _startAudioSource.clip = await _onStartPaintingReference.GetSound();
                _startAudioSource.Play();
            }

            if (_onPaintingReference.HasAnySound())
            {
                await AsyncHelper.DelayFloat(_onPaintingReference.data.delay);
                _mainAudioSource.enabled = true;
                _mainAudioSource.clip = await _onPaintingReference.GetSound();
                _mainAudioSource.Play();
            }
        }

        private void Stop()
        {
            if (_mainAudioSource == null) { return; }
            if (_startAudioSource == null) { return; }

            if (_startAudioSource.enabled)
            {
                _startAudioSource.enabled = false;
                _startAudioSource.Stop();
                _startAudioSource.clip = null;
            }

            if (_mainAudioSource.enabled)
            {
                _mainAudioSource.enabled = false;
                _mainAudioSource.Stop();
                _mainAudioSource.clip = null;
            }
        }
    }
}