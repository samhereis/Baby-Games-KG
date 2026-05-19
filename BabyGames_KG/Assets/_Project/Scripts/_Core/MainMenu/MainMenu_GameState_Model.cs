using _Project.Scripts.Services;
using DataClasses;
using Interfaces.Providers;
using Interfaces.Services;
using Services;
using SO;
using System;
using _Project.Scripts.Services.Purchase;
using UnityEngine;
using Zenject;
using static GameState.MainMenu_GameState;

namespace GameState
{
    public class MainMenu_GameState_Model
    {
        public OpenSettings openSettings;

        public Action<ActivityCategory_SO> onActivityCategoryOpenRequested;
        public Action onPurchaseRequested;

        public Action<Activity> onActivityChosen;

        public Action onReloadRequested;

        [Inject] public ListOfAllMenus_SO listOfAllMenus;

        [Inject] public IGameStateService gameStateService;

        [Inject] public IActivityCategory_UIUnits_Provider activityCategory_UIUnits_Provider;
        [Inject] public IActivity_UIUnits_Provider activity_UIUnits_Provider;

        [Inject] public IContentDeliveryService contentDeliveryService;
        [Inject] public IActivity_Identifier_Provider activity_Identifier_Provider;
        [Inject] public ISubscriptionChecker subscriptionChecker;
        [Inject] public SubscriptionController subscriptionController;

        [Inject] public BackgroundMusicService backgroundMusicService;
        [Inject] public GameSaveService gameSaveService;

        [Inject] public ISoundPlayer soundPlayer;

        public static Activity selectedActivity;
        public static ActivityCategory_SO selectedActivityCategory;

        private const string SAVED_PANNEL_NUMBER_KEY = nameof(SAVED_PANNEL_NUMBER_KEY);
        private const string SAVED_PANNEL_POSITION_KEY = nameof(SAVED_PANNEL_POSITION_KEY);

        public int savedPannelNumber
        {
            get { return PlayerPrefs.GetInt(SAVED_PANNEL_NUMBER_KEY, 1); }
            set { PlayerPrefs.SetInt(SAVED_PANNEL_NUMBER_KEY, value); }
        }

        public float savedPannelPosition
        {
            get { return PlayerPrefs.GetFloat(SAVED_PANNEL_POSITION_KEY, 1); }
            set { PlayerPrefs.SetFloat(SAVED_PANNEL_POSITION_KEY, value); }
        }

        public MainMenu_GameState_Model(OpenSettings newOpenSettings)
        {
            openSettings = newOpenSettings;
        }

        public void Initialize()
        {
            DiService.Inject(this);

            onActivityCategoryOpenRequested += SetSelectedCategory;
            onActivityChosen += SetSelectedActivity;
        }

        public void Dispose()
        {
            onActivityCategoryOpenRequested -= SetSelectedCategory;
            onActivityChosen -= SetSelectedActivity;
        }

        private void SetSelectedCategory(ActivityCategory_SO sO)
        {
            selectedActivityCategory = sO;
        }

        private void SetSelectedActivity(Activity activity)
        {
            selectedActivity = activity;
        }
    }
}