using Helpers;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Samhereis.Helpers
{
    public sealed class ProjectHelper : MonoBehaviour
    {
        [SerializeField] private int _targetFPS = 120;

        private void Awake()
        {
            Application.targetFrameRate = _targetFPS;
        }

#if UNITY_EDITOR
        [MenuItem("Project/Delete All Data")]
        public static async void DeleAllData()
        {
            PlayerPrefs.DeleteAll();
            await DeleCache();
        }

        [MenuItem("Project/Dele Cache")]
        public static async Task DeleCache()
        {
            Caching.ClearCache();
            await TryDeleteAllPersistentDataPath();
        }

        [MenuItem("Project/OpenPersistentDataPath")]
        public static void OpenPersistentDataPath()
        {
            Process.Start(Application.persistentDataPath);
        }
#endif

        public static async Task TryDeleteAllPersistentDataPath()
        {
            await DeleteEveryFile(Application.persistentDataPath);
        }

        private static async Task DeleteEveryFile(string directory)
        {
            string[] filePaths = Directory.GetFiles(directory);
            foreach (string filePath in filePaths)
            {
                File.Delete(filePath);
                await AsyncHelper.Skip();
            }

            string[] folders = Directory.GetDirectories(directory);
            foreach (string folder in folders)
            {
                await DeleteEveryFile(folder);
                await AsyncHelper.Skip();
                Directory.Delete(folder);
            }
        }
    }
}