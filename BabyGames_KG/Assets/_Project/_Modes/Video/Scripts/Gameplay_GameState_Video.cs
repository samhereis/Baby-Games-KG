using DataClasses;
using GameState;
using Interfaces;
using Services;
using _Project.Scripts.Services;
using SO;
using Zenject;

namespace Video
{
    public class Gameplay_GameState_Video : IGameState
    {
        [Inject] private ISceneLoader _sceneLoader;
        [Inject] private IGameStateService _gameStateService;
        [Inject] private ListOfAllScenes_SO _listOfAllScenes;

        private Gameplay_GameState_Video_Model _model;
        private Gameplay_GameState_Video_View _view;

        public Gameplay_GameState_Video(Activity newActivity)
        {
            _model = new(newActivity);
        }

        public async void Enter()
        {
            DiService.Inject(this);

            await _sceneLoader.ShowLoadingLayerAsync();
            await _sceneLoader.LoadSceneAsyncWithTransition(_listOfAllScenes.gameplay_Scene_Video.scene);

            await _model.Initialize();
            _model.backgroundMusicService?.ChangeVolume_External(0);

            _view = new(_model);
            await _view.Initialize();

            _model.onGoToMenuRequested += GoToMainMenu;
            _model.onChangeVideoRequested += ChangeVideo;

            await _sceneLoader.HideLoadingLayerAsync();
            GameEvents.GameOpened(_model.activity);
        }

        public void Exit()
        {
            GameEvents.GameClosed(_model.activity);
        }

        private void GoToMainMenu()
        {
            _gameStateService.ChangeState(new MainMenu_GameState(MainMenu_GameState.OpenSettings.OpenActivitiesMenu));
            _model.backgroundMusicService?.ChangeVolume_External(1);
        }

        private void ChangeVideo(Activity activity)
        {
            MainMenu_GameState.TryOpenGame(activity, _gameStateService);
        }
    }
}