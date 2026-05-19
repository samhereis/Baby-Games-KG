using Helpers;
using Modes.Sorting;
using System.Threading.Tasks;

namespace CoockingSalade
{
    public class CoockingSalade_GameState_View
    {

        public GameplayMenu_CoockingSalade gameplayMenu_Hiding;
        public WinMenu_Universal winMenu;

        private readonly Coocking_GameState_Model _model;
        public CoockingSalade_GameState_View(Coocking_GameState_Model model)
        {
            _model = model;
        }

        public async Task Initialize()
        {
            winMenu = await _model.listOfAllMenus.winMenu_Universal.InstantiateAsync();

            gameplayMenu_Hiding = await _model.listOfAllMenus.gamgaeplayMenu_CoockingSalade.InstantiateAsync();
            gameplayMenu_Hiding.Construct(_model);
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