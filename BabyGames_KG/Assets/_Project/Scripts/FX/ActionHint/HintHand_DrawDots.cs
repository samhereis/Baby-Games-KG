using Agents;
using DG.Tweening;
using Gameplay;
using Helpers;
using Loggers;
using Modes.Puzzle;
using System;
using System.Collections.Generic;
using _Project._Modes.Puzzle.Scripts;
using PaintCore;
using PaintIn2D;
using Sirenix.OdinInspector;
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

        private Transform _drawableCopy;
        private DrawableP2D _copyDrawable;
        private CwPaintableTexture _copyTexture;

        private Vector3 GetSourcePosition()
        {
            var target = sources.GetRandom();
            if (target is not RectTransform) return Camera.main.WorldToScreenPoint(target.position);
            return target.position;
        }

        public void SetIsActive(bool isActive) => _isActive = isActive;

        private void Awake()
        {
            if (animationAgent == null) animationAgent = PlayerActions_DataHolder.instance.hintVisual_HandWithAnimation;
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
                if (isShowing) return;
                Show();
            }
            else
            {
                Clear();
                Hide();
            }
        }

        public async void Show()
        {
            if (isShowing) return;
            isShowing = true;

            try
            {
                if (drawable != null)
                {
                    _drawableCopy = Instantiate(drawable);
                    await AsyncHelper.NextFrame();

                    _copyDrawable = _drawableCopy.GetComponentInChildren<DrawableP2D>();
                    if (_copyDrawable == null) { return; }
                    _copyDrawable.isActive = false;
                    _copyDrawable.ignoreFinger = true;
                    if (_copyDrawable.TryGet<CwPaintableSprite>(out var ps)) { ps.enabled = false; }
                    _copyTexture = _copyDrawable.PaintableSpriteTexture;

                    foreach (var sr in _drawableCopy.GetComponentsInChildren<SpriteRenderer>(true))
                    {
                        sr.sortingLayerName = "AlwaysOnTop";
                    }

                    outline?.DOLocalMoveX(0, 0);
                    outline?.gameObject?.SetActive(true);
                    outline?.DOScale(1.4f, 0.25f);
                }

                var position = GetSourcePosition();
                position.z = 0;
                animationAgent.transform.position = position;
                animationAgent.transform.localScale = Vector3.zero;
                animationAgent.gameObject.SetActive(true);

                await animationAgent.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack).AsyncWaitForCompletion();
                await animationAgent.transform.DOMove(position, 0.5f).SetEase(Ease.OutBack).SetDelay(1f).AsyncWaitForCompletion();
                await animationAgent.transform.DOScale(0.5f, 0.25f).SetEase(Ease.OutBack).SetDelay(0.25f).AsyncWaitForCompletion();
                await animationAgent.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack).SetDelay(0.25f).AsyncWaitForCompletion();

                foreach (var point in targets.GetRandom().secondary)
                {
                    if (!isShowing) break;

                    var tween = animationAgent.transform.DOMove(Camera.main.WorldToScreenPoint(point.position), moveToDotDuration);
                    tween.OnUpdate(() =>
                    {
                        if (!isShowing)
                        {
                            tween.Complete();
                            return;
                        }
                        Vector2 worldPos = Camera.main.ScreenToWorldPoint(animationAgent.transform.position);
                        _copyDrawable?.DrawAtMousePosition(worldPos, _copyTexture);
                    });

                    await tween.AsyncWaitForCompletion();
                }
            } catch (Exception ex) { CustomLogger.instance?.LogException(ex); }

            PlayerActions_DataHolder.ResetTime();
            Clear();
        }

        private async void Clear()
        {
            isShowing = false;
            PlayerActions_DataHolder.ResetTime();

            var copy = _drawableCopy;
            _drawableCopy = null;
            _copyDrawable = null;
            _copyTexture = null;

            try
            {
                AsyncHelper.DoDelayed(async () =>
                {
                    if (animationAgent != null)
                    {
                        animationAgent.transform.DOKill();
                        await animationAgent.transform.DOScale(0, 0.25f).SetEase(Ease.OutBack).SetDelay(0.25f).AsyncWaitForCompletion();
                        animationAgent.gameObject.SetActive(false);
                    }
                }, 0);

                AsyncHelper.DoDelayed(async () =>
                {
                    if (copy != null)
                    {
                        copy.transform.localScale = Vector3.zero;
                        copy.GetComponentInChildren<CwPaintableTexture>()?.Deactivate();
                        copy.gameObject.SetActive(false);
                        Destroy(copy.gameObject);
                    }

                    outline?.DOScale(0, 0.25f).OnComplete(() => outline?.gameObject?.SetActive(false));
                }, 0);

            } catch (Exception ex) { CustomLogger.instance?.LogException(ex); }
            isShowing = false;
        }

        [Button]
        private void Hide()
        {
            isShowing = false;
            if (!animationAgent.gameObject.activeSelf) return;
            animationAgent.transform.DOKill();
            animationAgent.gameObject.SetActive(false);
        }
    }
}