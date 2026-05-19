using DG.Tweening;
using Helpers;
using Interfaces;
using Loggers;
using System;
using System.Threading.Tasks;
using UI.Menus;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Services
{
    [RequireComponent(typeof(CanvasGroup))]
    public class SceneLoader : MonoBehaviour, ISceneLoader
    {
        [SerializeField] private Image _background;
        [SerializeField] private CanvasGroup _canvasGroup;

        private GameObject _destroyable;
        private bool _isOpenning = false;

        protected void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();

            _canvasGroup.FadeDownQuick();

            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            _canvasGroup?.DOKill();
        }

        public async Task LoadSceneAsyncWithTransition(string nameOfScene, Action onLoadedCallback = null, Action onError = null)
        {
            try
            {
                var operatopm = SceneManager.LoadSceneAsync(nameOfScene);
                while (operatopm.isDone == false) { await AsyncHelper.Skip(); }

                onLoadedCallback?.Invoke();
            }
            catch (Exception e)
            {
                CustomLogger.instance?.LogException(e, $"Could not load scene: {nameOfScene}");

                onError?.Invoke();
            }
        }

        public async Task LoadSceneAsyncWithTransition(AssetReference scene, Action onLoadedCallback = null, Action onError = null)
        {
            try
            {
                var operation = Addressables.LoadSceneAsync(scene);
                while (operation.IsDone == false) { await AsyncHelper.Skip(); }

                onLoadedCallback?.Invoke();
            }
            catch (Exception e)
            {
                CustomLogger.instance?.LogException(e, $"Could not load scene: {scene.RuntimeKey}");

                onError?.Invoke();
            }
        }

        public async void ShowLoadingLayer()
        {
            await ShowLoadingLayerAsync();
        }

        public async Task ShowLoadingLayerAsync(float duration = 0.25f)
        {
            try
            {
                if (_isOpenning) { return; }
                _isOpenning = true;

                _background?.DOKill();
                if (_canvasGroup != null)
                {
                    _canvasGroup?.DOKill();
                    await _canvasGroup.FadeUp(duration).AsyncWaitForCompletion();
                }

                if (Activities_Menu.currentBackground != null)
                {
                    if (Activities_Menu.currentBackground.TryGetComponent<Image>(out var image))
                    {
                        if (_destroyable != null) { Destroy(_destroyable); }

                        _background?.DOFade(1, 0);
                        _background.sprite = image.sprite;
                    }
                    else
                    {
                        if (_destroyable != null)
                        {
                            if (_destroyable.gameObject == Activities_Menu.currentBackground.gameObject) { return; }
                            Destroy(_destroyable);
                        }

                        _background.DOFade(0, 0);
                        _destroyable = Activities_Menu.currentBackground.gameObject;

                        Activities_Menu.currentBackground?.SetParent(_background?.transform, true);
                        Activities_Menu.currentBackground?.GetComponent<RectTransform>()?.DOAnchorPos(Vector2.zero, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
                _canvasGroup?.FadeDown(duration);
            }
            _isOpenning = false;
        }

        public async Task HideLoadingLayerAsync()
        {
            try
            {
                if (_canvasGroup != null) { await _canvasGroup.FadeDown(0.25f).AsyncWaitForCompletion(); }
            }
            catch (Exception ex) { CustomLogger.instance?.LogException(ex); }
        }
    }
}