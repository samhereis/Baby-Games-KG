using DG.Tweening;
using Helpers;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace UI.Popups
{
    public abstract class PopupBase_Position : PopupBase
    {
        protected static Action<PopupBase> _onAPopupOpen;

        [SerializeField] protected BaseSettings _baseSettings = new BaseSettings();

        public bool isEnabled => _baseSettings.isEnabled;

        protected virtual void Awake()
        {
            _baseSettings.Initialize();
            Disable(0);

            _onAPopupOpen += OnAPopupOpen;
        }

        protected virtual void OnDestroy()
        {
            _baseSettings.background?.DOKill();
            _onAPopupOpen -= OnAPopupOpen;
        }

        public virtual void OnAPopupOpen(PopupBase popup)
        {
            if (popup != this) Disable();
        }

        public override void Enable(float? duration = null)
        {
            if (duration == null) duration = _baseSettings.animationDuration;
            if (_baseSettings.notifyOthers == true) _onAPopupOpen?.Invoke(this);

            _baseSettings.background?.DOKill();
            _baseSettings.holder?.DOKill();

            _baseSettings.holder.anchoredPosition = _baseSettings.offPosition + _baseSettings.onDisablePositionOffset;
            if (_baseSettings.enableDisable) gameObject.SetActive(true);

            _baseSettings.background?.FadeUp(duration.Value);
            _baseSettings.holder?.DOAnchorPos3D(_baseSettings.onPosition, duration.Value);

            _baseSettings.isEnabled = true;
        }

        public override void Disable(float? duration = null)
        {
            if (duration == null) duration = _baseSettings.animationDuration;

            if (duration.Value == 0)
            {
                _baseSettings.background?.FadeDownQuick();
                if (_baseSettings.holder != null)
                {
                    _baseSettings.holder.anchoredPosition = _baseSettings.offPosition + _baseSettings.onDisablePositionOffset;
                }

                if (_baseSettings.enableDisable) gameObject.SetActive(false);
            }
            else
            {
                _baseSettings.background?.DOKill();
                _baseSettings.holder?.DOKill();

                _baseSettings.background?.FadeDown(duration.Value);

                _baseSettings.holder?.DOAnchorPos3D(_baseSettings.offPosition + _baseSettings.onDisablePositionOffset,
                    duration.Value)?.OnComplete(() =>
                {
                    if (_baseSettings.enableDisable) gameObject.SetActive(false);
                });
            }

            _baseSettings.isEnabled = false;
        }

        [Serializable]
        protected class BaseSettings
        {
            [Required] public CanvasGroup background;
            [Required] public RectTransform holder;

            [Header("Settings")]
            public bool enableDisable = true;
            public bool notifyOthers = true;
            public float animationDuration = 0.5f;
            public Vector3 onPosition = Vector3.zero;
            public Vector3 offPosition = new Vector3(0, -1500, 0);
            public Vector3 onDisablePositionOffset = Vector3.zero;

            [Header("Debug")]
            public bool isEnabled = false;

            public void Initialize()
            {
                if (holder != null) { offPosition = holder.anchoredPosition; }
            }
        }
    }
}