using Coloring.ForParents;
using Helpers;
using System.Collections.Generic;
using System.Linq;
using UI.Menus;
using UI.UIAnimationElements;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace UI.Popups
{
    public class LanguageMenu : MenuBase
    {
        [SerializeField] private Transform _languagesHolder;
        [SerializeField] private LanguageButton _languageButton;
        [SerializeField] private UIAnimationElement_Base _windowAnimation;

        private List<LanguageButton> _spawnedLanguageButtons = new List<LanguageButton>();

        public async void Initialize()
        {
            foreach (var languageButton in _languagesHolder.GetComponentsInChildren<LanguageButton>(true))
            {
                Destroy(languageButton.gameObject);
            }

            while (LocalizationSettings.InitializationOperation.IsDone == false) await AsyncHelper.Skip();
            var locales = LocalizationSettings.AvailableLocales.Locales;
            locales = locales.OrderBy(x => x.Identifier.CultureInfo.NativeName).ToList();

            _spawnedLanguageButtons.Clear();
            foreach (var language in locales)
            {
                var languageButton = Instantiate(_languageButton, _languagesHolder);
                languageButton.Initialize(language, (language) =>
                {
                    ChangeLanguage(language);
                });

                _spawnedLanguageButtons.Add(languageButton);
            }
        }

        public override void Enable(float? duration = null)
        {
            base.Enable(duration);
            _windowAnimation.TurnOn();
        }

        public void ChangeLanguage(string lang)
        {
            PlayerPrefs.SetString("Language", lang);
            PlayerPrefs.Save();

            Apply();
        }

        public void Apply()
        {
            UpdateLocale();

            _windowAnimation.TurnOff();
            Disable();

            DontDestroyOnLoad(gameObject);
            Destroy(gameObject, 1);
        }

        public static void SetStartupLanguage()
        {
            if (PlayerPrefs.HasKey("Language") == false)
            {
                PlayerPrefs.SetString("Language", "кыргызча");
                PlayerPrefs.Save();
            }

            UpdateLocale();
        }

        public static void UpdateLocale()
        {
            foreach (var item in LocalizationSettings.AvailableLocales.Locales)
            {
                bool foundLanguage = item.Identifier.CultureInfo.NativeName == PlayerPrefs.GetString("Language");
                if (foundLanguage)
                {
                    LocalizationSettings.SelectedLocale = item;
                }
            }
        }
    }
}