using DataClasses;
using Helpers;
using Identifiers;
using Interfaces.Providers;
using Interfaces.Services;
using Loggers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Services.Factories
{
    public class Activity_Identifier_Provider : MonoBehaviour, IActivity_Identifier_Provider
    {
        [Inject] private IContentDeliveryService _contentDeliveryService;

        [SerializeField] private List<KeyedObject<Activity, bool>> checkedForUpdate = new();

        private void Awake()
        {
            DiService.Inject(this);
        }

        public bool IsCashed(Activity activity)
        {
            if (DevelopmentConfigs.contentDeliveryDebugMode == true)
            {
                return true;
            }

            bool isCashed = false;

            try
            {
                isCashed = _contentDeliveryService.IsAssetCashed(activity.GetUrl(), activity.version);
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex, $"Error during check IsCashed({activity.GetName()})");
            }

            return isCashed;
        }

        public async Task<bool> HasUpdate(Activity activity)
        {
            bool hasUpdate = false;
            if (activity == null) { return hasUpdate; }

            try
            {
                KeyedObject<Activity, bool> cached = checkedForUpdate.Find(x => x.key == activity);
                if (cached != null) { return cached.value; }

                if (!ApplicationHelper.HasInternetConnection()) { return hasUpdate; }
                if (string.IsNullOrEmpty(activity.version) || string.IsNullOrWhiteSpace(activity.version)) { return hasUpdate; }

                _ActivityBase_Identifier activityBase_Identifier = await GetActivity(activity);
                if (activityBase_Identifier == null) { return hasUpdate; }
                if (string.IsNullOrEmpty(activityBase_Identifier.version) || string.IsNullOrWhiteSpace(activityBase_Identifier.version)) { return hasUpdate; }
                if (activityBase_Identifier.version != activity.version) { hasUpdate = true; }

                if (hasUpdate) { Debug.Log($"{activity.GetName()} needs an update!"); }

                _contentDeliveryService.UnloadAssetBundle(activity.GetName());
                _contentDeliveryService.UnloadAssetBundle(activity.GetUrl());

                if (hasUpdate == false)
                {
                    checkedForUpdate.Add(new(activity, hasUpdate));
                }
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex, $"Error during check HasUpdate({activity.GetName()})");
            }

            return hasUpdate;
        }

        public async Task<_ActivityBase_Identifier> InstantiaseActivity(Activity activity)
        {
            _ActivityBase_Identifier activityBase_Identifier = await GetActivity(activity);
            if (activityBase_Identifier == null)
            {
                return null;
            }

            return Instantiate(activityBase_Identifier);
        }

        public async Task<_ActivityBase_Identifier> GetActivity(Activity activity, Action<float> onDownloadingUpdate = null)
        {
#if UNITY_EDITOR
            if (DevelopmentConfigs.contentDeliveryDebugMode == true)
            {
                return activity.prefab_reference;
            }
#endif

            _ActivityBase_Identifier activityBase_Identifier = null;

            if (IsCashed(activity))
            {
                GameObject activityBase_Identifier_gameobjet = await _contentDeliveryService.GetAsset<GameObject>(activity.GetUrl(), activity.GetName(), activity.version,
                    (progress) => { onDownloadingUpdate?.Invoke(progress); });

                if (activityBase_Identifier_gameobjet != null)
                {
                    activityBase_Identifier = activityBase_Identifier_gameobjet.GetComponent<_ActivityBase_Identifier>();
                }
            }
            else
            {
                activityBase_Identifier = await Download(activity, onDownloadingUpdate);
            }

            return activityBase_Identifier;
        }

        public async Task<_ActivityBase_Identifier> Download(Activity activity, Action<float> onDownloadingUpdate = null)
        {
            _ActivityBase_Identifier activityBase_Identifier = null;

            for (int i = 0; i < 5; i++)
            {
                GameObject activityBase_Identifier_gameobjet = await _contentDeliveryService.GetAsset<GameObject>(activity.GetUrl(), activity.GetName(), activity.version,
                    (progress) => { onDownloadingUpdate?.Invoke(progress); });

                if (activityBase_Identifier_gameobjet != null)
                {
                    activityBase_Identifier = activityBase_Identifier_gameobjet.GetComponent<_ActivityBase_Identifier>();
                }

                bool downloaded = activityBase_Identifier != null && IsCashed(activity);

                if (downloaded == false)
                {
                    CustomLogger.instance?.LogWarning("Could not download", activity.GetName(), null, LogTypes.General);
                    await AsyncHelper.DelayFloat(0.25f);
                }
                else
                {
                    break;
                }
            }

            return activityBase_Identifier;
        }
    }
}