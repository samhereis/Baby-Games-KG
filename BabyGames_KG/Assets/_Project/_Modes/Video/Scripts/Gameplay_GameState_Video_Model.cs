using _Project.Scripts.Services;
using DataClasses;
using GameState;
using Identifiers;
using Interfaces.Providers;
using Interfaces.Services;
using Observables;
using Services;
using SO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Project.Scripts.Services.Purchase;
using UnityEngine;
using UnityEngine.Video;
using Zenject;

namespace Video
{
    public class Gameplay_GameState_Video_Model
    {
        public static Action onAnyVideoListChanged;

        private const string _iS_FAVORITED_VIDEO = nameof(_iS_FAVORITED_VIDEO);

        public static List<Activity> downloadedVideos = new();
        public static List<Activity> favoriteVideos = new();
        public static List<Activity> videosToWatch = new();

        public Action onGoToMenuRequested;
        public Action<Activity> onChangeVideoRequested;
        public ObservableValue<bool> areControlsLocked = new("AreControlsLocked");

        [Inject] public IContentDeliveryService contentDeliveryService;
        [Inject] public ListOfAllMenus_SO listOfAllMenus_SO;
        [Inject] public IActivity_Identifier_Provider activity_Identifier_Provider;
        [Inject] public BackgroundMusicService backgroundMusicService;
        [Inject] public ISubscriptionChecker subscriptionChecker;

        public Activity activity { get; private set; }
        public VideoActivity_Identifier _videoActivity_Identifier { get; private set; }
        public VideoClip videoClip { get; private set; }

        public Gameplay_GameState_Video_Model(Activity newActivity)
        {
            activity = newActivity;
        }

        public async Task Initialize()
        {
            DiService.Inject(this);

            _videoActivity_Identifier = await activity_Identifier_Provider.GetActivity(activity) as VideoActivity_Identifier;
            _videoActivity_Identifier = MonoBehaviour.Instantiate(_videoActivity_Identifier);

            videoClip = await _videoActivity_Identifier.GetVideoClipAsync(activity, contentDeliveryService);

            if (subscriptionChecker.IsSubscribed() == false)
            {
                videosToWatch.RemoveAll(x => x.isAlwaysUnlocked == false);
            }
        }

        public static void UpdateDownloadedVideos(IContentDeliveryService contentDeliveryService)
        {
            Gameplay_GameState_Video_Model.downloadedVideos.Clear();

            foreach (var item in MainMenu_GameState_Model.selectedActivityCategory.activities)
            {
                if (contentDeliveryService.IsAssetCashed(item.GetUrl(), item.version))
                {
                    Gameplay_GameState_Video_Model.downloadedVideos.Add(item);
                }
            }

            Gameplay_GameState_Video_Model.onAnyVideoListChanged?.Invoke();
        }

        public static void AddOrDeleteFavorite(Activity activityData, ActivityCategory_SO activityCategory_SO)
        {
            if (IsFavoritedVideo(activityData))
            {
                PlayerPrefs.DeleteKey($"{_iS_FAVORITED_VIDEO}_{activityData.activityName}");
            }
            else
            {
                PlayerPrefs.SetString($"{_iS_FAVORITED_VIDEO}_{activityData.activityName}", activityData.activityName);
            }

            UpdateFavoriteVideos(activityCategory_SO);
        }

        public static bool IsFavoritedVideo(Activity activityData)
        {
            return PlayerPrefs.GetString($"{_iS_FAVORITED_VIDEO}_{activityData.activityName}") == activityData.activityName;
        }

        public static void UpdateFavoriteVideos(ActivityCategory_SO activityCategory_SO)
        {
            favoriteVideos.Clear();

            foreach (var item in activityCategory_SO.activities)
            {
                if (PlayerPrefs.GetString($"{_iS_FAVORITED_VIDEO}_{item.activityName}") == item.activityName)
                {
                    favoriteVideos.Add(item);
                }
            }

            Gameplay_GameState_Video_Model.onAnyVideoListChanged?.Invoke();
        }
    }
}