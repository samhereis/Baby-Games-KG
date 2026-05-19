using DataClasses;
using Interfaces.Providers;
using Interfaces.Services;
using Observables;
using Services;
using SO;
using System;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace ColorfulTrain
{
    public class ColorfulTrain_GameState_Model
    {
        public Action goToMainMenuRequest;
        public Action goToNextActivityRequest;
        public Action replayRequest;
        public Action onFinish;

        public ColorfulTrainActivity_Identifier _activityIdentifier;
        public ColorfulTrain_TrainIdentifier train;
        public ColorfulTrain_Controller controller;

        [Inject] public IContentDeliveryService contentDeliveryService;
        [Inject] public ListOfAllMenus_SO listOfAllMenus;

        [Inject] public IActivity_Identifier_Provider activity_Identifier_Provider;
        [Inject] public IActivities_SO_Provider activities_SO_Provider;

        public Activity nextActivity { get; private set; }
        public Activity activity;
        public ActivityCategory_SO activityCategory_SO;

        public ColorfulTrain_GameState_Model(Activity activity)
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
            _activityIdentifier = activityIdentifier.GetComponent<ColorfulTrainActivity_Identifier>();
            train = _activityIdentifier.Get<ColorfulTrain_TrainIdentifier>();
            controller = _activityIdentifier.Get<ColorfulTrain_Controller>();
        }
    }
}