using DataClasses.AssetReferences;
using RTLTMPro;
using System;
using System.Linq;
using UI.Popups;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace UI
{
    public class SettingsTab_Settings : SettingsTab_Base
    {
        [SerializeField] private ExternalAssetReference_HasComponent<LanguageMenu> _languageMenuPrefab = new();

        [SerializeField] private Button _openChangeLanguageMenuButton;
        [SerializeField] private RTLTextMeshPro _openChangeLanguageMenuText;

        private LanguageMenu _languagePopup;

        private void OnEnable()
        {
            _openChangeLanguageMenuButton.onClick.AddListener(OpenLanguageChangeMenu);
        }

        private void OnDisable()
        {
            _openChangeLanguageMenuButton.onClick.RemoveListener(OpenLanguageChangeMenu);
        }

        private void Update()
        {
            if (_openChangeLanguageMenuText == null) { return; }

            string text = FirstCharToUpper(LocalizationSettings.SelectedLocale.Identifier.CultureInfo.NativeName);
            _openChangeLanguageMenuText.text = text;
        }

        public string FirstCharToUpper(string input)
        {
            if (String.IsNullOrEmpty(input)) { return input; }

            string result = input.First().ToString().ToUpper() + input.Substring(1);
            return result;
        }

        public async void OpenLanguageChangeMenu()
        {
            _languagePopup = await _languageMenuPrefab.InstantiateAsync();
            if (_languagePopup == null) { return; }

            _languagePopup.Initialize();
            _languagePopup.Enable();
        }
    }
}