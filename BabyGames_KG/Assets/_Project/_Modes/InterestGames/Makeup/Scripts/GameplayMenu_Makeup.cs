using UI.Menus;
using UnityEngine.UI;

namespace Modes.Puzzle
{
    public class GameplayMenu_Makeup : MenuBase
    {
        public Button backButton;
        public CompleteButton completeButton;

        private GameplayGameState_Makeup_Model _model;

        public void Construct(GameplayGameState_Makeup_Model model)
        {
            _model = model;
        }

        public override void Enable(float? duration = null)
        {
            completeButton.SetAcitve(false);

            base.Enable(duration);

            completeButton?.onClick.RemoveListener(CompleteButtonClicked);
            completeButton?.onClick.AddListener(CompleteButtonClicked);

            _model.completeButtonShowRequested -= CompleteButtonShowRequested;
            _model.completeButtonShowRequested += CompleteButtonShowRequested;
        }

        public override void Disable(float? duration = null)
        {
            base.Disable(duration);

            completeButton?.onClick.RemoveListener(CompleteButtonClicked);
            _model.completeButtonShowRequested -= CompleteButtonShowRequested;
        }

        private void CompleteButtonClicked()
        {
            _model.onCompleteButtonClicked?.Invoke();
            completeButton.SetAcitve(false);
        }

        private void CompleteButtonShowRequested()
        {
            completeButton.SetAcitve(true);
        }
    }
}