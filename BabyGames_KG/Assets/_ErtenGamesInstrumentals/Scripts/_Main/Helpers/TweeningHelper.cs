using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System;
using Loggers;
using UnityEngine;

namespace Helpers
{
    public static class TweeningHelper
    {

        #region Transform
        public static Tweener NormalShake(this Transform obj, float duration, float strenght = 10)
        {
            return obj.DOShakePosition(duration, strenght, 10, 50, false, true).SetUpdate(true);
        }

        public static Tweener ScaleDown(this Transform obj, float duration, Ease ease = Ease.OutBack)
        {
            return obj.ScaleTo(0, duration, ease);
        }

        public static Tweener ScaleTo(this Transform obj, float value, float duration, Ease ease = Ease.OutBack)
        {
            return obj.DOScale(value, duration).SetEase(ease).SetUpdate(true);
        }

        public static Tweener ScaleUp(this Transform obj, float duration, Ease ease = Ease.OutBack)
        {
            return obj.ScaleTo(1, duration, ease);
        }

        #endregion

        #region Number

        public static TweenerCore<float, float, FloatOptions> TweenFloat(this float value, float to, float duration, Action<float> onUpdateCallback = null, Action<float> completedCallback = null)
        {
            return DOTween.To(() => value, x => value = x, to, duration).
                OnUpdate(() =>
                {
                    onUpdateCallback?.Invoke(value);
                }).
                OnComplete(() =>
                {
                    completedCallback?.Invoke(value);
                }).
                SetUpdate(true);
        }

        public static TweenerCore<int, int, NoOptions> TweenInt(this int value, int to, float duration, Action<int> onUpdateCallback = null, Action<float> completedCallback = null)
        {
            return DOTween.To(() => value, x => value = x, to, duration).
                OnUpdate(() =>
                {
                    onUpdateCallback?.Invoke(value);
                }).
                OnComplete(() =>
                {
                    completedCallback?.Invoke(value);
                }).
                SetUpdate(true);
        }

        #endregion

        #region Fade

        public static void FadeDownQuick(this CanvasGroup obj, bool setUninteractable = true, bool setActiveToFalse = false)
        {
            if (setUninteractable)
            {
                obj.interactable = false;
                obj.blocksRaycasts = false;
            }

            if (setActiveToFalse) obj.gameObject.SetActive(false);

            obj.alpha = 0;
        }

        public static void FadeUpQuick(this CanvasGroup obj)
        {
            obj.interactable = true;
            obj.blocksRaycasts = true;
            obj.alpha = 1;
        }

        public static Tweener FadeDown(this CanvasGroup obj, float duration = 0.5f, bool setUninteractable = true, Ease ease = Ease.OutBack, bool setActiveToFalse = false, Action completeCallback = null)
        {
            if (obj == null) return null;

            if (setUninteractable)
            {
                obj.interactable = false;
                obj.blocksRaycasts = false;
            }

            if (setActiveToFalse) obj.gameObject.SetActive(false);

            return obj.FadeTo(0, duration, ease).OnComplete(() =>
            {
                if (setUninteractable)
                {
                    obj.interactable = false;
                    obj.blocksRaycasts = false;
                }

                completeCallback?.Invoke();
            }); ;
        }

        public static Tweener FadeTo(this CanvasGroup obj, float value, float duration, Ease ease = Ease.OutBack)
        {
            if (obj == null) return null;
            return obj?.DOFade(value, duration).SetEase(ease).SetUpdate(true);
        }

        public static Tweener FadeUp(this CanvasGroup obj, float duration = 0.5f, bool setInteractable = true, Ease ease = Ease.OutBack, bool setActiveToTrue = true, Action completeCallback = null)
        {
            if (obj == null) return null;

            if (setInteractable)
            {
                obj.interactable = true;
                obj.blocksRaycasts = true;
            }

            if (setActiveToTrue) obj.gameObject.SetActive(true);

            return obj.FadeTo(1, duration, ease).OnComplete(() =>
            {
                if (setInteractable)
                {
                    obj.interactable = true;
                    obj.blocksRaycasts = true;
                }

                completeCallback?.Invoke();
            });
        }

        public static Tweener FadeWithActiveStatus(this SpriteRenderer spriteRenderer, bool toShow, float duration = 0.5f)
        {
            try
            {
                spriteRenderer.DOKill();
            
                if (toShow)
                {
                    spriteRenderer.gameObject.SetActive(true);
                    return spriteRenderer.DOFade(1, duration);
                }
                else
                {
                    return spriteRenderer.DOFade(0, duration).OnComplete(() =>
                    {
                        spriteRenderer.gameObject.SetActive(false);
                    });
                }
            }
            catch (Exception e)
            {
                CustomLogger.instance?.LogException(e);
                return null;
            }
        }

        #endregion
    }
}