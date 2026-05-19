using Helpers;
using Modes.Sorting;
using System.Threading.Tasks;

namespace Modes.Puzzle
{
    public class GameplayGameState_Makeup_View
    {
        public GameplayMenu_Makeup gameplayMenu_Princess;
        public WinMenu_Universal winMenu;

        private GameplayGameState_Makeup_Model _model;

        public GameplayGameState_Makeup_View(GameplayGameState_Makeup_Model model)
        {
            _model = model;
        }

        public virtual async Task Initialize()
        {
            gameplayMenu_Princess = await _model.listOfAllMenus_SO.gameplayMenus_Makeug.InstantiateAsync();
            gameplayMenu_Princess.Construct(_model);
            gameplayMenu_Princess.Enable();

            winMenu = await _model.listOfAllMenus_SO.winMenu_Universal.InstantiateAsync();

            winMenu.mainMenu.onClick.AddListener(() => { _model.goToMainMenuRequest?.Invoke(); });
            winMenu.next.onClick.AddListener(() => { _model.goToNextActivityRequest?.Invoke(); });
            winMenu.replay.onClick.AddListener(() => { _model.replayRequest?.Invoke(); });

            _model.hasWon.AddListener(async x =>
            {
                if (_model.hasWon.value == true)
                {
                    gameplayMenu_Princess.Disable(1f);
                    await AsyncHelper.DelayFloat(1f);
                    winMenu.Enable();
                }
            });
        }
    }
}