using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace EditorHelper
{
    public class FileSearcher : MonoBehaviour
    {
        [FolderPath] public string folderPath;

        public string fileExtension;
        public List<string> foundFiles = new List<string>();
        public bool _isBusy = false;

        [Button]
        private async void SearchFiles()
        {
            foundFiles.Clear();

            if (_isBusy == true) { return; }
            _isBusy = true;

            await SearchFiles(folderPath, fileExtension);

            _isBusy = false;
        }

        private async Task SearchFiles(string folder, string extension)
        {
            try
            {
                await Task.Run(async () =>
                {
                    foreach (string file in Directory.GetFiles(folder))
                    {
                        string currentExtention = Path.GetExtension(file);
                        if (currentExtention == extension)
                        {
                            File.Delete(file);
                            foundFiles.Add(file);
                        }
                    }

                    foreach (string subFolder in Directory.GetDirectories(folder))
                    {
                        await SearchFiles(subFolder, extension);
                    }
                });
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error searching files: {e.Message}");
            }
        }
    }
}