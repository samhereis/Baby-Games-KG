using DataClasses.Consts;
using Identifiers;
using Interfaces.Providers;
using Interfaces.Services;
using Loggers;
using Newtonsoft.Json;
using Services;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _Project.Scripts.Services.Purchase;
using UnityEngine;
using Application = UnityEngine.Application;

namespace DataClasses
{
    [Serializable]
    public class Activity
    {
        [field: SerializeField, FoldoutGroup("$activityName")] [JsonProperty] public string activityName { get; private set; } = "ActivityName";
        [field: SerializeField, FoldoutGroup("$activityName/Names")] [JsonProperty] public string displayName { get; set; } = "";
        [field: SerializeField, FoldoutGroup("$activityName/Names")] [JsonProperty] public string acitvityCategory { get; private set; } = "ActivityCategory";
        [field: SerializeField, FoldoutGroup("$activityName/Names")] [JsonProperty] public string activityFolder { get; private set; } = "ActivityMode";

        [field: SerializeField, FoldoutGroup("$activityName/Additional")] [JsonProperty] public ActivityType type { get; private set; }
        [field: SerializeField, FoldoutGroup("$activityName/Additional")] [JsonProperty] public string subtype { get; set; }
        [field: SerializeField, FoldoutGroup("$activityName/Additional")] [JsonProperty] public string version { get; private set; }
        [field: SerializeField, FoldoutGroup("$activityName/Additional")] [JsonProperty] public Vector3Int gameVersionSupported { get; private set; }
        [field: SerializeField, FoldoutGroup("$activityName/Additional")] [JsonProperty] public string icon { get; private set; }
        [field: SerializeField, FoldoutGroup("$activityName/Additional")] [JsonProperty] public bool isAlwaysUnlocked { get; set; }

        [field: SerializeField, FoldoutGroup("$activityName/Links")] [JsonProperty] public string assetBundleUrl_Android { get; private set; } = "";
        [field: SerializeField, FoldoutGroup("$activityName/Links")] [JsonProperty] public string assetBundleUrl_iOS { get; private set; } = "";
        [field: SerializeField, FoldoutGroup("$activityName/Links")] [JsonProperty] public string assetBundleUrl_Windows { get; private set; } = "";
        [field: SerializeField, FoldoutGroup("$activityName/Links")] [JsonProperty] public string assetBundleUrl_OSX { get; private set; } = "";

        [field: SerializeField, FoldoutGroup("$activityName/Localization")] [JsonProperty] public string loc_tableName { get; set; } = Constants_Localization.Categories;
        [field: SerializeField, FoldoutGroup("$activityName/Localization")] [JsonProperty] public string loc_entryName { get; set; } = "";
        [field: SerializeField, FoldoutGroup("$activityName/Localization")] [JsonProperty] public string loc_tableName_String { get; set; } = Constants_Localization.Categories;
        [field: SerializeField, FoldoutGroup("$activityName/Localization")] [JsonProperty] public string loc_entryName_String { get; set; } = "";

        private Sprite _iconSprite;

#if UNITY_EDITOR
        [field: SerializeField, FoldoutGroup("$activityName/Debug")] [JsonIgnore] public _ActivityBase_Identifier prefab_reference;
        [field: SerializeField, FoldoutGroup("$activityName/Debug")] [JsonIgnore] public Texture2D icon_reference;
        [field: SerializeField, FoldoutGroup("$activityName/Debug")] [JsonIgnore] public List<string> _errorList = new();
#endif

        public Activity()
        {

        }

        public Activity(string newCategory, string newName, string newFolder, string newVertion, string newIcon = null)
        {
            activityName = newName;
            acitvityCategory = newCategory;
            activityFolder = newFolder;
            version = newVertion;
            icon = newIcon;
        }

