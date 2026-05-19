#if DoTweenInstalled
using DG.Tweening;

#endif

using Interfaces;
using Sirenix.OdinInspector;
using System;
using UI.Menus;
using UnityEngine;
using Helpers;

namespace UI.Page
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class MenuPageBase : MonoBehaviour, IMenuWindow
    {
        [SerializeField] protected WindowPageBaseSettings _baseSettings = new WindowPageBaseSettings();

        public virtual void Enable(float? duration = null)
        {
            if (duration == null) duration = _baseSettings.animationDuration;

#if DoTweenInstalled
            _baseSettings.canvasGroup.DOKill();
            if (_baseSettings.enableDisable) gameObject.SetActive(true);
            _baseSettings.canvasGroup.FadeUp(duration.Value);
#endif
        }

        public virtual void Disable(float? duration = null)
        {
#if DoTweenInstalled
            if (duration == null) duration = _baseSettings.animationDuration;

            if (duration.Value == 0)
            {
                _baseSettings.canvasGroup.FadeDownQuick();
                if (_baseSettings.enableDisable) gameObject.SetActive(false);
            }
            else
            {
                _baseSettings.canvasGroup.FadeDown(duration.Value, completeCallback: () =>
                {
                    if (_baseSettings.enableDisable) gameObject.SetActive(false);
                });
            }
#endif
        }

        [ContextMenu(nameof(Setup))]
        public void Setup()
        {
            if (_baseSettings.canvasGroup == null) _baseSettings.canvasGroup = GetComponent<CanvasGroup>();
            if (_baseSettings.parent == null) _baseSettings.parent = GetComponentInParent<MenuBase>(true);
        }

        [Serializable]
        protected class WindowPageBaseSettings
        {
            [Required] public MenuBase parent;
            [Required] public CanvasGroup canvasGroup;
            public float animationDuration = 0.5f;
            public bool enableDisable = true;
        }
    }
}
