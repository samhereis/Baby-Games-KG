using GameState;
using Identifiers;

namespace UI
{
    public class SettingsTab_Base : IdentifierBase
    {
        protected MainMenu_GameState_Model _model;

        public void Construct(MainMenu_GameState_Model model)
        {
            _model = model;
        }
    }
}