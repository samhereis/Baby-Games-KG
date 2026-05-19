using Helpers;
using Services;
using SO.Lists;
using Sounds;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using Zenject;

namespace Modes.Coloring
{
    public class DrawSounds : MonoBehaviour
    {
        [SerializeField] private AssetReferenceT<Sound_SO> _onStartPaintingReference;
        [SerializeField] private AssetReferenceT<Sound_SO> _onPaintingReference;

        [Space]
        [SerializeField] private AudioSource _startAudioSource;
        [SerializeField] private AudioSource _mainAudioSource;

        [Inject] private GameController _gameController;

        private bool _isPlayingAnimation;

        private Sound _onStartPainting;
        private Sound _onPainting;

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
            if (_mainAudioSource == null) { return; }
            if (_startAudioSource == null) { return; }

            bool isOverUI = UIHelper.IsPointOverUI();

            var pointer = Pointer.current;
            if (pointer == null) return;

            if (pointer.press.wasPressedThisFrame && isOverUI == false)
            {
                Play();
            }

            bool isPainting = pointer.press.isPressed && _isPlayingAnimation == false;
            _startAudioSource.enabled = isPainting;
            _mainAudioSource.enabled = isPainting;

            if (pointer.press.wasReleasedThisFrame)
            {
                Stop();
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

            _startAudioSource.enabled = true;
            _startAudioSource.clip = await _onStartPainting.GetSound();
            _startAudioSource.Play();

            _mainAudioSource.enabled = true;
            await AsyncHelper.DelayFloat(_onPainting.data.delay);
            _mainAudioSource.clip = await _onPainting.GetSound();
            _mainAudioSource.Play();
        }

        private void Stop()
        {
            if (_mainAudioSource == null) { return; }
            if (_startAudioSource == null) { return; }

            _startAudioSource.enabled = false;
            _startAudioSource.Stop();
            _startAudioSource.clip = null;

            _mainAudioSource.enabled = false;
            _mainAudioSource.Stop();
            _mainAudioSource.clip = null;
        }
    }
}