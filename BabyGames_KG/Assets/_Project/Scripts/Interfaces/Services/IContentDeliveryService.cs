using System;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Interfaces.Services
{
    public interface IContentDeliveryService
    {
        public bool IsLoaded(string name, out AssetBundle assetBundle);

        public bool IsAssetCashed(string url, string version = "0");

        public Task<T> GetAsset<T>(string url, string name, string version = "0", Action<float> onDownloadingUpdate = null) where T : Object;

        public Task<AssetBundle> GetAsset(string url, string name, string version = "0", Action<float> onDownloadingUpdate = null);

        public Task<T> GetAsset_Raw<T>(string url, string name, string version = "0", Action<float> onDownloadingUpdate = null) where T : Object;
        public Task<AssetBundle> GetAsset_Raw(string url, string name, string version = "0", Action<float> onDownloadingUpdate = null);

        public void UnloadAssetBundle(string name);

        public Task ClearCacheAsync(string bundleName, string version, string url, int times = 3);

        public void ClearCache(string bundleName, string version, string url);
    }
}