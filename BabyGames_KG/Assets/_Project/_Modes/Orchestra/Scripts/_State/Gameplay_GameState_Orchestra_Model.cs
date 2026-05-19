using _Project._Modes.Orchestra.Scripts;
using _Project._Modes.Orchestra.Scripts.SO;
using DataClasses;
using GameState;
using Identifiers;
using Interfaces.Providers;
using Services;
using SO;
using System;
using System.Threading.Tasks;
using Zenject;

namespace Modes.Sorting
{
    public class Gameplay_GameState_Orchestra_Model
    {
        public Action onWin;
        public Action<OrchestraWaveUnit> onWaveUnitCompleted;
        public Action<OrchestraWaveData> onWaveCompleted;
        public Action<OrchestraWaveUnit> doConfetti;

        public Action<bool> onPanelVisibilityChanged;

        public Action goToMainMenuRequest;
        public Action goToNextActivityRequest;
        public Action replayRequest;

        [Inject] public Orchestra_Data orchestra_Data;

        [Inject] public ListOfAllMenus_SO listOfAllMenus;
        [Inject] public IGameStateService gameStateService;

        [Inject] public IActivity_Identifier_Provider activity_Identifier_Provider;
        [Inject] public IActivities_SO_Provider activities_SO_Provider;

        public Activity activity { get; private set; }
        public Activity nextActivity { get; private set; }
        public OrchestraActivity_Identifier activity_Identifier { get; set; }

        public OrchestraWaveData currentOrchestraWaveData { get; set; }

        public Gameplay_GameState_Orchestra_Model(Activity newActivity)
        {
            activity = newActivity;
            DiService.Inject(this);
        }

        public async Task Initialize()
        {
            this.nextActivity = await activity.GetNextActivity(activities_SO_Provider);
        }
    }
}