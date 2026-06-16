using DataClasses;
using GameState;
using Identifiers;
using InterestGames;
using Interfaces;
using Loggers;
using Services;
using _Project.Scripts.Services;
using SO;
using System;
using Zenject;

namespace Modes.Puzzle
{
    public class Gameplay_GameState_InterestGameGirl : IGameState, ISubscribesToEvents
    {
        [Inject] private ISceneLoader _sceneLoader;
        [Inject] private ListOfAllScenes_SO _listOfAllScenes;
        [Inject] public IGameStateService gameStateService;

        private Gameplay_GameState_InterestGameGirl_Model _model;
        private Gameplay_GameState_InterestGameGirl_View _view;

        public Gameplay_GameState_InterestGameGirl(Activity activity)
        {
            _model = new(activity);
            _view = new(_model);
        }

        public async void Enter()
        {
            string pictureName = "";

            try
            {
                DiService.Inject(this);

                await _sceneLoader.ShowLoadingLayerAsync();
                await _sceneLoader.LoadSceneAsyncWithTransition(_listOfAllScenes.gameplay_Scene_Princess.scene);

                await _model.Initialize();
                await _view.Initialize();

                _model.activityIdentifier = await _model.activity_Identifier_Provider.InstantiaseActivity(_model.activity) as PrincessActivity_Identifier;
                _model.princessController = _model.activityIdentifier.Get<Princess_Controller>();
                _model.skinCombiner = _model.activityIdentifier.Get<SkinCombiner>();
                _model.princessController.Initialize(_model);

                SubscribeToEvents();

                _model.replayRequest += Replay;
                _model.goToMainMenuRequest += GoToMainMenu;
                _model.goToNextActivityRequest += GoNextGame;

                await _sceneLoader.HideLoadingLayerAsync();
                GameEvents.GameOpened(_model.activity);
            }
            catch (Exception ex)
            {
                ex = new Exception($"Could start gameplay - {pictureName}", ex);
                _model.onFatalError.Invoke(ex);
            }
        }

        public void Exit()
        {
            GameEvents.GameClosed(_model.activity);
            try
            {
                _model.replayRequest -= Replay;
                _model.goToMainMenuRequest -= GoToMainMenu;
                _model.goToNextActivityRequest -= GoNextGame;

                UnsubscribeFromEvents();
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex, "Could not SubscribeToEvents:");
            }
        }

        public void SubscribeToEvents()
        {
            try
            {
                _view.gameplayMenu_Princess.backButton.onClick.AddListener(GoToMainMenu);
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex, "Could not SubscribeToEvents:");
            }
        }

        public void UnsubscribeFromEvents()
        {
            try
            {
                _view.gameplayMenu_Princess.backButton.onClick.RemoveListener(GoToMainMenu);
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex, "Could not UnsubscribeFromEvents:");
            }
        }

        private void OnFatalError(Exception message)
        {
            CustomLogger.instance?.LogException(message);
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