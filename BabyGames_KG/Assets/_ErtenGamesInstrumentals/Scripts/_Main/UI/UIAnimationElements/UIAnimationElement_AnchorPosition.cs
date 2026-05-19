#if DoTweenInstalled
using DG.Tweening;
#endif

using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.UIAnimationElements
{
    public class UIAnimationElement_AnchorPosition : UIAnimationElement_Base
    {
        [Required]
        [SerializeField] private RectTransform _holder;
        [SerializeField] private Vector3 _offPosition;
        [SerializeField] private Vector3 _onPosition;

        protected override void Awake()
        {
            base.Awake();
            if (_holder == null) { _holder = GetComponent<RectTransform>(); }
        }

        private void OnDestroy()
        {
#if DoTweenInstalled
            _holder.DOKill();
#endif
        }

        [Button]
        public override void TurnOff(float? duration = null)
        {
#if DoTweenInstalled
            if (duration == null)
            {
                duration = _baseSettings.turnOffDuration;
            }

            if (duration.Value == 0)
            {
                _holder.anchoredPosition3D = _offPosition;
            }
            else
            {
                _holder.DOKill();
                _holder.DOAnchorPos3D(_offPosition, duration.Value).SetEase(_baseSettings.offEase);
            }
#endif
        }

        [Button]
        public override void TurnOn(float? duration = null)
        {
#if DoTweenInstalled
            if (duration == null)
            {
                duration = _baseSettings.turnOnDuration;
            }

            if (duration.Value == 0)
            {
                _holder.anchoredPosition3D = _onPosition;
            }
            else
            {
                _holder.DOKill();
                _holder.DOAnchorPos3D(_onPosition, duration.Value).SetEase(_baseSettings.onEase);
            }
#endif
        }
    }
}