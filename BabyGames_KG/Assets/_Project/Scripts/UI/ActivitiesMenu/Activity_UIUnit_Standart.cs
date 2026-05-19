using CustomAttributes;
using DG.Tweening;
using GameState;
using Helpers;
using Loggers;
using Services;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Identifiers.UI
{
    public class Activity_UIUnit_Standart : Activity_UIUnit_Base
    {
        [Space]
        [SerializeField] [Re_Fg_Co] private CanvasGroup _lockedState;
        [SerializeField] [Re_Fg_Co] private CanvasGroup _toDownloadState;
        [SerializeField] [Re_Fg_Co] private CanvasGroup _downloadingState;
        [SerializeField] [Re_Fg_Co] private CanvasGroup _unlockedState;
        [SerializeField] [Re_Fg_Co] private Image _downloadingImage;

        [Space]
        [SerializeField] [Fg_Se] private float _statusUpdateRate = 2;
        [SerializeField] [Fg_Se] private Color _lockedColor;

        [field: SerializeField] [Fg_De] public bool isCached { get; set; } = false;
        [field: SerializeField] [Fg_De] public bool isDownloading { get; set; } = false;

        [ShowInInspector] private static Dictionary<string, float> _downloadStatuses = new();

        private static LazyUpdator_Service _lazyUpdate = new();

        private List<CanvasGroup> _states = new();

        private void Awake()
        {
            if (_lazyUpdate == null) { _lazyUpdate = new LazyUpdator_Service(); }
            _lastClickedToOpenAndReady.Clear();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _lockedState.FadeDownQuick();
            _toDownloadState.FadeDownQuick();
            _downloadingState.FadeDownQuick();
            _unlockedState.FadeDownQuick();

            _states.SafeAdd(_lockedState);
            _states.SafeAdd(_toDownloadState);
            _states.SafeAdd(_downloadingState);
            _states.SafeAdd(_unlockedState);

            _lazyUpdate.AddToQueue(UpdateState);

            SetIconTransform_Old();
        }

        protected override void Update()
        {
            base.Update();
            
            if (Keyboard.current.oKey.wasReleasedThisFrame)
            {
                SetIconTransform_Old();
            }

            if (Keyboard.current.nKey.wasReleasedThisFrame)
            {
                SetIconTransform_New();
            }
        }

        [Button]
        private void SetIconTransform_Old()
        {
            var iconScale = MainMenu_GameState_Model.selectedActivityCategory.vector3Settings.Find(x => x.key == activityData.activityName + "_iconScale");
            var iconOffset = MainMenu_GameState_Model.selectedActivityCategory.vector3Settings.Find(x => x.key == activityData.activityName + "_iconOffset");

            if (_iconImageHolder != null)
            {
                if (iconScale != null) { _iconImageHolder.DOScale(iconScale.value, 0.25f); }
                if (iconOffset != null) { _iconImageHolder.DOAnchorPos(iconOffset.value, 0.25f); }
            }

            if (_iconRam != null)
            {
                _iconRam.DOAnchorPosX(15, 0.25f);
            }
        }

        [Button]
        private void SetIconTransform_New()
        {
            var iconScale = MainMenu_GameState_Model.selectedActivityCategory.iconScales.Find(x => x.key == activityData.activityName);
            var iconOffset = MainMenu_GameState_Model.selectedActivityCategory.iconOffsets.Find(x => x.key == activityData.activityName);

            if (_iconImageHolder != null)
            {
                if (iconScale != null) { _iconImageHolder.DOScale(iconScale.value, 0.25f); }
                if (iconOffset != null) { _iconImageHolder.DOAnchorPos(iconOffset.value, 0.25f); }
            }

            if (_iconRam != null)
            {
                _iconRam.DOAnchorPos(Vector2.zero, 0.25f);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _lazyUpdate.RemoveFromQueue(UpdateState);
            _downloadingImage.DOKill();

            _toDownloadState.DOKill();
            _downloadingState.DOKill();
            _unlockedState.DOKill();

            foreach (var i in _states)
            {
                i.DOKill();
            }
        }

        public override async void OnOpenRequested()
        {
            if (activityData.IsUnlocked(_model.subscriptionChecker) == false)
            {
                _model.onPurchaseRequested?.Invoke();
                return;
            }

            if (isCached == false)
            {
                if (ApplicationHelper.HasInternetConnection() == false)
                {
                    MessagesMenu.instance?.ShowMessage("no_internet_connection");
                    return;
                }

                OnDownloadRequested();
                await AsyncHelper.WaitWhile(() => isDownloading == true, destroyCancellationToken);
            }

            _lastClickedToOpenAndReady.Add(this);

            if (isCached == true)
            {
                onActivityChosen?.Invoke(_lastClickedToOpenAndReady.Last());
            }
            else
            {
                while (_downloadStatuses.Count > 0) { await AsyncHelper.NextFrame(); }
                onActivityChosen?.Invoke(_lastClickedToOpenAndReady.Last());
            }
        }

        public override async void OnDownloadRequested()
        {
            if (isCached == false)
            {
                await TryDownload();
            }
        }

        private async Task UpdateState()
        {
            if (_currentHoldTime > 7)
            {
                await _model.contentDeliveryService.ClearCacheAsync(activityData.GetName(), activityData.version, activityData.GetUrl());
            }
            
            if (activityData.IsUnlocked(_model.subscriptionChecker) == false)
            {
                _iconImage.DOColor(_lockedColor, 0.25f);
                EnableState(_lockedState);
                await AsyncHelper.NextFrame();
                return;
            }
            else
            {
                _iconImage.DOColor(Color.white, 0.25f);
            }

            if (isDownloading == true)
            {
                EnableState(_downloadingState);

                _downloadingImage?.DOFillAmount(_downloadStatuses[activityData.GetName()], _statusUpdateRate);
                await AsyncHelper.NextFrame();
                return;
            }

            try
            {
                if (_model != null)
                {
                    isCached = _model.activity_Identifier_Provider.IsCashed(activityData);

                    if (isCached == true)
                    {
                        EnableState(_unlockedState);
                        _downloadStatuses.Remove(activityData.GetName());
                    }
                    else
                    {
                        if (_downloadStatuses.ContainsKey(activityData.GetName()))
                        {
                            EnableState(_downloadingState);
                            _downloadingImage?.DOFillAmount(_downloadStatuses[activityData.GetName()], _statusUpdateRate);
                        }
                        else
                        {
                            EnableState(_toDownloadState);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex, "Error during UpdateDownloadStatus()");
            } finally
            {
                await AsyncHelper.NextFrame();
            }
        }

        public async Task TryDownload()
        {
            if (_downloadStatuses.Count > 3)
            {
                CustomLogger.instance?.LogWarning("Too many downloads", $"Cancelling {activityData.GetName()} download", transform, LogTypes.General);
                return;
            }

            if (isDownloading == true) { return; }
            if (_downloadStatuses.ContainsKey(activityData.GetName())) { return; }
            if (_model.activity_Identifier_Provider.IsCashed(activityData) == true) { return; }

            _downloadStatuses.TryAdd(activityData.GetName(), 0);

            try
            {
                isDownloading = true;
                var activityBase_Identifier = await _model.activity_Identifier_Provider.GetActivity(activityData, (progress) =>
                {
                    _downloadStatuses[activityData.GetName()] = progress / 2;
                });

                if (activityBase_Identifier != null)
                {
                    await activityBase_Identifier.DownloadAdditionals(activityData, _model.contentDeliveryService, (progress) =>
                    {
                        _downloadStatuses[activityData.GetName()] = progress / 2;
                    });

                    _downloadStatuses[activityData.GetName()] = 1;
                    await AsyncHelper.DelayFloat(_statusUpdateRate);
                    onActivityDownloaded?.Invoke(activityData);
                }
                else
                {
                    await _model.contentDeliveryService.ClearCacheAsync(activityData.GetName(), activityData.version, activityData.GetUrl());
                }
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex, $"Could not download {activityData.GetName()}");
            } finally
            {
                isDownloading = false;
                _downloadStatuses.Remove(activityData.GetName());
            }
        }

        private void EnableState(CanvasGroup stateToEnable)
        {
            foreach (var state in _states)
            {
                if (state == stateToEnable)
                {
                    state.FadeUp();
                }
                else
                {
                    state.FadeDown();
                }
            }
        }
    }
}