        public string GetUrl()
        {
#if UNITY_ANDROID
            if (string.IsNullOrEmpty(assetBundleUrl_Android) || string.IsNullOrWhiteSpace(assetBundleUrl_Android)) { assetBundleUrl_Android = $"{Constants_CD.GetAssetBundlePath()}/{acitvityCategory}/{activityName}.bundle"; }
            return assetBundleUrl_Android;
#endif
#if UNITY_IOS
            if (string.IsNullOrEmpty(assetBundleUrl_iOS) || string.IsNullOrWhiteSpace(assetBundleUrl_iOS)) { assetBundleUrl_iOS = $"{Constants_CD.GetAssetBundlePath()}/{acitvityCategory}/{activityName}.bundle"; }
            return assetBundleUrl_iOS;
#endif
#if UNITY_STANDALONE_WIN
            if (string.IsNullOrEmpty(assetBundleUrl_Windows) || string.IsNullOrWhiteSpace(assetBundleUrl_Windows)) { assetBundleUrl_Windows = $"{Constants_CD.GetAssetBundlePath()}/{acitvityCategory}/{activityName}.bundle"; }
            return assetBundleUrl_Windows;
#endif
#if UNITY_STANDALONE_OSX
            if (string.IsNullOrEmpty(assetBundleUrl_OSX) || string.IsNullOrWhiteSpace(assetBundleUrl_OSX)) { assetBundleUrl_OSX = $"{Constants_CD.GetAssetBundlePath()}/{acitvityCategory}/{activityName}.bundle"; }
            return assetBundleUrl_OSX;
#endif
        }

        public string GetName()
        {
            return activityName;
        }

        public string GetIconUrl()
        {
            if (string.IsNullOrEmpty(icon)) { return string.Empty; }
            return $"{GetUrl().Replace(".bundle", "")}-icon.bundle";
        }

        public async Task<Sprite> GetIcon(IContentDeliveryService contentDeliveryService)
        {
            if (string.IsNullOrEmpty(GetIconUrl())) { return _iconSprite; }
            if (_iconSprite != null) { return _iconSprite; }

            var iconBundle = await contentDeliveryService.GetAsset<Texture2D>(GetIconUrl(), icon, version);
            if (iconBundle != null)
            {
                _iconSprite = Sprite.Create(iconBundle, new Rect(0.0f, 0.0f, iconBundle.width, iconBundle.height), new Vector2(0.5f, 0.5f));
            }

            return _iconSprite;
        }

        public void UpdateData()
        {
#if UNITY_EDITOR
            if (Application.isPlaying) { return; }
            type = prefab_reference.type;
            version = prefab_reference.version;
#endif

            assetBundleUrl_Android = $"{Constants_CD.GetAssetBundlePath("Android")}/{acitvityCategory}/{activityFolder}/{activityName}.bundle";
            assetBundleUrl_iOS = $"{Constants_CD.GetAssetBundlePath("iOS")}/{acitvityCategory}/{activityFolder}/{activityName}.bundle";
            assetBundleUrl_Windows = $"{Constants_CD.GetAssetBundlePath("StandaloneWindows64")}/{acitvityCategory}/{activityFolder}/{activityName}.bundle";
            assetBundleUrl_OSX = $"{Constants_CD.GetAssetBundlePath("StandaloneOSX")}/{acitvityCategory}/{activityFolder}/{activityName}.bundle";
        }

        public bool IsUnlocked(ISubscriptionChecker subscriptionChecker)
        {
            if (isAlwaysUnlocked) { return true; }

            return subscriptionChecker.IsSubscribed();
        }

        public async Task<Activity> GetNextActivity(IActivities_SO_Provider activities_SO_Provider)
        {
            Activity foundActivity = null;

            try
            {
                IContentDeliveryService contentDeliveryService = DiService.Get<IContentDeliveryService>();
                ISubscriptionChecker subscriptionChecker = DiService.Get<ISubscriptionChecker>();
                var activities = await activities_SO_Provider.GetActivities_SO();

                var category = activities.activityCategories.Find(x => x.activityCategoryName == acitvityCategory);
                var index = category.activities.IndexOf(this) + 1;

                for (int i = index; i < category.activities.Count; i++)
                {
                    var temp = category.activities[i];

                    if (subscriptionChecker.IsSubscribed())
                    {
                        if (contentDeliveryService.IsAssetCashed(temp.GetUrl(), temp.version))
                        {
                            foundActivity = temp;
                            break;
                        }
                    }
                    else
                    {
                        if (contentDeliveryService.IsAssetCashed(temp.GetUrl(), temp.version) && temp.isAlwaysUnlocked)
                        {
                            foundActivity = temp;
                            break;
                        }
                    }
                }

                if (foundActivity == null) { foundActivity = category.activities.First(x => contentDeliveryService.IsAssetCashed(x.GetUrl(), x.version)); }
            } catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            } finally
            {
                if (foundActivity == null) { foundActivity = this; }
            }

            return foundActivity;
        }
    }
}