using Agents;
using CustomAttributes;
using DG.Tweening;
using Helpers;
using Loggers;
using Sirenix.OdinInspector;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Modes.Coloring
{
    [RequireComponent(typeof(AnimationAgent))]
    [RequireComponent(typeof(CanvasGroup))]
    public class HintHand : MonoBehaviour
    {
        [SerializeField] private AnimationAgent _animationAgent;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _playAnimationButton;

        [Space]
        [SerializeField] private float _popupTime = 1f;
        [SerializeField] private float _goToBrushesTime = 1f;
        [SerializeField] private float _goToCenterTime = 1f;

        [SerializeField, Fg_De] private float _currentDrawWaitTime = 10f;

        [Space]
        [SerializeField, Fg_De] private bool _isPlayingAnimation;
        [SerializeField, Fg_De] private string _currentAnimation;

        private Gameplay_GameState_Coloring_Model _model;

        private PalleteInstrumentSelector[] _instrumentButtons;

        private bool _isInitialized = false;

        private CancellationToken _cancellationToken;

        [ShowInInspector] private bool _everDrawn => Paintable_Identifier_Basic.isEverPainted;

        private void Awake()
        {
            _animationAgent = GetComponent<AnimationAgent>();
            _canvasGroup = GetComponent<CanvasGroup>();

            _canvasGroup?.FadeDownQuick(setActiveToFalse: false);

            _cancellationToken = destroyCancellationToken;
        }

        private void OnDestroy()
        {
            _canvasGroup.DOKill();
        }

        private void OnDisable()
        {
            transform?.DOKill();
            _canvasGroup?.DOKill();
            StopAllCoroutines();
        }

        private void Update()
        {
            var pointer = Pointer.current;
            if (pointer == null)
                return;

            if (PlayerActions_DataHolder.instance?.timeSinceLastAction > _currentDrawWaitTime)
            {
                InitializeAnimation();
            }
            else
            {
                Clear();
            }
        }

        public void Initialize(Gameplay_GameState_Coloring_Model model)
        {
            _model = model;

            if (_isInitialized) { return; }

            _animationAgent.onAnimationCallback += OnAnimationCallback;

            if (model.isInitialized.value)
            {
                _isInitialized = true;
            }
            else
            {
                model.isInitialized.AddListener((value) =>
                {
                    _isInitialized = true;
                });
            }

            _currentDrawWaitTime = _model.gameSettings.hint_delayDraw;
        }

        [Button]
        private async void InitializeAnimation()
        {
            if (_model.isAnimationPlaying.value == true)
            {
                PlayerActions_DataHolder.ResetTime();
                Clear();
                return;
            }

            if (_model.isInitialized.value == false) { return; }
            if (_isPlayingAnimation) { return; }
            if (_cancellationToken.IsCancellationRequested) { return; }

            Clear();

            if (_everDrawn)
            {
                if (Random.Range(0, 5) > 2)
                {
                    _currentAnimation = "StartHint_PlayAnimation";
                    await StartHint_PlayAnimation();
                }
                else
                {
                    _currentAnimation = "Initialize_Draw";
                    await Initialize_Draw();
                }
            }
            else
            {
                await Initialize_Draw();
            }
        }

        [Button]
        private async Task Initialize_Draw()
        {
            _instrumentButtons = FindObjectsByType<PalleteInstrumentSelector>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            await StartHint_Draw();
        }

        public async Task StartHint_Draw()
        {
            try
            {
                _canvasGroup.FadeDownQuick(setActiveToFalse: false);

                _animationAgent?.PlayAnimation("Idle");
                _isPlayingAnimation = true;

                await transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack).AsyncWaitForCompletion();
                await _canvasGroup.DOFade(1, _popupTime).SetEase(Ease.OutSine).AsyncWaitForCompletion();
                Vector3 targetPosition = _instrumentButtons.GetRandom().GetComponent<RectTransform>().position;
                await transform.DOMove(targetPosition, _goToBrushesTime).SetEase(Ease.OutSine).AsyncWaitForCompletion();
                _animationAgent?.PlayAnimation("Click");

                await AsyncHelper.WaitWhile(() => _isPlayingAnimation == false, _cancellationToken);
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }
        }

        public async Task StartHint_PlayAnimation()
        {
            try
            {
                transform.DOKill();

                _isPlayingAnimation = true;
                _canvasGroup.FadeDownQuick(setActiveToFalse: false);

                _animationAgent?.PlayAnimation("Idle");
                transform.position = _playAnimationButton.transform.position;

                transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
                await _canvasGroup.DOFade(1, _popupTime).SetEase(Ease.OutSine).AsyncWaitForCompletion();
                await transform.DOMove(_playAnimationButton.transform.position, 0.5f).SetEase(Ease.OutBack).SetDelay(1f).AsyncWaitForCompletion();

                await transform.DOScale(0.5f, 0.25f).SetEase(Ease.OutBack).SetDelay(0.25f).AsyncWaitForCompletion();
                await AsyncHelper.DelayFloat(0.25f);

                await transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack).SetDelay(0.25f).AsyncWaitForCompletion();
                await transform.DOScale(0, 0.25f).SetEase(Ease.OutBack).SetDelay(0.25f).AsyncWaitForCompletion();
                _canvasGroup.DOFade(0, _popupTime);

                PlayerActions_DataHolder.ResetTime();
                _isPlayingAnimation = false;
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }
        }

        private void OnAnimationCallback(string name)
        {
            switch (name)
            {
                case "Clicked":
                    {
                        transform.DOLocalMove(Vector3.zero, _goToCenterTime).SetEase(Ease.OutSine).OnComplete(() =>
                        {
                            _animationAgent?.PlayAnimation("Draw");
                        });
                        break;
                    }
                case "Drawn":
                    {
                        _canvasGroup.DOFade(0, _popupTime).SetEase(Ease.OutSine).OnComplete(() =>
                        {
                            PlayerActions_DataHolder.ResetTime();
                            _isPlayingAnimation = false;
                        });
                        break;
                    }
            }
        }

        private void Clear()
        {
            if (_canvasGroup.alpha > 0)
            {
                transform.DOKill();
                _canvasGroup.DOKill();

                transform.localPosition = Vector3.zero;
                _canvasGroup.FadeDownQuick(setActiveToFalse: false);

                StopAllCoroutines();
            }

            _isPlayingAnimation = false;
        }
    }
}