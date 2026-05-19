using UI.Menus;
using UnityEngine;
using UnityEngine.UI;

namespace CoockingSalade
{
    public class GameplayMenu_CoockingSalade : MenuBase
    {
        public Button backButton;
        public CompleteButton complete;
        public RectTransform content;

        private Coocking_GameState_Model _model;

        public void Construct(Coocking_GameState_Model model)
        {
            _model = model;
        }

        public override void Enable(float? duration = null)
        {
            base.Enable(duration);

            CompleteButton.instance.onClick.AddListener(Complete);

            content.gameObject.SetActive(false);

            _model.requestCompleteButtonHide += OnShowCompleteButtonHideRequested;
            _model.requestCompleteButtonShow += OnShowCompleteButtonShowRequested;

            complete.SetAcitve(false);
        }

        public override void Disable(float? duration = null)
        {
            base.Disable(duration);

            CompleteButton.instance.onClick.RemoveListener(Complete);

            _model.requestCompleteButtonHide -= OnShowCompleteButtonHideRequested;
            _model.requestCompleteButtonShow -= OnShowCompleteButtonShowRequested;
        }

        private void OnShowCompleteButtonShowRequested()
        {
            complete.SetAcitve(true);
        }

        private void OnShowCompleteButtonHideRequested()
        {
            complete.SetAcitve(false);
        }

        private void Complete()
        {
            complete.SetAcitve(false);
            _model.onCompleteButtonPressed?.Invoke();
        }
    }
}