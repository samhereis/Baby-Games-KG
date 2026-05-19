using Helpers;
using System;
using System.Threading.Tasks;

namespace Modes.Sorting
{
    public class Gameplay_GameState_Orchestra_View : IDisposable
    {
        private Gameplay_GameState_Orchestra_Model _model;

        public GameplayMenu_Orchestra gameplayMenu_Orchestra { get; private set; }
        public WinMenu_Universal winMenu_Orchestra { get; private set; }

        public Gameplay_GameState_Orchestra_View(Gameplay_GameState_Orchestra_Model model)
        {
            _model = model;
        }

        public async Task Initialize()
        {
            gameplayMenu_Orchestra = await _model.listOfAllMenus.gameplayMenu_Orchestra.InstantiateAsync();
            winMenu_Orchestra = await _model.listOfAllMenus.winMenu_Universal.InstantiateAsync();

            winMenu_Orchestra.mainMenu.onClick.AddListener(() => { _model.goToMainMenuRequest?.Invoke(); });
            winMenu_Orchestra.next.onClick.AddListener(() => { _model.goToNextActivityRequest?.Invoke(); });
            winMenu_Orchestra.replay.onClick.AddListener(() => { _model.replayRequest?.Invoke(); });

            if(_model.nextActivity == null)
            {
                winMenu_Orchestra.next.gameObject.SetActive(false);
            }

            _model.onWin += async () =>
            {
                gameplayMenu_Orchestra.Disable(1f);
                await AsyncHelper.DelayFloat(1f);
                winMenu_Orchestra?.Enable();
            };

            gameplayMenu_Orchestra.Enable();
        }

        public void Dispose()
        {
        }
    }
}