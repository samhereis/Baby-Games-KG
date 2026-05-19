using Helpers;
using Hiding;
using Modes.Sorting;
using System.Threading.Tasks;
using UI.Menus;

namespace _Project._Modes.Hiding.Scripts.State
{
    public class Hiding_GameState_View
    {
        private readonly Hiding_GameState_Model _model;

        public GameplayMenu_Hiding gameplayMenu_Hiding;
        public WinMenu_Universal winMenu;

        public Hiding_GameState_View(Hiding_GameState_Model model)
        {
            _model = model;
        }

        public async Task Initialize()
        {
            winMenu = await _model.listOfAllMenus.winMenu_Universal.InstantiateAsync();

            gameplayMenu_Hiding = await _model.listOfAllMenus.gameplayMenu_Hiding.InstantiateAsync();
            gameplayMenu_Hiding.Enable();

            gameplayMenu_Hiding.backButton.onClick.AddListener(() => _model.goToMainMenuRequest?.Invoke());

            winMenu.mainMenu.onClick.AddListener(() => { _model.goToMainMenuRequest?.Invoke(); });
            winMenu.next.onClick.AddListener(() => { _model.goToNextActivityRequest?.Invoke(); });
            winMenu.replay.onClick.AddListener(() => { _model.replayRequest?.Invoke(); });

            OnWaveCompleted(_model.currentWave);
            _model.onFinish += OnFinish;

        }

        private async void OnWaveCompleted(HidingWave wave)
        {
            await AsyncHelper.DelayFloat(0.5f);

            if (_model.currentWave != null)
            {
                _model.currentWave.onCompleted += OnWaveCompleted;
                await gameplayMenu_Hiding.Initialize(_model.currentWave, _model);
            }
        }

        private async void OnFinish()
        {
            gameplayMenu_Hiding.Disable(1f);
            await AsyncHelper.DelayFloat(1f);
            winMenu?.Enable();
        }
    }
}