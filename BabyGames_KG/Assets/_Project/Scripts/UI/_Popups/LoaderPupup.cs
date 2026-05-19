using DG.Tweening;
using Helpers;
using Identifiers;
using Loggers;
using System;
using System.Threading;
using UnityEngine;

namespace UI.Helpers
{
    [RequireComponent(typeof(CanvasGroup))]
    public class LoaderPupup : IdentifierBase
    {
        [SerializeField] private float _lifetime = 10;
        [SerializeField] private CanvasGroup _canvasGroup;

        private CancellationToken _cancellationToken;

        private void Awake()
        {
            _cancellationToken = destroyCancellationToken;
        }

        private void OnDestroy()
        {
            _canvasGroup.DOKill();
        }

        public async void Open()
        {
            Get<RectTransform>().anchoredPosition = Vector2.zero;

            try
            {
                _canvasGroup?.DOFade(0.75f, 0.5f);

                await AsyncHelper.DelayFloat(_lifetime);
                if (_cancellationToken.IsCancellationRequested == false) { _canvasGroup?.FadeDown(); }
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }
        }

        public void Close()
        {
            try
            {
                _canvasGroup?.FadeDown();
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }
        }
    }
}