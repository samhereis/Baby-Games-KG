using _Project._Modes.Orchestra.Scripts;
using DataClasses;
using GameState;
using Identifiers;
using Interfaces;
using Services;
using SO;
using Zenject;

namespace Modes.Sorting
{
    public class Gameplay_GameState_Orchestra : IGameState
    {
        [Inject] public ListOfAllScenes_SO listOfAllScenes;
        [Inject] public ISceneLoader sceneLoader;
        [Inject] public IGameStateService gameStateService;

        private Gameplay_GameState_Orchestra_Model _model;
        private Gameplay_GameState_Orchestra_View _view;

        private Activity _activity;

        public Gameplay_GameState_Orchestra(Activity activity)
        {
            _activity = activity;
        }

        public async void Enter()
        {
            DiService.Inject(this);

            await sceneLoader.ShowLoadingLayerAsync();
            await sceneLoader.LoadSceneAsyncWithTransition(listOfAllScenes.gameplay_Scene_Orchestra.scene);

            _model = new(_activity);
            await _model.Initialize();

            _model.activity_Identifier = await _model.activity_Identifier_Provider.InstantiaseActivity(_model.activity) as OrchestraActivity_Identifier;
            _model.activity_Identifier.Initialize(_model.orchestra_Data);

            _view = new(_model);
            await _view.Initialize();

            await _model.activity_Identifier.Get<OrchestraController>().Initialize(_model, _view);

            _model.replayRequest += Replay;
            _model.goToMainMenuRequest += GoToMainMenu;
            _model.goToNextActivityRequest += GoNextGame;


            await sceneLoader.HideLoadingLayerAsync();
        }

        public void Exit()
        {
            _model.replayRequest -= Replay;
            _model.goToMainMenuRequest -= GoToMainMenu;
        }

        private void Replay()
        {
            MainMenu_GameState.TryOpenGame(_model.activity, gameStateService);
        }

        private void GoNextGame()
        {
            MainMenu_GameState.TryOpenGame(_model.nextActivity, _model.gameStateService);
        }

        private void GoToMainMenu()
        {
            _model.gameStateService.ChangeState(new MainMenu_GameState(MainMenu_GameState.OpenSettings.OpenActivitiesMenu));
        }
    }
}