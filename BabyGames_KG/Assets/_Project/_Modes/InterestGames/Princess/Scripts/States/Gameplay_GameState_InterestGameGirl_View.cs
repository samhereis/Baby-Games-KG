using Helpers;
using Modes.Sorting;
using System.Threading.Tasks;

namespace Modes.Puzzle
{
    public class Gameplay_GameState_InterestGameGirl_View
    {
        public GameplayMenu_Princess gameplayMenu_Princess;
        public WinMenu_Universal winMenu;

        private Gameplay_GameState_InterestGameGirl_Model _model;

        public Gameplay_GameState_InterestGameGirl_View(Gameplay_GameState_InterestGameGirl_Model model)
        {
            _model = model;
        }

        public virtual async Task Initialize()
        {
            gameplayMenu_Princess = await _model.listOfAllMenus_SO.gameplayMenu_Princess.InstantiateAsync();
            gameplayMenu_Princess.Construct(_model);

            winMenu = await _model.listOfAllMenus_SO.winMenu_Universal.InstantiateAsync();
            gameplayMenu_Princess.Enable();

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