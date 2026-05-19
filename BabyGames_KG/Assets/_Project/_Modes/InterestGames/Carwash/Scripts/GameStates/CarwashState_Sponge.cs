using DG.Tweening;
using FX;
using Services;
using UnityEngine;

namespace Carwash
{
    public class CarwashState_Sponge : CarwashState_Base
    {
        [SerializeField] private InstrumentUI_Sponge _instrumentUI_Sponge;

        private void OnEnable()
        {
            if (_instrumentUI_Sponge == null) { _instrumentUI_Sponge = _instrumentUI as InstrumentUI_Sponge; }
            _instrumentUI_Sponge.onComplete += OnComplete;
        }

        private void OnComplete(InstrumentUI_Base @base)
        {
            _nextState = _nextStateOnWin;
            _car.dirt.DOFade(0, 1f);
            _canvasGroup_Complete.DOFade(1, 1);
        }
    }
}