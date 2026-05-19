using DG.Tweening;
using Helpers;

using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace UI.Popups
{
    public abstract class PopupBase_Fade : PopupBase
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
            _baseSettings?.canvasGroup?.DOKill();
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

            _baseSettings.canvasGroup?.DOKill();

            if (_baseSettings.enableDisable) gameObject.SetActive(true);

            _baseSettings.canvasGroup?.FadeUp(duration.Value);

            _baseSettings.isEnabled = true;
        }

        public override void Disable(float? duration = null)
        {
            if (duration == null) duration = _baseSettings.animationDuration;

            if (duration.Value == 0)
            {
                _baseSettings.canvasGroup?.FadeDownQuick();
                if (_baseSettings.enableDisable) gameObject.SetActive(false);
            }
            else
            {
                _baseSettings.canvasGroup?.DOKill();

                _baseSettings.canvasGroup?.FadeDown(duration.Value)?.OnComplete(() =>
                {
                    if (_baseSettings.enableDisable) gameObject.SetActive(false);
                });
            }

            _baseSettings.isEnabled = false;
        }

        [System.Serializable]
        protected class BaseSettings
        {
            [Required] public CanvasGroup canvasGroup;

            [Header("Settings")]
            public bool enableDisable = true;
            public bool notifyOthers = true;
            public float animationDuration = 0.5f;

            [Header("Debug")]
            public bool isEnabled = false;
        }
    }
}