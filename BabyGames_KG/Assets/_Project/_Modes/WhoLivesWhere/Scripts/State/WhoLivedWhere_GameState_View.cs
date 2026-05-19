using Carwash;
using Helpers;
using Modes.Sorting;
using System.Threading.Tasks;

namespace _Project._Modes.Hiding.Scripts.State
{
    public class WhoLivedWhere_GameState_View
    {
        private readonly WhoLivedWhere_GameState_Model _model;

        public GameplayMenu_WhoLivesWhere gameplayMenu_Hiding;
        public WinMenu_Universal winMenu;

        public WhoLivedWhere_GameState_View(WhoLivedWhere_GameState_Model model)
        {
            _model = model;
        }

        public async Task Initialize()
        {
            winMenu = await _model.listOfAllMenus.winMenu_Universal.InstantiateAsync();

            gameplayMenu_Hiding = await _model.listOfAllMenus.gamgaeplayMenu_WhoLivesWhere.InstantiateAsync();
            gameplayMenu_Hiding.Enable();

            gameplayMenu_Hiding.backButton.onClick.AddListener(() => _model.goToMainMenuRequest?.Invoke());

            winMenu.mainMenu.onClick.AddListener(() => { _model.goToMainMenuRequest?.Invoke(); });
            winMenu.next.onClick.AddListener(() => { _model.goToNextActivityRequest?.Invoke(); });
            winMenu.replay.onClick.AddListener(() => { _model.replayRequest?.Invoke(); });

            _model.onFinish += OnFinish;
        }

        private async void OnFinish()
        {
            gameplayMenu_Hiding.Disable(1f);
            await AsyncHelper.DelayFloat(1f);
            winMenu?.Enable();
        }
    }
}