using System;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Settings;

namespace _Project.Scripts.Services
{
    public class LocalizationHelper : MonoBehaviour
    {
        public static string currentLanguage => LocalizationSettings.SelectedLocale.Identifier.CultureInfo.NativeName;

        public static async Task<string> GetAsync(string table, string entry, params object[] args)
        {
            await LocalizationSettings.InitializationOperation.Task;

            var handle = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(table, entry, args);
            try
            {
                return await handle.Task;
            } finally
            {
                Addressables.Release(handle);
            }
        }

        public static async Task PreloadTableAsync(string table)
        {
            await LocalizationSettings.InitializationOperation.Task;
            await LocalizationSettings.StringDatabase.GetTableAsync(table).Task;
        }

        public static async void Get(string table, string entry, Action<string> onReady, params object[] args)
        {
            var s = await GetAsync(table, entry, args);
            onReady?.Invoke(s);
        }

        public static async Task<string> GetTranslationAsync(string table, string entry)
        {
            var s = await GetAsync(table, entry, Array.Empty<object>());
            return s;
        }

        [Button]
        public static async Task<string> Get_Debug(string table, string entry)
        {
            var s = await GetTranslationAsync(table, entry);
            Debug.Log(s);
            return s;
        }
    }
}