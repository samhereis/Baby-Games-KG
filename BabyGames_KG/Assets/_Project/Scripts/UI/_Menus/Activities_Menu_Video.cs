using DataClasses;
using Services;
using Sounds;
using UnityEngine;
using UnityEngine.InputSystem;
using Video;
using Zenject;

namespace UI.Menus
{
    public class Activities_Menu_Video : Activities_Menu
    {
        [SerializeField] private Video_TabButton _allVideosButton;
        [SerializeField] private Video_TabButton _downloadedVideosButton;
        [SerializeField] private Video_TabButton _favoriteVideosButton;

        [SerializeField] private CurrentShowState _currentShowState;
        [SerializeField] private Sound _dragSound;

        [Inject] private ISoundPlayer _soundPlayer;

        private float _sqrMagnitude = 0f;
        private bool _canPlay = true;

        private enum CurrentShowState
        {
            AllVideos,
            DownloadedVideos,
            FavoriteVideos
        }

        protected override void Awake()
        {
            base.Awake();

            _allVideosButton.button.onClick.AddListener(ShowAllVideos);
            _downloadedVideosButton.button.onClick.AddListener(ShowDownloadedVideos);
            _favoriteVideosButton.button.onClick.AddListener(ShowFavoriteVideos);

            Gameplay_GameState_Video_Model.onAnyVideoListChanged -= UpdateShowState;
            Gameplay_GameState_Video_Model.onAnyVideoListChanged += UpdateShowState;

            DiService.Inject(this);
        }

        private void Update()
        {
            if (Pointer.current.press.isPressed)
            {
                _sqrMagnitude = Pointer.current.delta.ReadValue().sqrMagnitude;
                if (_sqrMagnitude > 200)
                {
                    if (_canPlay == true)
                    {
                        _soundPlayer?.TryPlay(_dragSound);
                        _canPlay = false;
                    }
                }
                else
                {
                    _canPlay = true;
                }
            }

            if (Pointer.current.press.wasReleasedThisFrame)
            {
                _canPlay = true;
            }
        }

        public override void Enable(float? duration = null)
        {
            base.Enable(duration);
            ShowAllVideos();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _allVideosButton.button.onClick.RemoveListener(ShowAllVideos);
            _downloadedVideosButton.button.onClick.RemoveListener(ShowDownloadedVideos);
            _favoriteVideosButton.button.onClick.RemoveListener(ShowFavoriteVideos);

            Gameplay_GameState_Video_Model.onAnyVideoListChanged -= UpdateShowState;
        }

        private void UpdateShowState()
        {
            switch (_currentShowState)
            {
                case CurrentShowState.AllVideos:
                    {
                        ShowAllVideos();
                        break;
                    }
                case CurrentShowState.DownloadedVideos:
                    {
                        ShowDownloadedVideos();
                        break;
                    }
                case CurrentShowState.FavoriteVideos:
                    {
                        ShowFavoriteVideos();
                        break;
                    }
            }
        }

        private void ShowAllVideos()
        {
            _currentShowState = CurrentShowState.AllVideos;

            foreach (var item in _activity_Instances)
            {
                item?.gameObject?.SetActive(true);
            }

            _allVideosButton.SetActiveStatus(true);
            _downloadedVideosButton.SetActiveStatus(false);
            _favoriteVideosButton.SetActiveStatus(false);

            Gameplay_GameState_Video_Model.videosToWatch.Clear();
            Gameplay_GameState_Video_Model.videosToWatch.AddRange(_activityCategory.activities);
        }

        private void ShowDownloadedVideos()
        {
            _currentShowState = CurrentShowState.DownloadedVideos;

            foreach (var item in _activity_Instances)
            {
                bool shouldSee = _model.contentDeliveryService.IsAssetCashed(item.activityData.GetUrl(), item.activityData.version);
                item?.gameObject?.SetActive(shouldSee);
            }

            _allVideosButton.SetActiveStatus(false);
            _downloadedVideosButton.SetActiveStatus(true);
            _favoriteVideosButton.SetActiveStatus(false);

            Gameplay_GameState_Video_Model.videosToWatch.Clear();
            Gameplay_GameState_Video_Model.videosToWatch.AddRange(Gameplay_GameState_Video_Model.downloadedVideos);
        }

        private void ShowFavoriteVideos()
        {
            _currentShowState = CurrentShowState.FavoriteVideos;

            foreach (var item in _activity_Instances)
            {
                bool shouldSee = Gameplay_GameState_Video_Model.IsFavoritedVideo(item.activityData);
                item.gameObject.SetActive(shouldSee);
            }

            _allVideosButton.SetActiveStatus(false);
            _downloadedVideosButton.SetActiveStatus(false);
            _favoriteVideosButton.SetActiveStatus(true);

            Gameplay_GameState_Video_Model.videosToWatch.Clear();
            Gameplay_GameState_Video_Model.videosToWatch.AddRange(Gameplay_GameState_Video_Model.favoriteVideos);
        }
    }
}