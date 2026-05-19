using System;
using System.Threading.Tasks;
using DataClasses;
using DG.Tweening;
using GameState;
using Helpers;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
using Video;

namespace Identifiers.UI
{
    public class Activity_UIUnit_Video : Activity_UIUnit_Standart
    {
        [Space]
        [SerializeField] private Button _addToFavoritesButton;

        private void Awake()
        {
            onActivityDownloaded += OnDownloaded;
        }

        private void OnDestroy()
        {
            onActivityDownloaded -= OnDownloaded;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            UpdateFavoriteView();

            _addToFavoritesButton.onClick.AddListener(AddOrDeleteFavorite);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            _addToFavoritesButton.onClick.RemoveListener(AddOrDeleteFavorite);
        }

        protected override async void TrySetLocalization()
        {
            try
            {
                Destroy(_itemNameText.GetComponent<LocalizeStringEvent>());

                _itemNameText.text = activityData.displayName;
                if (string.IsNullOrEmpty(_itemNameText.text))
                {
                    _itemNameText.text = activityData.activityName;
                }

                await AsyncHelper.NextFrame();
                _itemNameText.transform.DOScale(1, 0.25f).SetEase(Ease.OutBack);
            }
            catch (Exception e)
            {

            }
        }

        private void OnDownloaded(Activity activity)
        {
            Gameplay_GameState_Video_Model.UpdateDownloadedVideos(_model.contentDeliveryService);
        }

        private async void AddOrDeleteFavorite()
        {
            Gameplay_GameState_Video_Model.AddOrDeleteFavorite(activityData, MainMenu_GameState_Model.selectedActivityCategory);

            await AsyncHelper.NextFrame();
            UpdateFavoriteView();
        }

        private void UpdateFavoriteView()
        {
            _addToFavoritesButton.targetGraphic.DOColor(Gameplay_GameState_Video_Model.IsFavoritedVideo(activityData) ? Color.yellow : Color.white, 0.25f);
        }
    }
}