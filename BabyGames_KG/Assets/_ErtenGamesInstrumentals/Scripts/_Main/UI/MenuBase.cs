using DG.Tweening;

using Helpers;
using Identifiers;
using Interfaces;
using Sirenix.OdinInspector;
using System;
using System.Threading;
using UI.Helpers;
using UI.UIAnimationElements;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Menus
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    [RequireComponent(typeof(GraphicRaycaster))]
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class MenuBase : IdentifierBase, IMenuWindow, ISelfValidator
    {
        public Action onEnable;
        public Action onDisable;

        public Action onSubscribeToEvents;
        public Action onUnsubscribeFromEvents;

        [SerializeField] protected RectTransform _holder;
        [SerializeField] protected CanvasGroup _holder_CG;

        public static Action<MenuBase> onAWindowOpen { get; private set; }

        [FoldoutGroup("BaseSettings")][SerializeField] protected BaseSettings _baseSettings = new BaseSettings();

        [FoldoutGroup("BaseSettings")] public Button[] buttons;
        [FoldoutGroup("BaseSettings")] public ScaleByPercentage[] _scaleByPercentages;

        protected CancellationToken _dct;

        public virtual void Validate(SelfValidationResult result)
        {
            buttons = GetComponentsInChildren<Button>(true);
            _scaleByPercentages = GetComponentsInChildren<ScaleByPercentage>(true);

            Validate();
        }

        public void Validate()
        {
            if (_baseSettings.canvasGroup == null) { _baseSettings.canvasGroup = GetComponent<CanvasGroup>(); }
            if (_baseSettings.canvas == null) { _baseSettings.canvas = GetComponent<Canvas>(); }
            if (_holder_CG == null && _holder != null) { _holder_CG = _holder.gameObject.AddComponent<CanvasGroup>(); }

            _baseSettings.uIAnimationElements = GetComponentsInChildren<UIAnimationElement_Base>(true);
        }

        protected virtual void Awake()
        {
            _dct = destroyCancellationToken;

            Validate();

            onAWindowOpen += OnAWindowOpen;

            if (_baseSettings.disableOnStart) { TurnOff(0); }
        }

        protected virtual void OnDestroy()
        {
            UnsubscribeFromEvents();
            onAWindowOpen -= OnAWindowOpen;

            _baseSettings.canvasGroup.DOKill();
        }

        protected virtual void OnAWindowOpen(IMenuWindow uIWIndow)
        {
            if (_baseSettings.reactToOthers && uIWIndow != this as IMenuWindow)
            {
                Disable();
            }
        }

        public virtual void Enable(float? duration = null)
        {
            if (_baseSettings.isOpen == true) return;

            TurnOn(duration);

            onEnable?.Invoke();
        }

        public virtual async void Disable(float? duration = null)
        {
            if (_baseSettings.isOpen == false) return;

            if (_holder_CG != null) { await _holder_CG?.DOFade(0, 0.5f).AsyncWaitForCompletion(); }

            TurnOff(duration);

            onDisable?.Invoke();
        }

        protected void TurnOn(float? duration = null)
        {
            if (duration == null) duration = _baseSettings.animationDuration_Enable;

            _baseSettings.isOpen = true;

            _baseSettings.canvasGroup.DOKill();
            _baseSettings.canvasGroup.FadeUp(duration.Value);

            TurnOnUIAnimationElements_Async();

            if (_baseSettings.notifyOthers == true) onAWindowOpen?.Invoke(this);
        }

        protected void TurnOff(float? duration = null)
        {
            if (duration == null) duration = _baseSettings.animationDuration_Disable;

            _baseSettings.isOpen = false;

            TurnOffUIAnimationElements();

            _baseSettings.canvasGroup.DOKill();

            if (duration.Value == 0)
            {
                _baseSettings.canvasGroup.FadeDownQuick(setActiveToFalse: _baseSettings.enableDisable);
            }
            else
            {
                _baseSettings.canvasGroup.FadeDown(duration.Value, setActiveToFalse: _baseSettings.enableDisable);
            }
        }

        protected async void TurnOnUIAnimationElements_Async()
        {
            foreach (var uiAnimationElement in _baseSettings.uIAnimationElements)
            {
                if (destroyCancellationToken.IsCancellationRequested == true) break;

                try
                {
                    uiAnimationElement?.TurnOn();

                    await AsyncHelper.DelayFloat(_baseSettings.uiAnimationElementForeachDelay);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("Error animating ui animation element: " + ex, gameObject);
                }
            }
        }

        protected void TurnOffUIAnimationElements()
        {
            foreach (var uiAnimationElement in _baseSettings.uIAnimationElements)
            {
                try
                {
                    uiAnimationElement?.TurnOff();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("Error animating ui animation element: " + ex, gameObject);
                }
            }
        }

        protected virtual void SubscribeToEvents()
        {
            UnsubscribeFromEvents();
            onSubscribeToEvents?.Invoke();
        }

        protected virtual void UnsubscribeFromEvents()
        {
            onUnsubscribeFromEvents?.Invoke();
        }

        [Serializable]
        protected class BaseSettings
        {
            [Header("Settings")]
            public bool disableOnStart = true;
            public bool enableDisable = true;
            public bool notifyOthers = true;
            public bool reactToOthers = true;
            public float animationDuration_Enable = 1;
            public float animationDuration_Disable = 0.25f;
            public float uiAnimationElementForeachDelay = 0.025f;

            [Header("Components")]
            [Required] public CanvasGroup canvasGroup;
            [Required] public Canvas canvas;

            [Header("Debug")]
            public UIAnimationElement_Base[] uIAnimationElements;
            public bool isOpen = true;
        }
    }
}