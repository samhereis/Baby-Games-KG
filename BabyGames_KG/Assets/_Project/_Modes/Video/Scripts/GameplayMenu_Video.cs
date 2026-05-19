using DataClasses;
using DG.Tweening;
using Helpers;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UI.Menus;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;
using Video;
using Zenject;

namespace Assets._Project._Modes.Video.Scripts
{
    public class GameplayMenu_Video : MenuBase
    {
        [Space]
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _lockButton;

        [SerializeField] private Image _lockedImage;
        [SerializeField] private Image _unlockedImage;

        [Space]
        [SerializeField] private Slider _videoSlider;

        [Space]
        [SerializeField] private Toggle _videoPlayToggle;

        [SerializeField] private Image _toggle_PauseImage;
        [SerializeField] private Image _toggle_PlayImage;

        [Space]
        [SerializeField] private Button _previousButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _openControllsButton;
        [SerializeField] private Button _closeControllsButton;

        [Space]
        [SerializeField] private TextMeshProUGUI _currentTimeText;
        [SerializeField] private TextMeshProUGUI _videoDurationText;

        [Space]
        [SerializeField] private CanvasGroup _controllsCanvasGroup;
        [SerializeField] private CanvasGroup _moreVideosCanvasGroup;

        [Space]
        [SerializeField] private CanvasGroup[] _lockables;

        [Inject] private VideoPlayer _videoPlayer;

        public Gameplay_GameState_Video_Model model { get; private set; }

        private bool _isDraggingSlider = false;
        private bool _sliderBeingSetProgrammatically = false;

        private List<CanvasGroup> _allVideoStates = new List<CanvasGroup>();

        public void Construct(Gameplay_GameState_Video_Model model)
        {
            this.model = model;

            DiService.Inject(this);
        }

        public override async void Enable(float? duration = null)
        {
            _videoPlayer.clip = model.videoClip;

            _backButton.onClick.AddListener(() =>
            {
                if (model.areControlsLocked.value == true)
                {
                    return;
                }

                model.onGoToMenuRequested?.Invoke();
            });

            _openControllsButton.onClick.AddListener(() => { SetVideoWindow(_controllsCanvasGroup); });
            _closeControllsButton.onClick.AddListener(() => { SetVideoWindow(null); });

            OnControlsLockStatucChanged(model.areControlsLocked.value);
            _lockButton.onClick.AddListener(() =>
            {
                model.areControlsLocked.value = !model.areControlsLocked.value;
                OnControlsLockStatucChanged(model.areControlsLocked.value);
            });

            _previousButton.onClick.AddListener(() => { SetVideo(false); });
            _nextButton.onClick.AddListener(() => { SetVideo(true); });

            SetupVideoTrack();

            _videoPlayer.Play();

            _controllsCanvasGroup.FadeDownQuick();
            _moreVideosCanvasGroup.FadeDownQuick();

            _allVideoStates.Clear();
            _allVideoStates.Add(_controllsCanvasGroup);
            _allVideoStates.Add(_moreVideosCanvasGroup);

            SetVideoWindow(null);

            base.Enable(duration);
        }

        public override void Disable(float? duration = null)
        {
            base.Disable(duration);
            _videoSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
            _videoPlayToggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }

        private void Update()
        {
            if (_videoPlayer.clip == null)
            {
                return;
            }

            double totalDuration = _videoPlayer.clip.length;
            double currentTime = _videoPlayer.time;

            _currentTimeText.text = FormatTime(currentTime);
            _videoDurationText.text = FormatTime(totalDuration);

            if (!_isDraggingSlider && totalDuration > 0)
            {
                _sliderBeingSetProgrammatically = true;
                _videoSlider.value = (float)(currentTime / totalDuration);
                _sliderBeingSetProgrammatically = false;
            }

            if (currentTime >= totalDuration - 1)
            {
                SetVideo(true);
                this.enabled = false;
            }

            FixAspectRation();
        }

        private void FixAspectRation()
        {
            try
            {
                var widthDifference = Screen.height - 1080;
                var width = 1920 + widthDifference;
                var height = 1080 + widthDifference;

                _videoPlayer.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);

                var sizeDiference = (float)Screen.width / 1920f;
                if (sizeDiference <= 0) { sizeDiference = 1; }

                var videoHeight = NumberHelper.GetNumberFromPercentage(Screen.height, 75) * 0.75f;
                float aspectRatio = 1920f / 1080f;
                float videoWidth = videoHeight * aspectRatio;
            }
            catch (Exception ex)
            {

            }
        }

