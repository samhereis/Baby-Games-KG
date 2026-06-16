using CustomAttributes;
using DataClasses;
using GameState;
using Helpers;
using Interfaces;
using Loggers;
using PaintCore;
using Services;
using _Project.Scripts.Services;
using SO;
using System;
using UnityEngine;
using Zenject;

namespace Modes.Coloring
{
    public class Gameplay_GameState_Coloring : IGameState, ISubscribesToEvents
    {
        [Inject] private ISceneLoader _sceneLoader;
        [Inject] private ListOfAllScenes_SO _listOfAllScenes;

        private readonly Gameplay_GameState_Coloring_Model _model;

        private GameplayMenu_Coloring _gameplayMenu;

        public Gameplay_GameState_Coloring(Activity activity)
        {
            _model = new(activity);
        }

        public async void Enter()
        {
            _model.onFatalError += OnFatalError;
            string pictureName = "";

            try
            {
                DiService.Inject(this);

                await _sceneLoader.ShowLoadingLayerAsync();
                await _sceneLoader.LoadSceneAsyncWithTransition(_listOfAllScenes.gameplay_Scene_Coloring.scene);

                _model.Initialize();
                await _model.gameController.Initialize(_model);
                pictureName = _model.activity.GetName();

                foreach (var paintableTexture in CwPaintableTexture.Instances)
                {
                    paintableTexture.StateLimit = _model.gameSettings.drawSettings.undoMaxCount;
                }

                InitUI();

                await AsyncHelper.DelayFloat(0.5f);
                await _sceneLoader.HideLoadingLayerAsync();
                GameEvents.GameOpened(_model.activity);
            }
            catch (Exception ex)
            {
                ex = new Exception($"Could start gameplay - {pictureName}", ex);
                CustomLogger.instance?.LogException(ex);

                _model.onFatalError.Invoke(ex);
            }
        }

        public async void Exit()
        {
            GameEvents.GameClosed(_model.activity);
            try
            {
                _model.onFatalError -= OnFatalError;
                UnsubscribeFromEvents();

                _model.listOfAllMenus.Dispose();
                await _model.gameController.Dispose();
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
                _model.onClearRequested += OnClearDrawing;

                _gameplayMenu.clearButton.SetModel(_model);
                _gameplayMenu.buttonBack.onClick.AddListener(OnBackButtonClicked);
                _gameplayMenu.undoButton.onClick.AddListener(OnClickUndoButton);

                _gameplayMenu.playAnimation.onClick.AddListener(OnPlayAnimationButtonClicked);
                _gameplayMenu.saveDrawingbutton.onClick.AddListener(SaveScreenshot);
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
                _model.onClearRequested -= OnClearDrawing;

                _gameplayMenu.clearButton.SetModel(null);
                _gameplayMenu.buttonBack.onClick.RemoveListener(OnBackButtonClicked);
                _gameplayMenu.undoButton.onClick.RemoveListener(OnClickUndoButton);

                _gameplayMenu.saveDrawingbutton.onClick.RemoveListener(SaveScreenshot);
                _gameplayMenu.playAnimation.onClick.RemoveListener(OnPlayAnimationButtonClicked);
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex, "Could not UnsubscribeFromEvents:");
            }
        }

        private async void InitUI()
        {
            _gameplayMenu = await _model.listOfAllMenus.gameplayMenu_Coloring.InstantiateAsync();

            _model.pallete = _gameplayMenu.pallite;
            await _model.pallete.Initialize();

            _gameplayMenu.Enable(0);
            _model.pallete.HideAllPanels(0.25f);

            SubscribeToEvents();
            _gameplayMenu.hintHand.Initialize(_model);
        }

        private void OnFatalError(Exception message)
        {
            Debug.Log("Fatal Error");
            CustomLogger.instance?.LogException(message);
            GoToMainMenu();
        }

        private void GoToMainMenu()
        {
            _model.gameStateService.ChangeState(new MainMenu_GameState(MainMenu_GameState.OpenSettings.OpenActivitiesMenu));
        }

        private async void OnBackButtonClicked()
        {
            UnsubscribeFromEvents();

            _model.engineEyes.SetOpenEyes(true);
            await _sceneLoader.ShowLoadingLayerAsync();
            _model.gameController.WriteResult();
            _model.soundPlayer.Stop(_model.currentSpine.animationAudioClip);

            GoToMainMenu();
        }

        private void OnPlayAnimationButtonClicked()
        {
            _gameplayMenu.Deactivate();
            _model.gameController.PlayAnimation(() => { _gameplayMenu.Activate(); });
        }

        private async void OnClearDrawing()
        {
            Activity activity = _model.activity;

            try
            {
                UnsubscribeFromEvents();

                _model.gameSaveService.DeleteDrawings(_model.activity.acitvityCategory, _model.activity.GetName());
                _model.gameSaveService.DeleteBackgroundDrawings(_model.activity.acitvityCategory, _model.activity.GetName());
                _model.gameSaveService.DeleteIcon(_model.activity.acitvityCategory, _model.activity.GetName());

                await _sceneLoader.ShowLoadingLayerAsync();
                await _model.gameController.ClearDrawings();
            }
            catch (Exception ex)
            {
                GoToMainMenu();
                CustomLogger.instance?.LogException(ex, "Error during OnClearDrawing on Gameplay_GameState_SlotSeparation");
            } finally
            {
                MainMenu_GameState.TryOpenGame(activity, _model.gameStateService);
            }
        }

        private async void SaveScreenshot()
        {
            UnsubscribeFromEvents();
            await _model.gameController.SaveScreenshot();
            SubscribeToEvents();
        }

        [Fg_De, SerializeField] private int _numberOfUndos = 0;
        [Fg_De, SerializeField] private bool _canUndo = true;
        private async void OnClickUndoButton()
        {
            try
            {
                if (_canUndo == false) { return; }
                _canUndo = false;

                CwStateManager.UndoAll();
                await AsyncHelper.DelayFloat(0.25f);
                _canUndo = true;

                _numberOfUndos++;
                if (_numberOfUndos > 5)
                {
                    CwStateManager.ClearAllStates();
                    _numberOfUndos = 0;
                }
            }
            catch (Exception e)
            {
                CustomLogger.instance?.LogException(e, "Could not Undo All");
            }
        }
    }
}