using _Project._Modes.Hiding.Scripts.State;
using _Project.Scripts.FX.Movement;
using Carwash;
using ColorfulTrain;
using CoockingSalade;
using DataClasses;
using Identifiers;
using Interfaces;
using Interfaces.Services;
using Loggers;
using Modes.Coloring;
using Modes.Puzzle;
using Modes.Sorting;
using Services;
using SO;
using System;
using _Project.Scripts.Data;
using _Project.Scripts.Services;
using _Project.Scripts.UI.Purchase;
using Helpers;
using UnityEngine;
using Video;
using Zenject;

namespace GameState
{
    public class MainMenu_GameState : IGameState, ISubscribesToEvents
    {
        public enum OpenSettings
        {
            None,
            OpenActivitiesMenu
        }

        [Inject] public ListOfAllScenes_SO listOfAllScenes;
        private static ISceneLoader _sceneLoader;

        private MainMenu_GameState_Model _model;
        private MainMenu_GameState_View _view;

        private bool _doUpdate = false;
        private static bool _lastTimeSubsWasActive
        {
            get { return PlayerPrefs.GetString(nameof(_lastTimeSubsWasActive)) == true.ToString(); }
            set { PlayerPrefs.SetString(nameof(_lastTimeSubsWasActive), value.ToString()); }
        }
        public MainMenu_GameState(OpenSettings openSettings)
        {
            _model = new(openSettings);
            _view = new(_model);
        }

        public async void Enter()
        {
            PlayerActions_DataHolder.instance.hintVisual_Hand.gameObject.SetActive(false);
            PlayerActions_DataHolder.instance.hintVisual_HandWithAnimation.gameObject.SetActive(false);

            DiService.Inject(this);

            if (_sceneLoader == null) { _sceneLoader = DiService.Get<ISceneLoader>(); }

            await _sceneLoader.ShowLoadingLayerAsync();
            await _sceneLoader.LoadSceneAsyncWithTransition(listOfAllScenes.mainMenu_Scene.scene);
            ForceDeleteAllLeftOverUIs();
            if (ClearablesHolder.instance != null) { ClearablesHolder.instance.ClearAsync(); }

            ForceDeleteAllLeftOverUIs();
            _model.Initialize();
            _view.Initialize();

            SubscribeToEvents();
            await _sceneLoader.HideLoadingLayerAsync();

            _model.backgroundMusicService?.PlayMusicFor("MainMenu");

            // Counts main-menu opens; fires the native review prompt on the Nth open (throttled + OS-limited).
            DiService.Get<RateUs>()?.TryAutoReview();

            _doUpdate = true;
            while (_doUpdate)
            {
                TryShowSubscriptionInfo();
                await AsyncHelper.DelayFloat(2);
            }
        }

        public void Exit()
        {
            _doUpdate = false;

            UnsubscribeFromEvents();
            _model.Dispose();
            _view.Dispose();
        }

        private void ForceDeleteAllLeftOverUIs()
        {
            try
            {
                foreach (var item in MonoBehaviour.FindObjectsByType<WinMenu_Universal>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    MonoBehaviour.Destroy(item.gameObject);
                }

                foreach (var item in MonoBehaviour.FindObjectsByType<FinalAnimation_FX>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    MonoBehaviour.Destroy(item.gameObject);
                }
            } catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }
        }

        public void SubscribeToEvents()
        {
            UnsubscribeFromEvents();

            _model.onActivityChosen += TryOpenGame;
            _model.onPurchaseRequested += TryPurchase;
            _model.onReloadRequested += Reload;
        }

        public void UnsubscribeFromEvents()
        {
            _model.onActivityChosen -= TryOpenGame;
            _model.onPurchaseRequested -= TryPurchase;
            _model.onReloadRequested -= Reload;
        }

        private void Reload()
        {
            _model.gameStateService?.ChangeState(new MainMenu_GameState(_model.openSettings));
        }

        private void TryOpenGame(Activity activity)
        {
            if (activity.IsUnlocked(_model.subscriptionChecker))
            {
                TryOpenGame(activity, _model.gameStateService);
            }
            else
            {
                _model.onPurchaseRequested?.Invoke();
            }
        }

