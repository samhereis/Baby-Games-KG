using RTLTMPro;
using Sirenix.OdinInspector;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;

namespace UI
{
    [DisallowMultipleComponent]
    public class LocalizationOverrider : MonoBehaviour, ISelfValidator
    {
        [SerializeField] private bool _enableLocalization = true;
        [SerializeField] private string _languageOverride = "English (en)";
        [SerializeField] private LocalizeStringEvent[] _localizeStrings;
        [SerializeField] private TextMeshProUGUI[] _texts;

        public void Validate(SelfValidationResult result)
        {
            _localizeStrings = GetComponentsInChildren<LocalizeStringEvent>(true);
            _texts = GetComponentsInChildren<TextMeshProUGUI>(true).Where(x => x is not RTLTextMeshPro && x.GetComponent<LocalizeStringEvent>() != null).ToArray();
        }

#if UNITY_EDITOR
        [Button]
        private void AddArabicSupport()
        {
            foreach (var item in _texts)
            {
                var itemGameobject = item.gameObject;

                var fontAsset = item.font;
                var fontStyle = item.fontStyle;
                var enableAutoSizing = item.enableAutoSizing;
                var fontSizeMin = item.fontSizeMin;
                var fontSizeMax = item.fontSizeMax;
                var fontSize = item.fontSize;
                var color = item.color;
                var alignment = item.alignment;
                var enableWordWrapping = item.textWrappingMode;
                var overflowMode = item.overflowMode;
                var text = item.text;

                DestroyImmediate(item);

                var newText = itemGameobject.AddComponent<RTLTextMeshPro>();

                newText.font = fontAsset;
                newText.fontStyle = fontStyle;
                newText.enableAutoSizing = enableAutoSizing;
                newText.fontSizeMin = fontSizeMin;
                newText.fontSizeMax = fontSizeMax;
                newText.fontSize = fontSize;
                newText.color = color;
                newText.alignment = alignment;
                newText.textWrappingMode = enableWordWrapping;
                newText.overflowMode = overflowMode;
                newText.text = text;

                var stringEvent = itemGameobject.GetComponent<LocalizeStringEvent>();

                var targetinfo = UnityEvent.GetValidMethodInfo(newText, "set_text", new Type[] { typeof(string) });
                UnityAction<string> action = Delegate.CreateDelegate(typeof(UnityAction<string>), newText, targetinfo, false) as UnityAction<string>;
                UnityEditor.Events.UnityEventTools.AddStringPersistentListener(stringEvent.OnUpdateString, action, newText.text);
            }

            foreach (var item in _localizeStrings)
            {
                if (item.TryGetComponent<RightLeftTextHelper>(out var rightLeftTextHelper)) { rightLeftTextHelper.Validate(null); continue; }
                var textItem = item.gameObject.AddComponent<RightLeftTextHelper>();
            }
        }

        [Button]
        private async void SetLocalization()
        {
            foreach (var localizeString in _localizeStrings)
            {
                localizeString.enabled = _enableLocalization;

                if (_enableLocalization)
                {
                    localizeString.StringReference.Arguments?.Clear();
                    localizeString.StringReference.LocaleOverride = null;
                }
                else
                {
                    localizeString.StringReference.Arguments?.Clear();
                    localizeString.StringReference.LocaleOverride = LocalizationSettings.AvailableLocales.GetLocale(_languageOverride);
                }

                var hangle = localizeString.StringReference.GetLocalizedStringAsync(_languageOverride);
                await hangle.Task;

                if (localizeString.TryGetComponent<TextMeshProUGUI>(out var textMeshPro))
                {
                    textMeshPro.text = hangle.Result;
                }

                if (localizeString.TryGetComponent<RTLTextMeshPro>(out var RTLTextMeshPro))
                {
                    RTLTextMeshPro.OriginalText = hangle.Result;
                    RTLTextMeshPro.text = hangle.Result;
                }
            }
        }
#endif
    }
}