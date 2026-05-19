using Helpers;
using Modes.Sorting;
using System.Threading.Tasks;
using UI.Menus;

namespace Modes.Puzzle
{
    public class Gameplay_GameState_Puzzle_View
    {
        public GameplayMenu_Puzzle gameplayMenu_Puzzle;
        public WinMenu_Universal winMenu;

        private Gameplay_GameState_Puzzle_Model _model;

        public Gameplay_GameState_Puzzle_View(Gameplay_GameState_Puzzle_Model model)
        {
            _model = model;
        }

        public virtual async Task Initialize()
        {
            gameplayMenu_Puzzle = await _model.listOfAllMenus_SO.gameplayMenu_Puzzle.InstantiateAsync();
            winMenu = await _model.listOfAllMenus_SO.winMenu_Universal.InstantiateAsync();
            gameplayMenu_Puzzle.Enable();

            winMenu.mainMenu.onClick.AddListener(() => { _model.goToMainMenuRequest?.Invoke(); });
            winMenu.next.onClick.AddListener(() => { _model.goToNextActivityRequest?.Invoke(); });
            winMenu.replay.onClick.AddListener(() => { _model.replayRequest?.Invoke(); });

            _model.hasWon.AddListener(async x =>
            {
                if (_model.hasWon.value == true)
                {
                    gameplayMenu_Puzzle.Disable(1f);
                    await AsyncHelper.DelayFloat(1f);
                    winMenu.Enable();
                }
            });
        }

        public async Task Dispose()
        {
            await AsyncHelper.NextFrame();
        }
    }
}