        public static async void TryOpenGame(Activity activity, IGameStateService gameStateService)
        {
            await _sceneLoader?.ShowLoadingLayerAsync();

            PlayerActions_DataHolder.instance.hintVisual_Hand.gameObject.SetActive(false);
            PlayerActions_DataHolder.instance.hintVisual_HandWithAnimation.gameObject.SetActive(false);

            if (ClearablesHolder.instance != null) { await ClearablesHolder.instance.ClearAsync(); }

            switch (activity.type)
            {
                case ActivityType.Coloring:
                {
                    gameStateService?.ChangeState(new Gameplay_GameState_Coloring(activity));
                    break;
                }
                case ActivityType.Puzzle:
                {
                    gameStateService?.ChangeState(new Gameplay_GameState_Puzzle(activity));
                    break;
                }
                case ActivityType.Orchestra:
                {
                    gameStateService?.ChangeState(new Gameplay_GameState_Orchestra(activity));
                    break;
                }
                case ActivityType.Video:
                {
                    var contentDeliveryService = DiService.Get<IContentDeliveryService>();
                    Gameplay_GameState_Video_Model.UpdateDownloadedVideos(contentDeliveryService);
                    gameStateService?.ChangeState(new Gameplay_GameState_Video(activity));
                    break;
                }
                case ActivityType.Hiding:
                {
                    gameStateService?.ChangeState(new Hiding_GameState(activity));
                    break;
                }
                case ActivityType.Bubble:
                {
                    gameStateService?.ChangeState(new Bubble_GameState(activity));
                    break;
                }
                case ActivityType.Carwash:
                {
                    gameStateService?.ChangeState(new Carwash_GameState(activity));
                    break;
                }
                case ActivityType.WhoLivesWhere:
                {
                    gameStateService?.ChangeState(new WhoLivedWhere_GameState(activity));
                    break;
                }
                case ActivityType.CarTuning:
                {
                    gameStateService?.ChangeState(new CarTuning_GameState(activity));
                    break;
                }
                case ActivityType.InterestGame_Girl:
                {
                    if (activity.subtype == typeof(RoomCleaningActivity_Identifier).Name)
                    {
                        gameStateService?.ChangeState(new GameplayGameState_RoomCleaning(activity));
                    }
                    else if (activity.subtype == typeof(MakeupActivity_Identifier).Name)
                    {
                        gameStateService?.ChangeState(new GameplayGameState_Makeup(activity));
                    }
                    else
                    {
                        gameStateService?.ChangeState(new Gameplay_GameState_InterestGameGirl(activity));
                    }
                    break;
                }
                case ActivityType.ColorfulTrain:
                {
                    gameStateService?.ChangeState(new ColorfulTrain_GameState(activity));
                    break;
                }
                case ActivityType.Cooking:
                {
                    gameStateService?.ChangeState(new CoockingSalade_GameState(activity));
                    break;
                }
            }

        }

        private void TryPurchase()
        {
            _view.OpenParentalGate(async () =>
            {
                var lifetimePurchaseMenu = await _model.listOfAllMenus.purchaseMenu.InstantiateAsync();
                lifetimePurchaseMenu.Show(() => { _model.onReloadRequested?.Invoke(); }, () => { _model.onReloadRequested?.Invoke(); });
            }, null);
        }

        private async void TryShowSubscriptionInfo()
        {
            try
            {
                var isSubscribed = _model.subscriptionController.IsSubscribed();

                if (isSubscribed == false)
                {
                    if (_lastTimeSubsWasActive)
                    {
                        var popup = await _model.listOfAllMenus.subscriptionExp_Popup.InstantiateAsync();
                        popup.GetComponent<PurchaseMenu_Data>().Init(_model.subscriptionChecker);
                        popup.Open();
                    }
                }

                _lastTimeSubsWasActive = isSubscribed;
            } catch (Exception e)
            {
                CustomLogger.instance?.LogException(e);
            }
        }
    }
}