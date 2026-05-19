using DG.Tweening;
using Helpers;
using Sirenix.OdinInspector;
using System;
using UI.UIAnimationElements;
using UnityEngine;

namespace UI.Message
{
    public class BasicMessage : MonoBehaviour
    {
        public enum EffectOnShow { None, Vibrate }

        [SerializeField] private UIAnimationElement_Base _animation;
        [SerializeField] private CanvasGroup _background;
        [SerializeField] private RectTransform _holder;

        [Space]
        [SerializeField] private float _animationDuration = 1;
        [SerializeField] private float _lifetime = 15f;

        [SerializeField] private EffectOnShow effectOnShow = EffectOnShow.None;

        private Action _onHide;

        protected virtual void Awake()
        {
            _animation.TurnOff();
            _background.alpha = 0;
        }

        private void OnDestroy()
        {
            _background.DOKill();
        }

        [Button]
        public virtual async void Show(Action onHide)
        {
            _onHide = onHide;

            _background?.DOFade(1, _animationDuration);
            _animation.TurnOn(0.5f);

            switch (effectOnShow)
            {
                case EffectOnShow.None:
                    {
                        break;
                    }
                case EffectOnShow.Vibrate:
                    {
                        await AsyncHelper.DelayFloat(0.5f);
                        Vibrate();
                        break;
                    }
            }

            await AsyncHelper.DelayFloat(_lifetime);
            if (destroyCancellationToken.IsCancellationRequested == false)
            {
                Hide();
            }
        }

        [Button]
        public virtual async void Hide()
        {
            _background?.DOFade(0, _animationDuration);
            _animation.TurnOff();

            await AsyncHelper.DelayFloat(_animationDuration);
            _onHide?.Invoke();
        }

        [Button]
        private void Vibrate()
        {
            _holder.DOShakePosition(1f, 10f, 10);
        }
    }
}