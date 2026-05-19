using Bubble;
using Helpers;
using Modes.Sorting;
using System.Threading.Tasks;
using UnityEngine;

namespace _Project._Modes.Hiding.Scripts.State
{
    public class Bubble_GameState_View
    {
        private Bubble_GameState_Model _model;

        private GameplayMenu_Bubble _gameplayMenu;
        private WinMenu_Universal _winMenu;

        public Bubble_GameState_View(Bubble_GameState_Model model)
        {
            _model = model;
        }

        public async Task Initialize()
        {
            _gameplayMenu = await _model.listOfAllMenus.gameplayMenu_Bubble.InstantiateAsync();
            _winMenu = await _model.listOfAllMenus.winMenu_Universal.InstantiateAsync();

            _gameplayMenu.Enable();

            _gameplayMenu.backButton.onClick.AddListener(() => { _model.goToMainMenuRequest?.Invoke(); });
            _model.bubbleController.hasWon.AddListener(async x =>
            {
                if (_model.bubbleController.hasWon.value == true)
                {
                    _gameplayMenu.Deactivcate();
                    await AsyncHelper.DelayFloat(2f);
                    _winMenu.Enable();
                }
            });

            _winMenu.mainMenu.onClick.AddListener(() => { _model.goToMainMenuRequest?.Invoke(); });
            _winMenu.next.onClick.AddListener(() => { _model.goToNextActivityRequest?.Invoke(); });
            _winMenu.replay.onClick.AddListener(() => { _model.replayRequest?.Invoke(); });
        }
    }
}