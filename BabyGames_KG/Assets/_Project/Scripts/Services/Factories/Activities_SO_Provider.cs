using CustomAttributes;
using DataClasses;
using DataClasses.Consts;
using Helpers;
using Interfaces.Providers;
using Interfaces.Services;
using Loggers;
using Sirenix.OdinInspector;
using SO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Services.Factories
{
    public class Activities_SO_Provider : MonoBehaviour, IActivities_SO_Provider
    {
        private string _ACTIVITIES_URL => $"{Constants_CD.GetAssetBundlePath()}/{Activities_SO.BUNDLE_NAME}.bundle";

        [Inject] private IContentDeliveryService _contentDeliveryService;

        [SerializeField] private List<KeyedObject<Activity, bool>> checkedForUpdate = new();

        //[Space]
        //[SerializeField] private Activities_SO _activities_SOLocal;
        //[SerializeField] private bool _forceLocal;

        [ShowInInspector, Fg_De] private Activities_SO _activities_SO;

        private void Awake()
        {
            DiService.Inject(this);
        }

        public bool IsCashed()
        {
            bool isCashed = false;

            try
            {
                isCashed = _contentDeliveryService.IsAssetCashed(_ACTIVITIES_URL);
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex, $"Error during check {Activities_SO.BUNDLE_NAME})");
            }

            return isCashed;
        }

        public async Task<Activities_SO> Download(Action<float> onDownloadingUpdate = null)
        {
            //if (_forceLocal && _activities_SOLocal != null) { return _activities_SOLocal; }

            Activities_SO activities_SO = null;

            for (int i = 0; i < 5; i++)
            {
                activities_SO = await _contentDeliveryService.GetAsset<Activities_SO>(_ACTIVITIES_URL, Activities_SO.BUNDLE_NAME, "",
                    (progress) => { onDownloadingUpdate?.Invoke(progress); });

                bool downloaded = activities_SO != null && IsCashed();

                if (downloaded == false)
                {
                    CustomLogger.instance?.LogWarning("Could not download", Activities_SO.BUNDLE_NAME, null, LogTypes.General);
                    await AsyncHelper.DelayFloat(0.25f);
                }
                else
                {
                    break;
                }
            }

            //if (activities_SO == null) { activities_SO = _activities_SOLocal; }

            return activities_SO;
        }

        public async Task<Activities_SO> GetActivities_SO(Action<float> onDownloadingUpdate = null)
        {
            if (_activities_SO != null)
            {
                return _activities_SO;
            }
            else
            {
                try
                {
                    _activities_SO = await _contentDeliveryService.GetAsset_Raw<Activities_SO>(_ACTIVITIES_URL, Activities_SO.BUNDLE_NAME, "",
                        (progress) => { onDownloadingUpdate?.Invoke(progress); });

                    if (_activities_SO != null)
                    {
                        Debug.Log($"Force updating {Activities_SO.BUNDLE_NAME} - {_ACTIVITIES_URL}");

                        await _contentDeliveryService.ClearCacheAsync(Activities_SO.BUNDLE_NAME, "", _ACTIVITIES_URL);
                        Debug.Log($"Was {Activities_SO.BUNDLE_NAME} Cleared: {_contentDeliveryService.IsAssetCashed(_ACTIVITIES_URL)}");
                        _activities_SO = await Download(onDownloadingUpdate);
                        Debug.Log($"Is {Activities_SO.BUNDLE_NAME} Cashed: {_contentDeliveryService.IsAssetCashed(_ACTIVITIES_URL)}");
                    }
                    else
                    {
                        Debug.Log($"Is {Activities_SO.BUNDLE_NAME} Cashed: {_contentDeliveryService.IsAssetCashed(_ACTIVITIES_URL)}");
                        _activities_SO = await Download(onDownloadingUpdate);
                    }
                }
                catch (Exception ex)
                {
                    CustomLogger.instance?.LogException(ex);
                }
            }

            return _activities_SO;
        }
    }
}