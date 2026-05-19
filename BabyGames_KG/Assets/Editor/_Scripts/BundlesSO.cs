#if UNITY_EDITOR

using DataClasses;
using Sirenix.OdinInspector;
using SO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Helpers;
using Identifiers;
using Services;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using Path = System.IO.Path;

namespace EditorHelper
{
    [CreateAssetMenu(fileName = nameof(BundlesSO), menuName = "Scriptables/" + nameof(BundlesSO))]
    public class BundlesSO : ScriptableObject
    {
        [SerializeField] private Activities_SO _spinesSO;

        [SerializeField] [FolderPath] private string _contentFolder;
        [SerializeField] [FolderPath] private string _buildPath;
        [SerializeField] private List<Activity> _errorList = new();

        [SerializeField] private List<ActivityCategory_SO> _bundlables = new();

        [Space]
        [SerializeField] private ContentDeliveryService _contentDeliveryService;


        [Button, ButtonGroup("Prepare")]
        private void Fill()
        {
            foreach (var catagory in _spinesSO.activityCategories)
            {
                catagory.Fill(_contentFolder);
            }
        }

        [Button(), ButtonGroup("Prepare")]
        private void UpdateData_SpineToo()
        {
            foreach (var catagory in _spinesSO.activityCategories)
            {
                catagory.UpdateData();
                foreach (var item in catagory.activities)
                {
                    item.prefab_reference.UpdateData(item);
                }
            }
        }

        [Button(), ButtonGroup("Prepare")]
        private void UpdateData()
        {
            foreach (var catagory in _spinesSO.activityCategories)
            {
                catagory.UpdateData();
            }
        }

        [Button(), ButtonGroup("Prepare")]
        private void UpdateVersions_All()
        {
            foreach (var catagory in _spinesSO.activityCategories)
            {
                foreach (var item in catagory.activities)
                {
                    item.prefab_reference.UpdateVersion();
                }
            }
        }

        private string _buttonName_UpdateCloud = nameof(UpdateCloudFor);
        [Button(Name = "$_buttonName_UpdateCloud"), ButtonGroup("Prepare")]
        private async void UpdateCloud()
        {
            if (_contentDeliveryService == null) { return; }

            _errorList.Clear();
            foreach (var catagory in _spinesSO.activityCategories)
            {
                List<Task> tasks = new();
                foreach (var picture in catagory.activities)
                {
                    tasks.Add(UpdateCloudFor(picture, _contentDeliveryService));
                }
                await Task.WhenAll(tasks);
            }
            _buttonName_UpdateCloud = nameof(UpdateCloud);
        }

        private async Task UpdateCloudFor(Activity picture, ContentDeliveryService contentDeliveryService)
        {
            picture._errorList.Clear();

            contentDeliveryService.ClearCache(picture.GetName(), picture.version, picture.GetUrl());
            var go = await contentDeliveryService.GetAsset_Raw<GameObject>(picture.GetUrl(), picture.GetName(), picture.version);
            var spine = go?.GetComponent<_ActivityBase_Identifier>();

            var components = go?.GetComponents<Component>();

            if (spine == null)
            {
                picture._errorList.Add($"{picture.activityName} is null");
            }
            else
            {
                if (spine.version != picture.version) { picture._errorList.Add($"{picture.activityName} is not version synced: Local {picture.version} - Remote {spine.version}"); }
            }

            if (picture._errorList.Count > 0)
            {
                _errorList.SafeAdd(picture);
            }

            _buttonName_UpdateCloud = picture.activityName;
        }

