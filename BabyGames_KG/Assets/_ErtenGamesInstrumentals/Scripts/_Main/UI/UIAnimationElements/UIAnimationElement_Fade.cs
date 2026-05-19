using DG.Tweening;

using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.UIAnimationElements
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIAnimationElement_Fade : UIAnimationElement_Base
    {
        [Required]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _turkOffAlpha = 0f;
        [SerializeField] private float _turkOnAlpha = 1f;

        protected override void Awake()
        {
            base.Awake();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnDestroy()
        {
            _canvasGroup.DOKill();
            transform.DOKill();
        }

        public override void TurnOff(float? duration = null)
        {
            if (duration == null)
            {
                duration = _baseSettings.turnOffDuration;
            }

            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            if (duration.Value == 0) { _canvasGroup.alpha = _turkOffAlpha; }
            else
            {
                _canvasGroup.DOKill();

                _canvasGroup.DOFade(_turkOffAlpha, duration.Value).SetEase(_baseSettings.onEase);
            }
        }

        public override void TurnOn(float? duration = null)
        {
            if (duration == null)
            {
                duration = _baseSettings.turnOnDuration;
            }

            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            if (duration.Value == 0) { _canvasGroup.alpha = _turkOnAlpha; }
            else
            {
                _canvasGroup.DOKill();
                _canvasGroup.DOFade(_turkOnAlpha, duration.Value).SetEase(_baseSettings.onEase);
            }
        }
    }
}