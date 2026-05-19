using DataClasses;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

#if NewtonsoftInstalled
using Newtonsoft.Json;
#endif

namespace Helpers
{
    public class SaveHelper
    {
        public static async
#if UNITY_2023_2_OR_NEWER
            Awaitable
#else
            Task
#endif
            SaveToJsonAsync<T>(T objectToSave, string folder, string fileName) where T : ISavable
        {
#if UNITY_2023_2_OR_NEWER
            await Awaitable.BackgroundThreadAsync();
#endif

            if (string.IsNullOrEmpty(folder) == false && string.IsNullOrEmpty(folder)) folder = "_";
            if (string.IsNullOrEmpty(fileName) == false && string.IsNullOrEmpty(fileName)) throw new System.ArgumentNullException(nameof(fileName), "file name is null");

            string path = GetFullFolderName(folder) + "/";

            if (Directory.Exists(path) == false) Directory.CreateDirectory(path);

            FileStream fileSteam = new FileStream(path + fileName + ".json", FileMode.Create);

            string json = string.Empty;

#if NewtonsoftInstalled
            json = JsonConvert.SerializeObject(objectToSave);
#endif

            using (StreamWriter writer = new StreamWriter(fileSteam))
            {
                Debug.Log("Saving: " + json);
                await writer.WriteAsync(json);
            }

#if UNITY_2023_2_OR_NEWER
            await Awaitable.MainThreadAsync();
#endif
        }

        public static void SaveToJson<T>(T objectToSave, string folder, string fileName) where T : ISavable
        {
            if (string.IsNullOrEmpty(folder) == false && string.IsNullOrEmpty(folder)) folder = "_";
            if (string.IsNullOrEmpty(fileName) == false && string.IsNullOrEmpty(fileName)) throw new System.ArgumentNullException(nameof(fileName), "file name is null");

            string path = GetFullFolderName(folder) + "/";

            if (Directory.Exists(path) == false) Directory.CreateDirectory(path);

            FileStream fileSteam = new FileStream(path + fileName + ".json", FileMode.Create);

            string json = string.Empty;

#if NewtonsoftInstalled
            json = JsonConvert.SerializeObject(objectToSave);
#endif

            using (StreamWriter writer = new StreamWriter(fileSteam))
            {
                Debug.Log("Saving: " + json);
                writer.Write(json);
            }

        }

        public async static
#if UNITY_2023_2_OR_NEWER
            Awaitable<T>
#else
            Task<T>
#endif
            GetStoredDataClassAsync<T>(string folder, string fileName) where T : ISavable
        {
#if UNITY_2023_2_OR_NEWER
            await Awaitable.BackgroundThreadAsync();
#endif

            if (string.IsNullOrEmpty(folder) == false && string.IsNullOrEmpty(folder)) folder = "_";
            if (string.IsNullOrEmpty(fileName) == false && string.IsNullOrEmpty(fileName)) throw new System.ArgumentNullException(nameof(fileName), "file name is null");

            string file = GetFullFolderName(folder) + "/" + fileName + ".json";

            T result = default(T);

            if (File.Exists(file))
            {
                using (StreamReader reader = new StreamReader(file))
                {
#if NewtonsoftInstalled
                    result = JsonConvert.DeserializeObject<T>(await reader.ReadToEndAsync());

                    Debug.Log("Got: " + JsonConvert.SerializeObject(result));
#endif
                }
            }

#if UNITY_2023_2_OR_NEWER
            await Awaitable.MainThreadAsync();
#else
            await AsyncHelper.Skip();
#endif

            return result;
        }

        public static T GetStoredDataClass<T>(string folder, string fileName) where T : ISavable
        {
            if (string.IsNullOrEmpty(folder) == false && string.IsNullOrEmpty(folder)) folder = "_";
            if (string.IsNullOrEmpty(fileName) == false && string.IsNullOrEmpty(fileName)) throw new System.ArgumentNullException(nameof(fileName), "file name is null");

            string file = GetFullFolderName(folder) + "/" + fileName + ".json";

            T result = default(T);

            if (File.Exists(file))
            {
                using (StreamReader reader = new StreamReader(file))
                {
#if NewtonsoftInstalled
                    result = JsonConvert.DeserializeObject<T>(reader.ReadToEnd());

                    Debug.Log("Got: " + JsonConvert.SerializeObject(result));
#endif
                }
            }

            return result;
        }

        public static string ToJson<T>(T obj)
        {
            string json = string.Empty;

#if NewtonsoftInstalled
            json = JsonConvert.SerializeObject(obj);
#endif

            return json;
        }

        public static T FromJson<T>(string obj)
        {
            T result = default(T);

#if NewtonsoftInstalled
            result = JsonConvert.DeserializeObject<T>(obj);
#endif

            return result;
        }

        private static string GetFullFolderName(string folder)
        {
            return Application.persistentDataPath + "/" + folder;
        }
    }
}