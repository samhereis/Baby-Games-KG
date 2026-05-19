using Helpers;
using Modes.Sorting;
using System.Threading.Tasks;

namespace ColorfulTrain
{
    public class ColorfulTrain_GameState_View
    {
        private readonly ColorfulTrain_GameState_Model _model;

        public GameplayMenu_ColorfulTrain gameplayMenu_Hiding;
        public WinMenu_Universal winMenu;

        public ColorfulTrain_GameState_View(ColorfulTrain_GameState_Model model)
        {
            _model = model;
        }

        public async Task Initialize()
        {
            winMenu = await _model.listOfAllMenus.winMenu_Universal.InstantiateAsync();

            gameplayMenu_Hiding = await _model.listOfAllMenus.gamgaeplayMenu_ColorfulTrain.InstantiateAsync();
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