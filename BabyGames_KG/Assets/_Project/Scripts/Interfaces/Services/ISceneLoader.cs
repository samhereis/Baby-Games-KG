using System;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace Interfaces
{
    public interface ISceneLoader
    {
        public Task LoadSceneAsyncWithTransition(string nameOfScene, Action onLoadedCallback = null, Action onError = null);
        public Task LoadSceneAsyncWithTransition(AssetReference assetReference, Action onLoadedCallback = null, Action onError = null);

        public void ShowLoadingLayer();
        public Task ShowLoadingLayerAsync(float duration = 0.25f);
        public Task HideLoadingLayerAsync();
    }
}