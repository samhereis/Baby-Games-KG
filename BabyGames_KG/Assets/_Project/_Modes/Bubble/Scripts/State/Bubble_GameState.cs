using Bubble;
using DataClasses;
using GameState;
using Identifiers;
using Interfaces;
using Services;
using _Project.Scripts.Services;
using SO;
using UnityEngine;
using Zenject;

namespace _Project._Modes.Hiding.Scripts.State
{
    public class Bubble_GameState : IGameState
    {
        [Inject] public ListOfAllScenes_SO listOfAllScenes;
        [Inject] public ISceneLoader sceneLoader;
        [Inject] public IGameStateService gameStateService;

        private Bubble_GameState_Model _model;
        private Bubble_GameState_View _view;

        public Bubble_GameState(Activity activity)
        {
            _model = new(activity);
        }

        public async void Enter()
        {
            DiService.Inject(this);

            await sceneLoader.ShowLoadingLayerAsync();
            await sceneLoader.LoadSceneAsyncWithTransition(listOfAllScenes.gameplay_Scene_Bubble.scene);

            await _model.Initialize();
            _model.activity_Identifier = await _model.activity_Identifier_Provider.GetActivity(_model.activity) as BubbleActivity_Identifier;
            _model.activity_Identifier = MonoBehaviour.Instantiate(_model.activity_Identifier);
            _model.bubbleController = _model.activity_Identifier.Get<BubbleController>();

            _view = new(_model);
            await _view.Initialize();

            _model.activity_Identifier.Initialize();

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