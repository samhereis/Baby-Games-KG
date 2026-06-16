using Coloring.Notification;
using Data;
using DataClasses;
using DG.Tweening;
using GameState;
using Helpers;
using ScriptableObjects;
using Services;
using SO;
using System.Collections.Generic;
using _Project.Scripts.Services;
using _Project.Scripts.Services.Purchase;
using UI;
using UI.Menus;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Coloring.ForParents
{
    public class SettingsMenu : MenuBase
    {
        [field: SerializeField] public Button backButton;
        [field: SerializeField] public Button purchaseButton;

        [SerializeField] private List<KeyedObject<ForParents_TabButton, SettingsTab_Base>> _listSelection = new();

        [Space]
        [SerializeField] private Button _buttonLinkToInstagram;
        [SerializeField] private Button _buttonLinkToYoutube;
        [SerializeField] private Button _buttonLinkToSpotify;
        [SerializeField] private Button _buttonRateUs;

        [Space]
        [SerializeField] private Toggle _backgroundMusicToggle;
        [SerializeField] private Toggle _notificationsToggle;

        [Inject] private GameConfigs_Coloring_SO _gameSettings;
        [Inject] private GameSavableSettings _gameSavableSettings;
        [Inject] private ISubscriptionChecker _lifetimePurchase;
        [Inject] private NotificationBase _notificationBase;
        [Inject] private RateUs _rateUs;

        private MainMenu_GameState_Model _model;

        public void Construct(MainMenu_GameState_Model model)
        {
            _model = model;
        }

        protected override void Awake()
        {
            base.Awake();
            DiService.Inject(this);
        }

        private void Start()
        {
            if (_lifetimePurchase?.IsSubscribed() == true)
            {
                purchaseButton?.gameObject.SetActive(false);
            }
            else
            {
                purchaseButton.onClick.AddListener(() =>
                {
                    _model.onPurchaseRequested?.Invoke();
                });
            }

            foreach (var item in _listSelection)
            {
                item.value.Construct(_model);

                item.key.button.onClick.AddListener(() =>
                {
                    EnableSelection(item);
                });
            }
            EnableSelection(_listSelection[0]);

            _buttonLinkToInstagram.onClick.AddListener(() => OpenURL(_gameSettings.linkSettings.website));
            _buttonLinkToYoutube.onClick.AddListener(() => OpenURL(_gameSettings.linkSettings.youtubeLink));
            _buttonLinkToSpotify.onClick.AddListener(() => OpenURL(_gameSettings.linkSettings.spotifyLink));
            _buttonRateUs.onClick.AddListener(OnClickButtonRateUs);

            _notificationsToggle.isOn = _gameSavableSettings.notifications.currentValue;
            _backgroundMusicToggle.isOn = _gameSavableSettings.backgroundMusic.currentValue;

            _notificationsToggle.onValueChanged.AddListener(OnNotificationsToggleChanged);
            _backgroundMusicToggle.onValueChanged.AddListener(OnBackgroundMusicChanged);

        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _buttonLinkToInstagram.onClick.RemoveAllListeners();
            _buttonLinkToYoutube.onClick.RemoveAllListeners();
            _buttonLinkToSpotify.onClick.RemoveAllListeners();
            _buttonRateUs.onClick.RemoveAllListeners();
        }

        private void EnableSelection(KeyedObject<ForParents_TabButton, SettingsTab_Base> pair)
        {
            foreach (var item in _listSelection)
            {
                if (item == pair)
                {
                    item.key.EnableSelection();
                    item.value.Get<CanvasGroup>().DOKill();
                    item.value.Get<CanvasGroup>().FadeUp(setActiveToTrue: true);
                }
                else
                {
                    item.key.DisableSelection();
                    item.value.Get<CanvasGroup>().DOKill();
                    item.value.Get<CanvasGroup>().FadeDown(setActiveToFalse: true);
                }
            }
        }

        private void OpenURL(string url)
        {
            Application.OpenURL(url);
        }

        private void OnClickButtonRateUs()
        {
            _rateUs.OpenStorePage();
        }

        private void OnNotificationsToggleChanged(bool isOn)
        {
            _gameSavableSettings.notifications.SetData(isOn);
            _notificationBase.UpdateNotifications();
        }

        private void OnBackgroundMusicChanged(bool isOn)
        {
            _gameSavableSettings.backgroundMusic.SetData(isOn);
        }
    }
}