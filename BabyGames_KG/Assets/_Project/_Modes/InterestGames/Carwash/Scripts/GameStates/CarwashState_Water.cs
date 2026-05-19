using DG.Tweening;
using FX;
using Services;
using System.Threading.Tasks;
using UI_Spline_Renderer;
using UnityEngine;

namespace Carwash
{
    public class CarwashState_Water : CarwashState_Base
    {
        [SerializeField] private InstrumentUI_Water _instrumentUI_Water;
        [SerializeField] private UISplineRenderer _splineImage;

        [Header("Materials")]
        [SerializeField] private Material _materialActive;
        [SerializeField] private Material _materialDisable;

        public override async Task Enable()
        {
            await base.Enable();

            if (_instrumentUI_Water == null) { _instrumentUI_Water = _instrumentUI as InstrumentUI_Water; }

            _instrumentUI_Water.onComplete += OnComplete;
        }

        public override async Task Enter()
        {
            _splineImage.material = _materialActive;
            await base.Enter();
        }

        public override async Task Disable()
        {
            _splineImage.material = _materialDisable;
            await base.Disable();
        }

        private void OnComplete(InstrumentUI_Base @base)
        {
            _nextState = _nextStateOnWin;
            _canvasGroup_Complete.DOFade(1, 1);
        }
    }
}