using DataClasses;
using Helpers;
using Identifiers;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace SO
{
    [CreateAssetMenu(fileName = nameof(ActivityCategory_SO), menuName = "Scriptables/" + nameof(ActivityCategory_SO))]
    public class ActivityCategory_SO : ScriptableObject
    {
        [field: SerializeField][JsonProperty] public string activityCategoryName { get; private set; }
        [field: SerializeField][JsonProperty] public Sprite icon { get; private set; }
        [field: SerializeField][JsonProperty] public List<Activity> activities { get; private set; } = new();

        [field: SerializeField][JsonProperty] public List<KeyedObject<string, string>> stringSettings { get; private set; } = new();
        [field: SerializeField][JsonProperty] public List<KeyedObject<string, int>> intSettings { get; private set; } = new();
        [field: SerializeField][JsonProperty] public List<KeyedObject<string, float>> floatSettings { get; private set; } = new();
        [field: SerializeField][JsonProperty] public List<KeyedObject<string, Vector3>> vector3Settings { get; private set; } = new();
        [field: SerializeField][JsonProperty] public List<KeyedObject<string, Vector3>> iconOffsets { get; private set; } = new();
        [field: SerializeField][JsonProperty] public List<KeyedObject<string, Vector3>> iconScales { get; private set; } = new();

        [FolderPath] public string contentPath;

        public void Fill(string contentFolder)
        {
            if (string.IsNullOrEmpty(contentFolder) == false)
            {
                contentPath = contentFolder;
                contentPath += $"/{activityCategoryName}";
            }

#if UNITY_EDITOR
            activityCategoryName = name;

            activities.Clear();
            foreach (var picturePath in Directory.GetDirectories(contentPath))
            {
                var currentPicturePath = picturePath;
                // Get the original folder name
                string originalFolderName;
#if UNITY_EDITOR_OSX
                originalFolderName = picturePath.Replace($"{contentPath}/", "");
#else
                originalFolderName = currentPicturePath.Replace($"{contentPath}\\", "");
#endif

                // Clean the folder name: lowercase and keep only letters, numbers, underscore and hyphen
                string cleanedFolderName = Regex.Replace(originalFolderName.ToLower(), @"[^a-z0-9_-]", "");

                // Rename the folder if needed
                if (originalFolderName != cleanedFolderName)
                {
                    string newFolderPath = Path.Combine(contentPath, cleanedFolderName);
                    AssetDatabase.MoveAsset(currentPicturePath, newFolderPath);
                    currentPicturePath = newFolderPath; // Update the path for subsequent operations
                }

                // Rest of your existing code with the cleaned folder name
                var pictureName = cleanedFolderName;

                var iconPath = Directory.GetFiles(currentPicturePath).ToList().Find(x => x.Contains(".jpg"));
                if (iconPath == null) { iconPath = Directory.GetFiles(currentPicturePath).FirstOrDefault(x => x.EndsWith(".png") && x.Contains("icon")); }

                var iconName = iconPath?.Replace(currentPicturePath, "");
                iconName = iconName?.Replace("\\", "");
                iconName = iconName?.Replace("/", "");
                iconName = iconName?.Replace(".jpg", "");

                var prefabName = Directory.GetFiles(currentPicturePath).ToList().Find(x => x.Contains(".prefab"));
                prefabName = prefabName?.Replace("\\", "/");

                var spine = AssetDatabase.LoadMainAssetAtPath(prefabName) as GameObject;
                if (spine == null)
                {
                    Debug.LogError($"{pictureName} is problematic");
                    return;
                }

                var activityIdentifier = spine.GetComponent<_ActivityBase_Identifier>();
                if (activityIdentifier == null)
                {
                    activityIdentifier = spine.AddComponent<_ActivityBase_Identifier>();
                }

                if (activityIdentifier.ignoreBuild == true) { continue; }

                var spineGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(spine));
                var endPictureName = $"{activityCategoryName}-{pictureName}";
                var folderName = endPictureName.Replace($"{activityCategoryName}-", "");

                if (spine.name != endPictureName)
                {
                    var spinePath = AssetDatabase.GetAssetPath(spine);
                    AssetDatabase.RenameAsset(spinePath, endPictureName);
                }

                var picture = new Activity(activityCategoryName, endPictureName, endPictureName.Replace($"{activityCategoryName}-", ""), activityIdentifier.version, iconName)
                {
                    prefab_reference = activityIdentifier
                };
                activityIdentifier.UpdateData(picture);

                var icon_reference = AssetDatabase.LoadMainAssetAtPath(iconPath = iconPath?.Replace("\\", "/")) as Texture2D;
                if (icon_reference != null) { picture.icon_reference = icon_reference; }

                activities.Add(picture);
            }

            this.TrySetDirty();
#endif
        }

        [Button]
        public void UpdateData()
        {
            foreach (var item in activities)
            {
                item.UpdateData();
            }
        }

#if UNITY_EDITOR
        [Button]
        public void UpdateVersion()
        {
            foreach (var item in activities)
            {
                item.prefab_reference.UpdateVersion();
            }
        }
#endif

        public bool TryGetSetting_String(string key, string valueIfNotFound, out string result)
        {
            result = valueIfNotFound;

            var found = stringSettings.Find(x => x.key == key);
            if (found != null)
            {
                result = found.value;
            }

            return found != null;
        }

        public bool TryGetSetting_Int(string key, int valueIfNotFound, out int result)
        {
            result = valueIfNotFound;

            var found = intSettings.Find(x => x.key == key);
            if (found != null)
            {
                result = found.value;
            }

            return found != null;
        }

        public bool TryGetSetting_Float(string key, float valueIfNotFound, out float result)
        {
            result = valueIfNotFound;

            var found = floatSettings.Find(x => x.key == key);
            if (found != null)
            {
                result = found.value;
            }

            return found != null;
        }
    }
}