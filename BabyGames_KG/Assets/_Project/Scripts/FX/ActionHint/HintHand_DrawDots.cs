using Agents;
using DG.Tweening;
using Gameplay;
using Helpers;
using Loggers;
using Modes.Puzzle;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FX
{
    public class HintHand_DrawDots : MonoBehaviour
    {
        public AnimationAgent animationAgent;
        public List<Transform> sources = new();
        public List<PlacesHolder> targets = new();
        public Transform outline;
        public Transform drawable;

        public float hintEvery_Seconds = 2;
        public bool isShowing;

        [Space]
        public float moveToDotDuration = 1;

        [SerializeField] private bool _isActive = false;

        private Vector3 GetSourcePosition()
        {
            var target = sources.GetRandom();

            if (target is not RectTransform) { return Camera.main.WorldToScreenPoint(target.position); }
            else { return target.position; }
        }

        public void SetIsActive(bool isActive)
        {
            this._isActive = isActive;
        }

        private void Awake()
        {
            if (animationAgent == null) { animationAgent = PlayerActions_DataHolder.instance.hintVisual_HandWithAnimation; }
        }

        private void OnDisable()
        {
            animationAgent.gameObject.SetActive(false);
            animationAgent.transform.DOKill();
        }

        public void Update()
        {
            if (_isActive && PlayerActions_DataHolder.instance.timeSinceLastAction > hintEvery_Seconds)
            {
                if (isShowing == true) { return; }

                Show();
            }
            else
            {
                Hide();
            }
        }

        Transform _drawableCopy = null;
        Vector3? _initialScale = null;
        public async void Show()
        {
            if (isShowing == true) { return; }
            if (_drawableCopy != null)
            {
                Destroy(_drawableCopy.gameObject);
                if (_initialScale != null) { drawable.transform.localScale = _initialScale.Value; }
                else { drawable.transform.localScale = Vector3.one; }

                ResetState();
                return;
            }
            isShowing = true;

            try
            {
                if (drawable != null)
                {
                    _drawableCopy = Instantiate(drawable);
                    await AsyncHelper.NextFrame();

                    if (_initialScale == null) { _initialScale = drawable.transform.localScale; }
                    drawable.transform.localScale = Vector3.zero;

                    if (_drawableCopy.TryGetComponent<MeshRenderer>(out var meshRenderer)) { meshRenderer.sortingOrder++; }
                    _drawableCopy.GetComponentInChildren<SpriteRenderer>().sortingOrder++;
                    _drawableCopy.GetComponentInChildren<Drawable>().Initialize();
                    _drawableCopy.GetComponentInChildren<Drawable>().ignoreFinger = true;

                    foreach (var item in _drawableCopy.GetComponentsInChildren<MeshRenderer>())
                    {
                        item.sortingLayerName = "AlwaysOnTop";
                    }

                    foreach (var item in _drawableCopy.GetComponentsInChildren<SpriteRenderer>())
                    {
                        item.sortingLayerName = "AlwaysOnTop";
                    }

                    outline?.DOLocalMoveX(0, 0);
                    outline?.gameObject?.SetActive(true);
                    outline?.DOScale(1.4f, 0.25f);
                }

                var position = GetSourcePosition();
                position.z = 0;
                animationAgent.transform.transform.position = position;
                animationAgent.transform.transform.localScale = Vector3.zero;

                animationAgent.transform.gameObject.SetActive(true);
                var tween = animationAgent.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
                await tween.AsyncWaitForCompletion();

                tween = animationAgent.transform.DOMove(position, 0.5f).SetEase(Ease.OutBack).SetDelay(1f);
                await tween.AsyncWaitForCompletion();

                tween = animationAgent.transform.DOScale(0.5f, 0.25f).SetEase(Ease.OutBack).SetDelay(0.25f);
                await tween.AsyncWaitForCompletion();

                tween = animationAgent.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack).SetDelay(0.25f);
                await tween.AsyncWaitForCompletion();

                foreach (var item in targets.GetRandom().secondary)
                {
                    if (_isActive == false || isShowing == false)
                    {
                        animationAgent.transform.DOKill();
                        ResetState();
                        return;
                    }

                    tween = animationAgent.transform.DOMove(Camera.main.WorldToScreenPoint(item.position), moveToDotDuration);
                    tween.OnUpdate(() =>
                    {
                        Vector2 mouseWorldPositioni = Camera.main.ScreenToWorldPoint(animationAgent.transform.position);
                        _drawableCopy?.GetComponentInChildren<Drawable>().DrawAtMousePosition(mouseWorldPositioni);

                        if (_isActive == false || isShowing == false)
                        {
                            animationAgent.transform.DOKill();
                            ResetState();
                            tween.Complete();
                            return;
                        }
                    });

                    await tween.AsyncWaitForCompletion();
                }

                ResetState();

            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }

            PlayerActions_DataHolder.ResetTime();
            isShowing = false;
        }

        private async void ResetState()
        {
            PlayerActions_DataHolder.ResetTime();
            isShowing = false;

            var copy = _drawableCopy;

            if (animationAgent != null)
            {
                animationAgent.transform.DOKill();

                await animationAgent.transform.DOScale(0, 0.25f).SetEase(Ease.OutBack).SetDelay(0.25f).AsyncWaitForCompletion();
                animationAgent.gameObject.SetActive(false);
            }

            try
            {
                copy?.GetComponentInChildren<SpriteRenderer>().DOFade(0, 0.25f).OnComplete(() =>
                {
                    Destroy(copy.gameObject);
                });

                outline?.DOScale(0, 0.25f).OnComplete(() =>
                {
                    outline?.gameObject?.SetActive(false);
                });
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }

            if (drawable != null)
            {
                drawable.transform.localScale = _initialScale.Value;
            }
        }

        private void Hide()
        {
            if (animationAgent.gameObject.activeSelf == false) { return; }
            animationAgent.gameObject.SetActive(false);
            animationAgent.transform.DOKill();
            isShowing = false;
        }
    }
}