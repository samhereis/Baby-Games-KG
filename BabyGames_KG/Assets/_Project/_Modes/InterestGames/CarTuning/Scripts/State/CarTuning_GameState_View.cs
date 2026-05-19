using Assets._Project._Modes.Carwash.Scripts.State;
using CarTuning;
using DG.Tweening;
using Helpers;
using Modes.Sorting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Carwash
{
    public class CarTuning_GameState_View
    {
        private CarTuning_GameState_Model _model;

        public GameplayMenu_CarTuning gameplayMenu;
        private WinMenu_Universal _winMenu;

        public CarTuning_GameState_View(CarTuning_GameState_Model model)
        {
            _model = model;
        }

        public async Task Initialize()
        {
            gameplayMenu = await _model.listOfAllMenus.gameplayMenu_CarTuning.InstantiateAsync();
            gameplayMenu.Construct(_model);

            _winMenu = await _model.listOfAllMenus.winMenu_Universal.InstantiateAsync();

            gameplayMenu.backButton.onClick.AddListener(() => { _model.goToMainMenuRequest?.Invoke(); });

            gameplayMenu.checkMark.onClick.AddListener(() =>
            {
                _model.onCompleteClicked?.Invoke();
                gameplayMenu.checkMark.SetAcitve(false);
            });

            _model.onFinish += OnFinish;

            _winMenu.mainMenu.onClick.AddListener(() => { _model.goToMainMenuRequest?.Invoke(); });
            _winMenu.next.onClick.AddListener(() => { _model.goToNextActivityRequest?.Invoke(); });
            _winMenu.replay.onClick.AddListener(() => { _model.replayRequest?.Invoke(); });

            _model.onInitializeState += OnInitializeState;
            _model.onStateCompleted += CurrentCarPartChanged;

            _model.onColorState += OnColorState;
        }

        private async void OnFinish()
        {
            gameplayMenu.Disable(1f);
            await AsyncHelper.DelayFloat(2f);
            _winMenu.Enable();
        }

        private void OnInitializeState(List<CarPart_Base> list)
        {
            gameplayMenu.Initialize(list);
        }

        private void OnColorState()
        {
            _model.onStateCompleted -= CurrentCarPartChanged;
            _model.onStateCompleted += OnWin;

            gameplayMenu.ColorMode();
        }

        private void CurrentCarPartChanged()
        {
            gameplayMenu.checkMark.DOKill();
            gameplayMenu.checkMark.SetAcitve(_model.currentCarPart.value != null);
        }

        private void OnWin()
        {
            _model.onStateCompleted -= OnWin;

            gameplayMenu.checkMark.DOKill();
            gameplayMenu.checkMark.SetAcitve(true);
        }
    }
}