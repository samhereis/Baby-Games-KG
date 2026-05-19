using DG.Tweening;
using UnityEngine;

namespace Carwash
{
    public class CarwashState_Pena : CarwashState_Base
    {
        [SerializeField] private InstrumentUI_Pena _instrumentUI_Pena;

        private void OnEnable()
        {
            if (_instrumentUI_Pena == null) { _instrumentUI_Pena = _instrumentUI as InstrumentUI_Pena; }
            _instrumentUI_Pena.onComplete += OnComplete;
        }

        private void OnComplete(InstrumentUI_Base @base)
        {
            _nextState = _nextStateOnWin;
            _canvasGroup_Complete.DOFade(1, 1);
        }
    }
}