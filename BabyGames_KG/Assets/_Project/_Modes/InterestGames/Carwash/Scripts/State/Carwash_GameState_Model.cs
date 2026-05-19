using Carwash;
using DataClasses;
using Identifiers;
using Interfaces.Providers;
using Services;
using SO;
using System;
using System.Threading.Tasks;
using Zenject;

namespace Assets._Project._Modes.Carwash.Scripts.State
{
    public class Carwash_GameState_Model
    {
        public Action goToMainMenuRequest;
        public Action goToNextActivityRequest;
        public Action replayRequest;

        public Action<CarSelecButton> onCarSelected;
        public Action<CarSelecButton> onCarEntered;
        public Action onCarFinished;
        public Action onStartOver;
        public Action onFinish;


        [Inject] public IActivity_Identifier_Provider activity_Identifier_Provider;
        [Inject] public IActivities_SO_Provider activities_SO_Provider;

        [Inject] public ListOfAllMenus_SO listOfAllMenus;

        public CarwashActivity_Identifier activity_Identifier;
        public Carwash_Controller carwash_Controller;

        public Activity activity { get; private set; }
        public Activity nextActivity { get; private set; }
        public ActivityCategory_SO activityCategory_SO { get; private set; }

        public CarSelecButton currentCarSelection;
        public bool isWaitingForCarSelection;

        public Carwash_GameState_Model(Activity newCctivity)
        {
            activity = newCctivity;
        }

        public async Task Initialize()
        {
            DiService.Inject(this);
            this.nextActivity = await activity.GetNextActivity(activities_SO_Provider);
        }

        public void SelectCar(CarSelecButton carSelecButton)
        {
            currentCarSelection = carSelecButton;
            onCarSelected?.Invoke(carSelecButton);
        }
    }
}