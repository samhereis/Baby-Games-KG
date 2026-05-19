using DataClasses;
using Helpers;
using Interfaces.Services;
using Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _Project.Scripts.Data;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace Services
{
    public class ContentDeliveryService : MonoBehaviour, IContentDeliveryService
    {
        [SerializeField] private List<AssetBundle> _loadedAssetBundles = new();

        public bool IsLoaded(string name, out AssetBundle assetBundle)
        {
            assetBundle = null;

            try
            {
                _loadedAssetBundles = AssetBundle.GetAllLoadedAssetBundles().ToList();
                foreach (var item in _loadedAssetBundles)
                {
                    ClearablesHolder.instance.clearableBundles.SafeAdd(item);

                    if (item.name == $"{name}.bundle")
                    {
                        assetBundle = item;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex, $"Error during check IsLoaded({name})");
            }

            return false;
        }

        public bool IsAssetCashed(string url, string version = "0")
        {
            bool isCached = false;

            try
            {
                CachedAssetBundle cached = new CachedAssetBundle(url, Hash128.Parse(version));
                isCached = Caching.IsVersionCached(cached);
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex, $"Error during check IsAssetCashed({name})");
            }

            return isCached;
        }

        public async Task<T> GetAsset<T>(string url, string name, string version = "0", Action<float> onDownloadingUpdate = null) where T : Object
        {
            AssetBundle bundle = null;
            T asset = default(T);

            try
            {
                bundle = await GetAsset(url, name, version, onDownloadingUpdate);
                ClearablesHolder.instance.clearableBundles.SafeAdd(bundle);

                var assetNames = bundle?.GetAllAssetNames();
                asset = bundle?.LoadAsset<T>(name);

                bundle.Unload(false);
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex, $"Error during check GetAsset({name})");
            }

            return asset;
        }

        // Returns caches if possible
        public async Task<AssetBundle> GetAsset(string url, string name, string version = "0", Action<float> onDownloadingUpdate = null)
        {
            if (IsLoaded(name, out var assetBundle))
            {
                return assetBundle;
            }

            AssetBundle bundle = null;

            try
            {
                CachedAssetBundle cached = new CachedAssetBundle(url, Hash128.Parse(version));
                var uWR = UnityWebRequestAssetBundle.GetAssetBundle(url, cached, 0);
                var operation = uWR.SendWebRequest();

                while (operation.isDone == false)
                {
                    onDownloadingUpdate?.Invoke(operation.progress);
                    await AsyncHelper.NextFrame();
                }

                if (uWR.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"Could not download {url}");
                }
                else
                {
                    bundle = DownloadHandlerAssetBundle.GetContent(uWR);
                    ClearablesHolder.instance.clearableBundles.SafeAdd(bundle);
                }
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex, $"Error during check GetAsset({name})");
            }

            return bundle;
        }

        public async Task<T> GetAsset_Raw<T>(string url, string name, string version = "0", Action<float> onDownloadingUpdate = null) where T : Object
        {
            AssetBundle bundle = null;
            T asset = default(T);

            try
            {
                bundle = await GetAsset_Raw(url, name, version, onDownloadingUpdate);
                ClearablesHolder.instance?.clearableBundles.SafeAdd(bundle);

                asset = bundle.LoadAsset<T>(name);

                bundle.Unload(false);
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex, $"Error during check GetAsset_Raw({name})");
            }

            return asset;
        }

        // Always downloads
        public async Task<AssetBundle> GetAsset_Raw(string url, string name, string version = "0", Action<float> onDownloadingUpdate = null)
        {
            if (IsLoaded(name, out var assetBundle))
            {
                return assetBundle;
            }

            AssetBundle bundle = null;

            try
            {
                var uWR = UnityWebRequestAssetBundle.GetAssetBundle(url, 0);
                var operation = uWR.SendWebRequest();

                while (operation.isDone == false)
                {
                    onDownloadingUpdate?.Invoke(operation.progress);
                    await AsyncHelper.NextFrame();
                }

                if (uWR.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"Could not download {url}");
                }
                else
                {
                    bundle = DownloadHandlerAssetBundle.GetContent(uWR);
                    ClearablesHolder.instance.clearableBundles.SafeAdd(bundle);
                }
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex, $"Error during check GetAsset_Raw({name})");
            }

            return bundle;
        }

        public void UnloadAssetBundle(string name)
        {
            try
            {
                if (IsLoaded(name, out var bundle))
                {
                    bundle.Unload(true);
                }
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex, $"Error during check UnloadAssetBundle({name})");
            }
        }

        public async Task ClearCacheAsync(string bundleName, string version, string url, int times = 3)
        {
            try
            {
                for (int i = 0; i < times; i++)
                {
                    ClearCache(bundleName, version, url);

                    await AsyncHelper.DelayFloat(0.25f);
                }
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex, $"Error during ClearCacheAsync({bundleName})");
            }
        }

        public void ClearCache(string bundleName, string version, string url)
        {
            try
            {
                if (IsAssetCashed(url))
                {
                    Cache cache = Caching.GetCacheByPath(url);
                    if (cache.valid) { Caching.RemoveCache(cache); }
                }

                Caching.ClearAllCachedVersions(bundleName);
                Caching.ClearCachedVersion(bundleName, Hash128.Parse(version));

                Caching.ClearAllCachedVersions(bundleName + ".bundle");
                Caching.ClearCachedVersion(bundleName + ".bundle", Hash128.Parse(version));

                Caching.ClearAllCachedVersions(url);
                Caching.ClearCachedVersion(url, Hash128.Parse(version));
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex, $"Error during check ClearCache({bundleName})");
            }
        }
    }
}