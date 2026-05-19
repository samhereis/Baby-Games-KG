using DG.Tweening;
using Helpers;
using RTLTMPro;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace Coloring.ForParents
{
    public class LanguageButton : MonoBehaviour
    {
        [SerializeField] private RTLTextMeshPro _textButtonBack;
        private Locale _language;
        private UnityAction<string> _unityActionClick;

        [SerializeField] private CanvasGroup _selectedState;

        public void Initialize(Locale language, UnityAction<string> unityActionClick)
        {
            _language = language;
            UpdateSelected();

            if (_language.LocaleName.Contains("Arabic") || _language.LocaleName.Contains("Hebrew")) { _textButtonBack.horizontalAlignment = HorizontalAlignmentOptions.Right; }
            else { _textButtonBack.horizontalAlignment = HorizontalAlignmentOptions.Left; }

            _textButtonBack.text = FirstCharToUpper(language.Identifier.CultureInfo.NativeName);
            _unityActionClick = unityActionClick;
            GetComponent<Button>().onClick.AddListener(OnClikButtonLanguage);

            gameObject.name = _language.LocaleName;
        }

        private void OnDestroy()
        {
            GetComponent<Button>().onClick.RemoveListener(OnClikButtonLanguage);
            _selectedState.DOKill();
        }

        private void OnClikButtonLanguage()
        {
            _unityActionClick?.Invoke(_language.Identifier.CultureInfo.NativeName);
        }

        public void UpdateSelected()
        {
            if (LocalizationSettings.SelectedLocale == _language) { _selectedState?.FadeUp(); }
            else { _selectedState?.FadeDownQuick(); }
        }

        public string FirstCharToUpper(string input)
        {
            if (String.IsNullOrEmpty(input)) { return input; }
            return input.First().ToString().ToUpper() + input.Substring(1);
        }
    }
}
