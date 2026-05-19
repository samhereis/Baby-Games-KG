using CarTuning;
using DataClasses;
using GameState;
using Helpers;
using Identifiers;
using Interfaces.Providers;
using Observables;
using InterestGames;
using Services;
using SO;
using System;
using System.Threading.Tasks;
using Zenject;

namespace Modes.Puzzle
{
    public class Gameplay_GameState_InterestGameGirl_Model
    {
        public Action goToMainMenuRequest;
        public Action goToNextActivityRequest;
        public Action replayRequest;
        public Action onNextStateRequested;

        public ObservableValue<bool> hasWon = new("hasWon");
        public ObservableValue<Princess_DressBase> currentDress = new("CurrentDress");
        public ObservableValue<Princess_StateBase> currentState = new("CurrentState");

        public Action<Exception> onFatalError { get; set; }

        [Inject] public IActivity_Identifier_Provider activity_Identifier_Provider;
        [Inject] public IActivities_SO_Provider activities_SO_Provider;
        [Inject] public IGameStateService gameStateService;

        [Inject] public ListOfAllMenus_SO listOfAllMenus_SO;

        public PrincessActivity_Identifier activityIdentifier { get; set; }
        public Princess_Controller princessController { get; set; }
        public SkinCombiner skinCombiner { get; set; }

        public Activity activity { get; protected set; }
        public Activity nextActivity { get; protected set; }
        public bool firstEnterDone = false;

        public Gameplay_GameState_InterestGameGirl_Model(Activity newActivity)
        {
            activity = newActivity;
        }

        public virtual async Task Initialize()
        {
            DiService.Inject(this);
            nextActivity = await activity.GetNextActivity(activities_SO_Provider);
        }

        public async Task Dispose()
        {
            await AsyncHelper.NextFrame();
        }
    }
}