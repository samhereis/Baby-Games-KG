using DataClasses;
using FX;
using Interfaces;
using Interfaces.Providers;
using Interfaces.Services;
using Observables;
using Services;
using SO;
using System;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace CoockingSalade
{
    public class Coocking_GameState_Model
    {
        public Action goToMainMenuRequest;
        public Action goToNextActivityRequest;
        public Action replayRequest;
        public Action onFinish;
        public Action requestCompleteButtonShow;
        public Action requestCompleteButtonHide;
        public Action onCompleteButtonPressed;

        public Action<IDraggable> onDecorDropped;

        public CoockingActivity_Identifier _activityIdentifier;
        public Coocking_Controller controller;

        public ObservableValue<CoockingSalade_StateBase> currentState = new("CurrectCar");

        [Inject] public IContentDeliveryService contentDeliveryService;
        [Inject] public ListOfAllMenus_SO listOfAllMenus;

        [Inject] public IActivity_Identifier_Provider activity_Identifier_Provider;
        [Inject] public IActivities_SO_Provider activities_SO_Provider;
        [Inject] public StateEnd_FX stateEnd_FX;

        public Activity nextActivity { get; private set; }
        public Activity activity;
        public ActivityCategory_SO activityCategory_SO;

        public Coocking_GameState_Model(Activity activity)
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
            _activityIdentifier = activityIdentifier.GetComponent<CoockingActivity_Identifier>();
            controller = _activityIdentifier.Get<Coocking_Controller>();
        }
    }
}