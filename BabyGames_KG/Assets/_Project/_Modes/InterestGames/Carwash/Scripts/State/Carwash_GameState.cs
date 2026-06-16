using Assets._Project._Modes.Carwash.Scripts.State;
using DataClasses;
using GameState;
using Identifiers;
using Interfaces;
using Services;
using _Project.Scripts.Services;
using SO;
using UnityEngine;
using Zenject;

namespace Carwash
{
    public class Carwash_GameState : IGameState
    {
        [Inject] public ListOfAllScenes_SO listOfAllScenes;
        [Inject] public ISceneLoader sceneLoader;
        [Inject] public IGameStateService gameStateService;

        private Carwash_GameState_Model _model;
        private Carwash_GameState_View _view;

        public Carwash_GameState(Activity activity)
        {
            _model = new(activity);
        }

        public async void Enter()
        {
            DiService.Inject(this);

            await sceneLoader.ShowLoadingLayerAsync();
            await sceneLoader.LoadSceneAsyncWithTransition(listOfAllScenes.gameplay_Scene_Carwash.scene);

            await _model.Initialize();
            _model.activity_Identifier = await _model.activity_Identifier_Provider.GetActivity(_model.activity) as CarwashActivity_Identifier;
            _model.activity_Identifier = MonoBehaviour.Instantiate(_model.activity_Identifier);

            _view = new(_model);
            await _view.Initialize();

            _model.carwash_Controller = _view.gameplayMenu.Get<Carwash_Controller>();
            _model.carwash_Controller.Construct(_model);
            _model.carwash_Controller.Initialize();

            await sceneLoader.HideLoadingLayerAsync();

            _model.goToMainMenuRequest += GoToMainMenu;
            _model.goToNextActivityRequest += GoNextGame;
            _model.replayRequest += Replay;
            GameEvents.GameOpened(_model.activity);
        }

        public void Exit()
        {
            GameEvents.GameClosed(_model.activity);
            _model.goToMainMenuRequest -= GoToMainMenu;
            _model.goToNextActivityRequest -= GoNextGame;
            _model.replayRequest -= Replay;
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