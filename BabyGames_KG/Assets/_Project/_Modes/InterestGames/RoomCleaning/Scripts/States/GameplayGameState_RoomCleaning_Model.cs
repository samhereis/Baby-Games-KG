using DataClasses;
using GameState;
using Helpers;
using Identifiers;
using InterestGames;
using Interfaces.Providers;
using Observables;
using Services;
using SO;
using System;
using System.Threading.Tasks;
using Modes.Coloring;
using Zenject;

namespace Modes.Puzzle
{
    public class GameplayGameState_RoomCleaning_Model
    {
        public Action goToMainMenuRequest;
        public Action goToNextActivityRequest;
        public Action replayRequest;
        public Action onNextStateRequested;

        public ObservableValue<bool> hasWon = new("hasWon");

        public Action<Exception> onFatalError { get; set; }

        [Inject] public IActivity_Identifier_Provider activity_Identifier_Provider;
        [Inject] public IActivities_SO_Provider activities_SO_Provider;
        [Inject] public IGameStateService gameStateService;

        [Inject] public ListOfAllMenus_SO listOfAllMenus_SO;
        [Inject] public Content content;

        public RoomCleaningActivity_Identifier activityIdentifier { get; set; }
        public Interests_RoomCleaning_Controller princessController { get; set; }
        public SkinCombiner _skinCombiner { get; set; }

        public Activity activity { get; protected set; }
        public Activity nextActivity { get; protected set; }

        public bool firstEnterDone = false;

        public GameplayGameState_RoomCleaning_Model(Activity newActivity)
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