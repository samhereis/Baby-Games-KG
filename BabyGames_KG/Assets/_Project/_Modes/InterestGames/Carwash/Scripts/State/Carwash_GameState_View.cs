using Assets._Project._Modes.Carwash.Scripts.State;
using DG.Tweening;
using Helpers;
using Modes.Sorting;
using System.Threading.Tasks;
using UnityEngine;

namespace Carwash
{
    public class Carwash_GameState_View
    {
        public GameplayMenu_Carwash gameplayMenu;
        private WinMenu_Universal _winMenu;

        private Carwash_GameState_Model _model;

        public Carwash_GameState_View(Carwash_GameState_Model model)
        {
            _model = model;
        }

        public async Task Initialize()
        {
            gameplayMenu = await _model.listOfAllMenus.gameplayMenu_Carwash.InstantiateAsync();
            gameplayMenu.Construct(_model);
            gameplayMenu.Enable();
            gameplayMenu.backButton.onClick.AddListener(() => { _model.goToMainMenuRequest?.Invoke(); });

            _winMenu = await _model.listOfAllMenus.winMenu_Universal.InstantiateAsync();
            _winMenu.mainMenu.onClick.AddListener(() => { _model.goToMainMenuRequest?.Invoke(); });
            _winMenu.next.onClick.AddListener(() => { _model.goToNextActivityRequest?.Invoke(); });
            _winMenu.replay.onClick.AddListener(() => { _model.replayRequest?.Invoke(); });

            _model.onFinish += OnFinish;
        }

        private async void OnFinish()
        {
            gameplayMenu.Disable(1f);
            await AsyncHelper.DelayFloat(2f);
            _winMenu.Enable();
        }
    }
}