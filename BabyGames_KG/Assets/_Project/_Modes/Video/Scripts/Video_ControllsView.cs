using Assets._Project._Modes.Video.Scripts;
using DG.Tweening;
using GameState;
using Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Video
{
    public class Video_ControllsView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private SwipeDetector _swipeDetector;

        [SerializeField] private GameplayMenu_Video _gameplayMenu;

        [SerializeField] private CanvasGroup _moreVideosCanvasGroup;

        [SerializeField] private List<Image> _moreVideos_Previews = new();

        [Space]
        [SerializeField] private Button _favoriteButton;
        [SerializeField] private Image _favoriteImage;

        private async void Start()
        {
            foreach (var item in _moreVideos_Previews)
            {
                item.gameObject.SetActive(false);
            }

            var list = Gameplay_GameState_Video_Model.videosToWatch.Where((item) => item.activityName != _gameplayMenu.model.activity.activityName).ToList();

            for (int index = 0; index < list.Count; index++)
            {
                if (index > 3) { break; }
                if (index + 1 > list.Count) { break; }
                if (index + 1 > _moreVideos_Previews.Count) { break; }

                var activity = list[index];
                if (activity == null) { break; }

                _moreVideos_Previews[index].gameObject.SetActive(true);
                _moreVideos_Previews[index].sprite = await activity.GetIcon(_gameplayMenu.model.contentDeliveryService);
            }

            _favoriteImage.DOFade(Gameplay_GameState_Video_Model.IsFavoritedVideo(_gameplayMenu.model.activity).ToInt(), 1);
        }

        private void OnEnable()
        {
            _swipeDetector.onSwipeUp += OpenMoreVideos;
            _button?.onClick.AddListener(OpenMoreVideos);
            _favoriteButton?.onClick.AddListener(ToggleFavoriteVideo);
        }

        private void OnDisable()
        {
            _swipeDetector.onSwipeDown -= OpenMoreVideos;
            _button?.onClick.RemoveListener(OpenMoreVideos);
            _favoriteButton?.onClick.RemoveListener(ToggleFavoriteVideo);
        }

        private void OpenMoreVideos()
        {
            _gameplayMenu.SetVideoWindow(_moreVideosCanvasGroup);
        }

        private void ToggleFavoriteVideo()
        {
            Gameplay_GameState_Video_Model.AddOrDeleteFavorite(_gameplayMenu.model.activity, MainMenu_GameState_Model.selectedActivityCategory);
            _favoriteImage.DOFade(Gameplay_GameState_Video_Model.IsFavoritedVideo(_gameplayMenu.model.activity).ToInt(), 1);
        }
    }
}