using DG.Tweening;
using PaintCore;
using Services;
using UI.Menus;
using UI.UIAnimationElements;
using UnityEngine;
using UnityEngine.UI;

namespace Modes.Coloring
{
    public class GameplayMenu_Coloring : MenuBase
    {
        [field: SerializeField] public ClearButton clearButton;
        [field: SerializeField] public Button buttonBack { get; private set; }
        [field: SerializeField] public Button saveDrawingbutton { get; private set; }
        [field: SerializeField] public Button undoButton { get; private set; }
        [field: SerializeField] public Button playAnimation { get; private set; }
        [field: SerializeField] public Pallete pallite { get; private set; }
        [field: SerializeField] public HintHand hintHand { get; private set; }
        [field: SerializeField] public Image _curtain { get; private set; }

        [Space]
        [SerializeField] private UIAnimationElement_Base _instrumentsPanel;

        protected override void Awake()
        {
            base.Awake();
            DiService.Inject(this);
        }

        private void Update()
        {
            undoButton.interactable = CwStateManager.CanUndo;
        }

        public override void Enable(float? duration = null)
        {
            base.Enable(duration);
            Activate();

            _baseSettings.canvas.renderMode = RenderMode.ScreenSpaceCamera;
            _baseSettings.canvas.worldCamera = Camera.main;
            _baseSettings.canvas.planeDistance = 1;
        }

        public override void Disable(float? duration = null)
        {
            base.Disable(duration);
            Deactivate();
        }

        public void Deactivate()
        {
            saveDrawingbutton.interactable = false;
            playAnimation.interactable = false;
            undoButton.interactable = false;

            saveDrawingbutton.transform.DOScale(0, 0.25f);
            playAnimation.transform.DOScale(0, 0.25f);
            undoButton.transform.DOScale(0, 0.25f);

            _instrumentsPanel.TurnOff();
        }

        public void Activate()
        {
            saveDrawingbutton.interactable = true;
            playAnimation.interactable = true;
            undoButton.interactable = true;

            saveDrawingbutton.transform.DOScale(1, 0.25f);
            playAnimation.transform.DOScale(1, 0.25f);
            undoButton.transform.DOScale(1, 0.25f);

            _instrumentsPanel.TurnOn();
        }
    }
}