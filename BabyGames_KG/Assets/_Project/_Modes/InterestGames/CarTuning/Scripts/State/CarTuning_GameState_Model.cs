using CarTuning;
using DataClasses;
using FX;
using Identifiers;
using Interfaces.Providers;
using Observables;
using Services;
using SO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zenject;

namespace Assets._Project._Modes.Carwash.Scripts.State
{
    public class CarTuning_GameState_Model
    {
        public Action goToMainMenuRequest;
        public Action goToNextActivityRequest;
        public Action replayRequest;
        public Action onStateCompleted;
        public Action onFinish;

        public Action<List<CarPart_Base>> onInitializeState;
        public Action onColorState;

        public Action onCompleteClicked;

        public ObservableValue<CarTuning_Car_Identifier> currentCar = new("CurrectCar");
        public ObservableValue<CarPart_Base> currentCarPart = new("CurrectCarPart");
        public ObservableValue<string> currentColor = new("CurrectColor");

        [Inject] public IActivity_Identifier_Provider activity_Identifier_Provider;
        [Inject] public IActivities_SO_Provider activities_SO_Provider;
        [Inject] public StateEnd_FX stateEnd_FX;
        [Inject] public ISoundPlayer soundPlayer;

        [Inject] public ListOfAllMenus_SO listOfAllMenus;

        public CarTuning_Identifier activity_Identifier;
        public CarTuning_Controller _controller;

        public Activity activity { get; private set; }
        public Activity nextActivity { get; private set; }
        public ActivityCategory_SO activityCategory_SO { get; private set; }

        public bool isFirstEntered = false;

        public CarTuning_GameState_Model(Activity newCctivity)
        {
            activity = newCctivity;
        }

        public async Task Initialize()
        {
            DiService.Inject(this);
            this.nextActivity = await activity.GetNextActivity(activities_SO_Provider);
        }
    }
}