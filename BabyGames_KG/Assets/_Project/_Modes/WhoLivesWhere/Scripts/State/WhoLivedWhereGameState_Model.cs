using DataClasses;
using Hiding;
using Identifiers;
using Interfaces.Providers;
using Interfaces.Services;
using Services;
using SO;
using System;
using System.Threading.Tasks;
using UnityEngine;
using WhoLivesWhere;
using Zenject;

namespace _Project._Modes.Hiding.Scripts.State
{
    public class WhoLivedWhere_GameState_Model
    {
        public Action goToMainMenuRequest;
        public Action goToNextActivityRequest;
        public Action replayRequest;
        public Action onFinish;

        public WhoLivesWhereActivity_Identifier _activityIdentifier;
        public WLW_Controller controller;

        [Inject] public IContentDeliveryService contentDeliveryService;
        [Inject] public ListOfAllMenus_SO listOfAllMenus;

        [Inject] public IActivity_Identifier_Provider activity_Identifier_Provider;
        [Inject] public IActivities_SO_Provider activities_SO_Provider;

        public HidingWave currentWave;

        public Activity nextActivity { get; private set; }
        public Activity activity;
        public ActivityCategory_SO activityCategory_SO;

        public WhoLivedWhere_GameState_Model(Activity activity)
        {
            this.activity = activity;
        }

        public async Task Initialize()
        {
            DiService.Inject(this);

            this.nextActivity = await activity.GetNextActivity(activities_SO_Provider);
            await LoadAcitivtyIdentifier();
        }

        public async Task LoadAcitivtyIdentifier()
        {
            var gameObject = await contentDeliveryService.GetAsset<GameObject>(activity.GetUrl(), activity.GetName(), activity.version);
            var activityIdentifier = MonoBehaviour.Instantiate(gameObject);
            _activityIdentifier = activityIdentifier.GetComponent<WhoLivesWhereActivity_Identifier>();
            controller = _activityIdentifier.Get<WLW_Controller>();
        }
    }
}