        [Button]
        private void CreateAssetBundle()
        {
            AssetDatabase.RemoveUnusedAssetBundleNames();

            foreach (var catagory in _spinesSO.activityCategories)
            {
                foreach (var picture in catagory.activities)
                {
                    try
                    {
                        bool shouldBeBundlable = false;

                        if (_errorList.Contains(picture)) { shouldBeBundlable = true; }
                        if (_bundlables.Contains(catagory)) { shouldBeBundlable = true; }
                        if (_bundlables.Count < 1) { shouldBeBundlable = true; }
                        if (picture.prefab_reference.ignoreBuild) { shouldBeBundlable = false; }

                        string assetBundleName = shouldBeBundlable ? picture.GetName() : "";
                        string variantName = shouldBeBundlable ? "bundle" : "";

                        if (shouldBeBundlable) { Debug.Log("Bundles: " + assetBundleName); }

                        SetBundleName(assetBundleName, variantName, picture.prefab_reference);

                        if (picture.icon_reference != null && picture.icon_reference.name.Contains("icon"))
                        {
                            SetBundleName($"{assetBundleName}-icon", variantName, picture.icon_reference);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Error while CreateAssetBundle: " + ex.Message);
                    }
                }
            }

            SetBundleName(Activities_SO.BUNDLE_NAME, "bundle", _spinesSO);

            AssetDatabase.RemoveUnusedAssetBundleNames();
        }

        public static void SetBundleName(string assetBundleName, string variantName, Object obj)
        {
            string spinePath = $"{AssetDatabase.GetAssetPath(obj)}.meta";

            var lines = File.ReadAllLines(spinePath).ToList();

            for (var i = 0; i < lines.Count; i++)
            {
                if (lines[i].Contains("assetBundleName"))
                {
                    lines[i] = $"  assetBundleName: {assetBundleName}";
                }

                if (lines[i].Contains("assetBundleVariant"))
                {
                    lines[i] = $"  assetBundleVariant: {variantName}";
                }
            }

            lines.RemoveAll(x => string.IsNullOrEmpty(x) || string.IsNullOrWhiteSpace(x));

            File.WriteAllLines(spinePath, lines);
        }

        [Button]
        private void Build()
        {
            Build(EditorUserBuildSettings.activeBuildTarget);
        }

        [Button]
        private void Build(BuildTarget buildTarget)
        {
            UpdateData();

            string buildPath = $"{_buildPath}/{buildTarget}";
            if (Directory.Exists(buildPath) == false)
            {
                Directory.CreateDirectory(buildPath);
            }

            DeleteAll(buildPath);

            BuildPipeline.BuildAssetBundles(buildPath, BuildAssetBundleOptions.None, buildTarget);
            SortFilesIn();

            Process.Start(Path.GetFullPath(buildPath));
        }

        [Button]
        private void DeleteAll(string path)
        {
            if (Directory.Exists(path))
            {
                foreach (string file in Directory.GetFiles(path))
                {
                    File.Delete(file);
                }

                foreach (string directory in Directory.GetDirectories(path))
                {
                    Directory.Delete(directory, true);
                }

                Console.WriteLine("All files and folders have been deleted.");
            }
        }

        private string _sortFilesIn_ButtonName = nameof(SortFilesIn);

        [Button(Name = "_sortFilesIn_ButtonName")]
        private void SortFilesIn()
        {
            string[] folders = Directory.GetDirectories(_buildPath);

            foreach (var folder in folders)
            {
                string[] files = Directory.GetFiles(folder);
                foreach (var file in files)
                {
                    if (file.EndsWith(".bundle") == false)
                    {
                        File.Delete(file);
                        continue;
                    }

                    string fileName = System.IO.Path.GetFileName(file);
                    _sortFilesIn_ButtonName = fileName;

                    string[] parts = fileName.Split('-');
                    if (parts.Length > 1)
                    {
                        string categoryName = parts[0];
                        string modeName = parts[1];

                        if (string.IsNullOrEmpty(modeName))
                        {
                            string destinationFolder = System.IO.Path.Combine(folder, categoryName);

                            if (!Directory.Exists(destinationFolder))
                            {
                                Directory.CreateDirectory(destinationFolder);
                            }

                            string destinationPath = Path.Combine(destinationFolder, fileName);
                            destinationFolder.Replace("\\", "//");
                            File.Move(file, destinationPath);

                            Console.WriteLine($"Moved: {fileName} -> {destinationPath}");
                        }
                        else
                        {
                            string destinationFolder = System.IO.Path.Combine(folder, categoryName);
                            if (!Directory.Exists(destinationFolder))
                            {
                                Directory.CreateDirectory(destinationFolder);
                            }

                            destinationFolder.Replace("\\", "//");

                            destinationFolder = System.IO.Path.Combine(destinationFolder, modeName.Replace(".bundle", ""));
                            if (!Directory.Exists(destinationFolder))
                            {
                                Directory.CreateDirectory(destinationFolder);
                            }

                            destinationFolder.Replace("\\", "//");

                            string destinationPath = Path.Combine(destinationFolder, fileName);
                            File.Move(file, destinationPath);

                            Console.WriteLine($"Moved: {fileName} -> {destinationPath}");
                        }
                    }
                }

                Console.WriteLine("All files moved successfully.");
            }

            _sortFilesIn_ButtonName = nameof(SortFilesIn);
        }
    }
}
#endif