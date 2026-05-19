#if LocalizationInstalled

using Helpers;
using Interfaces;
using System;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace UI
{
    public class LanguageManager : IInitializable
    {
        private LanguageManager()
        {
            Initialize();
        }

        public async void Initialize()
        {
            while (LocalizationSettings.InitializationOperation.IsDone == false) await AsyncHelper.Skip();

            if (PlayerPrefs.HasKey("Language"))
            {
                UpdateLocale();
            }
            else
            {
                ChangeLanguage(2);
            }
        }

        public void ChangeLanguage(int index)
        {
            PlayerPrefs.SetInt("Language", index);
            PlayerPrefs.Save();

            UpdateLocale();
        }

        private void UpdateLocale()
        {
            try { LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[PlayerPrefs.GetInt("Language")]; }
            catch (Exception ex) { Debug.LogWarning(ex); }
        }
    }
}

#endif