using Assets._Project._Modes.Carwash.Scripts.State;
using DG.Tweening;
using FX;
using Modes.Puzzle;
using Services;
using System.Threading.Tasks;
using UnityEngine;

namespace Carwash
{
    public class CarwashState_Base : StateMachine_StateBase
    {
        protected CarwashCar_Identifier _car => _model.currentCarSelection.car;

        [SerializeField] protected CanvasGroup _canvasGroup;
        [SerializeField] protected CanvasGroup _canvasGroup_Active;
        [SerializeField] protected CanvasGroup _canvasGroup_Inactive;
        [SerializeField] protected CanvasGroup _canvasGroup_Complete;
        [SerializeField] protected InstrumentUI_Base _instrumentUI;
        [SerializeField] protected HintHand_DrawSimple _hintHand_Draw;

        protected Carwash_GameState_Model _model;

        private void Validate()
        {
            if (_canvasGroup == null) { _canvasGroup = GetComponent<CanvasGroup>(); }
            if (_instrumentUI == null) { _instrumentUI = GetComponent<InstrumentUI_Base>(); }
        }

        public void Construct(Carwash_GameState_Model model)
        {
            _model = model;

            _model.onCarEntered += async (x) =>
            {
                _canvasGroup_Complete.alpha = 0;
                await Disable();
            };
        }

        private void OnDestroy()
        {
            _canvasGroup_Active.DOKill();
            _canvasGroup_Inactive.DOKill();
        }

        public override async Task Enable()
        {
            Validate();
            _instrumentUI.Construct(_model);

            _instrumentUI.enabled = true;
            _canvasGroup.interactable = true;
            _canvasGroup_Active.DOFade(1f, 0.25f);
            await _canvasGroup_Inactive.DOFade(0f, 0.25f).AsyncWaitForCompletion();
        }

        public override async Task Enter()
        {
            await base.Enter();

            _hintHand_Draw.SetIsActive(true);
            _hintHand_Draw.sources.Clear();
            _hintHand_Draw.sources.Add(_canvasGroup.GetComponent<RectTransform>());
        }

        public override async Task Disable()
        {
            Validate();

            _instrumentUI.enabled = false;
            _instrumentUI.PutBack();
            _canvasGroup.interactable = false;
            await _canvasGroup_Inactive.DOFade(1f, 0.25f).AsyncWaitForCompletion();
        }

        public override async Task Exit()
        {
            await base.Exit();
            _hintHand_Draw.SetIsActive(false);
        }
    }
}