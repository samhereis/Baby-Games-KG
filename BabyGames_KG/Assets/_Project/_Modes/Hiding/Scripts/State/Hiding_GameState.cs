using DataClasses;
using GameState;
using Helpers;
using Interfaces;
using Services;
using SO;
using UnityEngine.UIElements;
using Zenject;

namespace _Project._Modes.Hiding.Scripts.State
{
    public class Hiding_GameState : IGameState
    {
        [Inject] public ListOfAllScenes_SO listOfAllScenes;
        [Inject] public ISceneLoader sceneLoader;
        [Inject] public IGameStateService gameStateService;

        private Hiding_GameState_Model _model;
        private Hiding_GameState_View _view;

        public Hiding_GameState(Activity activity)
        {
            _model = new(activity);
        }

        public async void Enter()
        {
            DiService.Inject(this);

            await sceneLoader.ShowLoadingLayerAsync();
            await sceneLoader.LoadSceneAsyncWithTransition(listOfAllScenes.gameplay_Scene_Hiding.scene);

            await _model.Initialize();
            await _model.hidingActivity_Identifier.Initialize();

            await _model.hidingController.Initialize(_model);

            _view = new(_model);
            await _view.Initialize();

            await AsyncHelper.DelayFloat(0.1f);
            await sceneLoader.HideLoadingLayerAsync();

            _model.replayRequest += Replay;
            _model.goToMainMenuRequest += GoToMainMenu;
            _model.goToNextActivityRequest += GoNextGame;
        }

        public void Exit()
        {
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