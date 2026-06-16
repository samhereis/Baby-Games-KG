using DataClasses;
using GameState;
using Interfaces;
using Services;
using _Project.Scripts.Services;
using SO;
using Zenject;

namespace CoockingSalade
{
    public class CoockingSalade_GameState : IGameState
    {
        [Inject] public ListOfAllScenes_SO listOfAllScenes;
        [Inject] public ISceneLoader sceneLoader;
        [Inject] public IGameStateService gameStateService;

        private Coocking_GameState_Model _model;
        private CoockingSalade_GameState_View _view;

        public CoockingSalade_GameState(Activity activity)
        {
            _model = new(activity);
        }

        public async void Enter()
        {
            DiService.Inject(this);

            await sceneLoader.ShowLoadingLayerAsync();
            await sceneLoader.LoadSceneAsyncWithTransition(listOfAllScenes.gameplay_Scene_CookingSalade.scene);

            await _model.Initialize();
            _model.controller.Construct(_model);

            _view = new(_model);
            await _view.Initialize();

            await _model.controller.Initialize();

            await sceneLoader.HideLoadingLayerAsync();

            _model.replayRequest += Replay;
            _model.goToMainMenuRequest += GoToMainMenu;
            _model.goToNextActivityRequest += GoNextGame;
            GameEvents.GameOpened(_model.activity);
        }

        public void Exit()
        {
            GameEvents.GameClosed(_model.activity);
            _model.replayRequest -= Replay;
            _model.goToMainMenuRequest -= GoToMainMenu;
            _model.goToNextActivityRequest -= GoNextGame;
        }

        private void Replay()
        {
            MainMenu_GameState.TryOpenGame(_model.activity, gameStateService);
        }

        private void GoToMainMenu()
        {
            gameStateService.ChangeState(new MainMenu_GameState(MainMenu_GameState.OpenSettings.OpenActivitiesMenu));
        }

        private void GoNextGame()
        {
            MainMenu_GameState.TryOpenGame(_model.nextActivity, gameStateService);
        }
    }
}