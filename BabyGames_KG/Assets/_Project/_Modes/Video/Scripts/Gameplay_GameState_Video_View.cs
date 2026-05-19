using Assets._Project._Modes.Video.Scripts;
using System.Threading.Tasks;
using Helpers;

namespace Video
{
    public class Gameplay_GameState_Video_View
    {
        private Gameplay_GameState_Video_Model _model;

        private GameplayMenu_Video _video_Menu;

        public Gameplay_GameState_Video_View(Gameplay_GameState_Video_Model model)
        {
            _model = model;
        }

        public async Task Initialize()
        {
            _video_Menu = await _model.listOfAllMenus_SO.gameplayMenu_Video.InstantiateAsync();
            _video_Menu.Construct(_model);
            _video_Menu.Enable(0.5f);
            await AsyncHelper.DelayFloat(0.5f);
        }
    }
}