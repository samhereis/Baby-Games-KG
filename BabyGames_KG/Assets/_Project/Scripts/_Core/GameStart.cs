using Coloring.Notification;
using Core.DI;
using DataClasses;
using GameState;
using Helpers;
using Interfaces;
using Interfaces.Providers;
using Loggers;
using ScriptableObjects;
using Services;
using SO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Project.Scripts.Data;
using UI;
using UI.Popups;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;
using Zenject;

namespace Core
{
    public class GameStart : MonoBehaviour
    {
        [Inject] private IGameStateService _gameStateService;
        [Inject] private IPurchasesService _purchasesService;
        [Inject] private IActivities_SO_Provider _activities_SO_Provider;
        [Inject] private NotificationBase _notification;
        [Inject] private GameSavableSettings _gameSavableSettings;

        public VideoPlayer videoPlayer;
        public Transform blur;

        private LazyUpdator_Service _lazyUpdator_Service;

        private bool _videoEnded;

        private void Awake()
        {
            Debug.Log("TempDebug: Awake");
            DontDestroyOnLoad(gameObject);

            if (Application.platform == RuntimePlatform.Android)
            {
                blur?.gameObject.SetActive(false);
            }

            Debug.Log("Application Version: " + Application.version);
        }

        private void OnEnable()
        {
            Application.targetFrameRate = 60;

            Init();

            PlayerActions_DataHolder.instance?.hintVisual_Hand.gameObject.SetActive(false);
            PlayerActions_DataHolder.instance?.hintVisual_HandWithAnimation.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            _lazyUpdator_Service.RemoveFromQueue(ClearJunk);
        }

        private async Task ClearJunk()
        {
            ClearablesHolder.instance.DeleteNulls();
            await AsyncHelper.DelayFloat(0.5f);
        }

#if UNITY_EDITOR
        public bool playVideoOnStart;
#endif
        public async void Init()
        {
#if UNITY_EDITOR
            if (playVideoOnStart) { PlayVideo(); }
            else { _videoEnded = true; }
#else
            PlayVideo();
#endif

            await AsyncHelper.WaitUntill(() => Global_DI.preloadStatus == Global_DI.PreloadStatus.DoneSuccessfuly, destroyCancellationToken);
            DiService.Inject(this);

            Activities_SO activities_SO = await _activities_SO_Provider.GetActivities_SO();

            while (activities_SO == null)
            {
                if (activities_SO == null)
                {
                    if (ApplicationHelper.HasInternetConnection() == false)
                    {
                        MessagesMenu.instance?.ShowMessage("no_internet_connection");
                    }
                    else
                    {
                        MessagesMenu.instance?.ShowMessage("server_error");
                    }
                }

                await AsyncHelper.DelayFloat(2f);
                activities_SO = await _activities_SO_Provider.GetActivities_SO();
            }

            DeleteUnsupportedGames(activities_SO);

            try
            {
                await _purchasesService?.Initialize();
                _gameSavableSettings.Initialize();
                _notification.Initialize(_gameSavableSettings);
            } catch (Exception ex)
            {
                CustomLogger.instance.LogException(ex, "Error during purchases init");
            }

#if UNITY_EDITOR
            await AsyncHelper.WaitWhile(() => _videoEnded == false, destroyCancellationToken);
#else
            await AsyncHelper.WaitWhile(() => _videoEnded == false, destroyCancellationToken);
#endif

            LanguageMenu.SetStartupLanguage();
            _gameStateService.ChangeState(new MainMenu_GameState(MainMenu_GameState.OpenSettings.None));

            _lazyUpdator_Service = new LazyUpdator_Service();
            _lazyUpdator_Service.AddToQueue(ClearJunk);
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (_videoEnded == true) { return; }
            if (Pointer.current?.press.isPressed == true)
            {
                _videoEnded = true;
                videoPlayer?.Stop();
            }
        }
#endif

        private async void PlayVideo()
        {
#if UNITY_EDITOR
            _videoEnded = true;
            return;
#endif
            videoPlayer.Play();
            await AsyncHelper.DelayFloat((float)videoPlayer.length);
            _videoEnded = true;
        }

        private void DeleteUnsupportedGames(Activities_SO activitiesSo)
        {
            try
            {
                string[] parts = Application.version.Split('.');
                Vector3Int current = new Vector3Int(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));

                foreach (var category in activitiesSo.activityCategories)
                {
                    category.activities.RemoveAll(activity =>
                    {
                        Vector3Int req = activity.gameVersionSupported;

                        if (current.x != req.x) { return current.x < req.x; }
                        if (current.y != req.y) { return current.y < req.y; }
                        return current.z < req.z;
                    });
                }
            } catch (Exception e) { CustomLogger.instance.LogException(e); }
        }
    }
}