using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Helpers
{
    public class AddressablesHelper
    {
        public static float initialDownloadSize = 0;

        public static float currentDownloadPercentage;
        public static float currentlyDownloaded;
        public static float totalSize;

        private static int _roundValue = 2;

        public static float ParseSize(long size)
        {
            return MathF.Round(size / (1024f * 1024f), _roundValue);
        }

        public static async Task<float> GetDownloadSize(string groupNameOrLabel)
        {
            var getDownloadSizeHandle = Addressables.GetDownloadSizeAsync(groupNameOrLabel);
            await getDownloadSizeHandle.Task;

            if (getDownloadSizeHandle.Status == AsyncOperationStatus.Succeeded)
            {
                long downloadSize = getDownloadSizeHandle.Result;
                initialDownloadSize = ParseSize(downloadSize);
            }
            else
            {
                Debug.LogError("Failed to get download size.");
            }

            return initialDownloadSize;
        }

        public static async Task<float> GetDownloadSize(AssetReference assetReference)
        {
            var getDownloadSizeHandle = Addressables.GetDownloadSizeAsync(assetReference);
            await getDownloadSizeHandle.Task;

            if (getDownloadSizeHandle.Status == AsyncOperationStatus.Succeeded)
            {
                long downloadSize = getDownloadSizeHandle.Result;
                initialDownloadSize = ParseSize(downloadSize);
            }
            else
            {
                Debug.LogError("Failed to get download size.");
            }

            return initialDownloadSize;
        }

        public static async Task CheckForUpdates(Action<float> onUpdate = null)
        {
            var handle = Addressables.CheckForCatalogUpdates();
            await Handle(handle, "Update", onUpdate, true);

            if (handle.Result?.Count > 0)
            {
                foreach (var item in handle.Result)
                {
                    Debug.Log($"Addressable needs an update: {item}");
                    await Addressables.UpdateCatalogs(handle.Result).Task;
                }
            }
            else
            {
                Debug.Log($"No addressable updates found");
            }
        }


        public static async Task<T> GetAssetAsync<T>(string name, Action<float> onUpdate = null)
        {
            var handle = Addressables.LoadAssetAsync<T>(name);
            await Handle(handle, name, onUpdate);

            var result = handle.Result;
            return result;
        }

        public static async Task<T> GetAssetAsync<T>(AssetReference assetReference, Action<float> onUpdate = null)
        {
            if (assetReference == null)
            {
                Debug.LogWarning($"Addressable Reference is null: {assetReference.ToString()}");
                return default;
            }

            var handle = Addressables.LoadAssetAsync<T>(assetReference);
            await Handle(handle, assetReference.ToString(), onUpdate);

            var result = handle.Result;
            return result;
        }

        public static async Task<bool> PreloadAssets<T>(IEnumerable<AssetReference> assetReferences, Action<T> onAssetLoaded = null)
        {
            if (assetReferences == null)
            {
                Debug.LogWarning($"Addressable Reference is null: {assetReferences.ToString()}");
            }

            var handle = Addressables.LoadAssetsAsync<T>(assetReferences, onAssetLoaded, Addressables.MergeMode.Union);

            string key = "";
            foreach (var item in assetReferences)
            {
                key += item.ToString();
            }

            await Handle(handle, key);

            var result = handle.Status == AsyncOperationStatus.Succeeded;
            return result;
        }

        public static async Task<bool> PreloadAssets<T>(IEnumerable<string> assetReferences, Action<T> onAssetLoaded = null)
        {
            if (assetReferences == null)
            {
                Debug.LogWarning($"Addressable Reference is null: {assetReferences.ToString()}");
            }

            var handle = Addressables.LoadAssetsAsync<T>(assetReferences, onAssetLoaded, Addressables.MergeMode.Union);

            string key = "";
            foreach (var item in assetReferences)
            {
                key += item.ToString();
            }

            await Handle(handle, key);

            var result = handle.Status == AsyncOperationStatus.Succeeded;
            return result;
        }

        public static async Task<T> InstantiateAsync<T>(string name, Vector3 position = new Vector3(), Quaternion rotation = new Quaternion(), Transform parent = null)
        {
            var handle = Addressables.InstantiateAsync(name, position, rotation, parent);
            await Handle(handle, name);

            if (handle.Result == null)
            {
                return default;
            }

            var result = handle.Result.GetComponent<T>();
            return result;
        }

        public static async Task<T> InstantiateAsync<T>(AssetReference assetReference, Vector3 position = new Vector3(), Quaternion rotation = new Quaternion(), Transform parent = null) where T : Object
        {
            if (assetReference == null)
            {
                Debug.LogWarning($"Adrressable Reference is null: {assetReference.ToString()}");
                return default;
            }

            var handle = Addressables.InstantiateAsync(assetReference, position, rotation, parent);
            await Handle(handle, assetReference.ToString());

            if (handle.Result == null)
            {
                return default;
            }

            var result = handle.Result.GetComponent<T>();
            return result;
        }

        public static void Release<T>(T toRelease)
        {
            Addressables.Release<T>(toRelease);
        }

        public static void DestroyObject(GameObject gameObject)
        {
            if (Addressables.ReleaseInstance(gameObject) == false)
            {
                Object.Destroy(gameObject);
            }
        }

        public static async Task Handle(AsyncOperationHandle asyncOperationHandle, string key, Action<float> onUpdate = null, bool debugPercent = false)
        {
            while (asyncOperationHandle.IsDone == false)
            {
                if (Application.isPlaying == false) { asyncOperationHandle.Task.Dispose(); }

                currentDownloadPercentage = MathF.Round(asyncOperationHandle.GetDownloadStatus().Percent, _roundValue);
                currentlyDownloaded = ParseSize(asyncOperationHandle.GetDownloadStatus().DownloadedBytes);
                totalSize = ParseSize(asyncOperationHandle.GetDownloadStatus().TotalBytes);

                if (debugPercent) { Debug.Log(key + asyncOperationHandle.PercentComplete); }

                onUpdate?.Invoke(currentDownloadPercentage);
                await AsyncHelper.NextFrame();
            }
        }
    }
}