        private void SetupVideoTrack()
        {
            _videoSlider.onValueChanged.AddListener(OnSliderValueChanged);

            EventTrigger trigger = _videoSlider.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = _videoSlider.gameObject.AddComponent<EventTrigger>();
            }

            EventTrigger.Entry pointerDown = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown
            };

            pointerDown.callback.AddListener((data) => { _isDraggingSlider = true; });
            trigger.triggers.Add(pointerDown);

            EventTrigger.Entry pointerUp = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerUp
            };

            pointerUp.callback.AddListener((data) => { _isDraggingSlider = false; });
            trigger.triggers.Add(pointerUp);

            _videoPlayToggle.isOn = true;
            OnToggleValueChanged(true);
            _videoPlayToggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        private void OnControlsLockStatucChanged(bool isLocked)
        {
            if (model.areControlsLocked.value)
            {
                _lockedImage.transform.DOScale(0, 0.25f).SetEase(Ease.InOutBack);
                _unlockedImage.transform.DOScale(1, 0.25f).SetEase(Ease.InOutBack);
            }
            else
            {
                _lockedImage.transform.DOScale(1, 0.25f).SetEase(Ease.InOutBack);
                _unlockedImage.transform.DOScale(0, 0.25f).SetEase(Ease.InOutBack);
            }

            foreach (var item in _lockables)
            {
                item.interactable = !model.areControlsLocked.value;
                item.blocksRaycasts = !model.areControlsLocked.value;
            }
        }

        private void OnSliderValueChanged(float value)
        {
            if (model.areControlsLocked.value == true)
            {
                return;
            }

            if (_sliderBeingSetProgrammatically || _videoPlayer.clip == null)
                return;

            double totalDuration = _videoPlayer.clip.length;
            _videoPlayer.time = value * totalDuration;
        }

        private void OnToggleValueChanged(bool isPlaying)
        {
            if (model.areControlsLocked.value == true)
            {
                return;
            }

            if (isPlaying)
            {
                _videoPlayer.Play();
                _toggle_PlayImage.transform.DOScale(0, 0.25f).SetEase(Ease.InOutBack);
                _toggle_PauseImage.transform.DOScale(1, 0.25f).SetEase(Ease.InOutBack);
            }
            else
            {
                _videoPlayer.Pause();
                _toggle_PlayImage.transform.DOScale(1, 0.25f).SetEase(Ease.InOutBack);
                _toggle_PauseImage.transform.DOScale(0, 0.25f).SetEase(Ease.InOutBack);
            }
        }

        private void SetVideo(bool doNext)
        {
            Activity activity = null;
            int index = Gameplay_GameState_Video_Model.videosToWatch.IndexOf(model.activity);

            if (doNext)
            {
                bool isLast = Gameplay_GameState_Video_Model.videosToWatch.Last() == model.activity;

                if (isLast)
                {
                    activity = Gameplay_GameState_Video_Model.videosToWatch.First();
                }
                else
                {
                    index++;
                    activity = Gameplay_GameState_Video_Model.videosToWatch[index++];
                }
            }
            else
            {
                bool isFirst = Gameplay_GameState_Video_Model.videosToWatch.First() == model.activity;

                if (isFirst)
                {
                    activity = Gameplay_GameState_Video_Model.videosToWatch.Last();
                }
                else
                {
                    activity = Gameplay_GameState_Video_Model.videosToWatch[index - 1];
                }
            }

            model?.onChangeVideoRequested(activity);
        }

        public void SetVideoWindow(CanvasGroup canvasGroup)
        {
            foreach (var item in _allVideoStates)
            {
                if (canvasGroup == item)
                {
                    item.FadeUp(setActiveToTrue: true);
                }
                else
                {
                    item.FadeDown(setActiveToFalse: true);
                }
            }
        }

        private string FormatTime(double time)
        {
            int minutes = (int)time / 60;
            int seconds = (int)time % 60;
            return string.Format("{0:D2}:{1:D2}", minutes, seconds);
        }
    }
}