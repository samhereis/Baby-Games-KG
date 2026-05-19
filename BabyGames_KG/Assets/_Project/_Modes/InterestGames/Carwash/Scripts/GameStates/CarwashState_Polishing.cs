using DG.Tweening;
using FX;
using Helpers;
using Services;
using System.Threading.Tasks;
using UnityEngine;

namespace Carwash
{
    public class CarwashState_Polishing : CarwashState_Base
    {
        [SerializeField] private InstrumentUI_Polishing _instrumentUI_Polishing;

        public override async Task Enable()
        {
            await base.Enable();

            if (_instrumentUI_Polishing == null) { _instrumentUI_Polishing = _instrumentUI as InstrumentUI_Polishing; }

            _instrumentUI_Polishing.onComplete += OnComplete;
            _instrumentUI_Polishing.isActive = true;
        }

        private async void OnComplete(InstrumentUI_Base @base)
        {
            DiService.Get<StateEnd_FX>()?.DoFX();

            await Disable();

            await _canvasGroup_Complete?.DOFade(1, 0.25f).AsyncWaitForCompletion();
            await AsyncHelper.DelayFloat(1f);

            _nextState = _nextStateOnWin;
            _model.currentCarSelection.car.isFinished = true;
            _model.onCarFinished?.Invoke();
        }
    }
}