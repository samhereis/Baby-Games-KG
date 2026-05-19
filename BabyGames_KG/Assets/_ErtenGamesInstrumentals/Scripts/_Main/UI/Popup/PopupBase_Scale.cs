#if DoTweenInstalled
using DG.Tweening;
using Helpers;

#endif

using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace UI.Popups
{
    public abstract class PopupBase_Scale : PopupBase
    {
        protected static Action<PopupBase> _onAPopupOpen;

        [SerializeField] protected BaseSettings _baseSettings = new BaseSettings();

        public bool isEnabled => _baseSettings.isEnabled;

        protected virtual void Awake()
        {
            Disable(0);

            _onAPopupOpen += OnAPopupOpen;
        }

        protected virtual void OnDestroy()
        {
            _onAPopupOpen -= OnAPopupOpen;
        }

        public virtual void OnAPopupOpen(PopupBase popup)
        {
            if (popup != this) Close();
        }

        public override void Enable(float? duration = null)
        {
            if (duration == null) duration = _baseSettings.animationDuration;
            if (_baseSettings.notifyOthers == true) _onAPopupOpen?.Invoke(this);


#if DoTweenInstalled
            _baseSettings.background?.DOKill();
            _baseSettings.holder?.DOKill();

            if (_baseSettings.enableDisable) gameObject.SetActive(true);

            _baseSettings.background?.FadeUp(duration.Value);
            _baseSettings.holder?.DOScale(1, duration.Value);
            
#endif

            _baseSettings.isEnabled = true;
        }

        public override void Disable(float? duration = null)
        {
#if DoTweenInstalled

            if (duration == null) duration = _baseSettings.animationDuration;

            if (duration.Value == 0)
            {
                _baseSettings.background?.FadeDownQuick();
                if (_baseSettings.holder != null) _baseSettings.holder.localScale = _baseSettings.onDisableScale;

                if (_baseSettings.enableDisable) gameObject.SetActive(false);
            }
            else
            {
                _baseSettings.background?.DOKill();
                _baseSettings.holder?.DOKill();

                _baseSettings.background?.FadeDown(duration.Value);

                _baseSettings.holder?.DOScale(_baseSettings.onDisableScale, duration.Value)?.OnComplete(() =>
                {
                    if (_baseSettings.enableDisable) gameObject.SetActive(false);
                });
            }
            
#endif
            _baseSettings.isEnabled = false;
        }

        [System.Serializable]
        protected class BaseSettings
        {
            [Required] public CanvasGroup background;
            [Required] public Transform holder;

            [Header("Settings")]
            public bool enableDisable = true;
            public bool notifyOthers = true;
            public float animationDuration = 0.5f;
            public Vector3 onDisableScale = Vector3.zero;

            [Header("Debug")]
            public bool isEnabled = false;
        }
    }
}