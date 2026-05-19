using _Project._Modes.Hiding.Scripts.State;
using _Project.Scripts.Sound;
using CustomAttributes;
using DG.Tweening;
using GameState;
using Helpers;
using Hiding;
using Identifiers;
using Loggers;
using Sirenix.OdinInspector;
using Spine.Unity;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Modes.Sorting
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(Move_FX))]
    [RequireComponent(typeof(Sound_FX))]
    public class HidingDraggable_Identifier : IdentifierBase, ISelfValidator, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        public enum DragMode { Manual, Automatic }

        public DragMode dragMode = DragMode.Automatic;
        public SkeletonGraphic skeletonGraphic;

        [Space]
        public Vector3 appearPosition;

        [Space]
        public Vector3 pannelScale = Vector3.one * 0.25f;
        public Vector3 panelAnchoredPosition = Vector3.zero;
        public string inPannelAnimation = "InAquarium";

        [SerializeField, Fg_Co] private RectTransform _rectTransform;
        [SerializeField, Fg_Co] private CanvasGroup _canvasGroup;
        [SerializeField, Fg_Co] private Canvas _canvas;

        [SerializeField, Fg_De] private Vector3 _initialAnchoredPosition;

        [Space]
        [SerializeField, FoldoutGroup("FX")] private Move_FX _move_FX;
        [SerializeField, FoldoutGroup("FX")] private Sound_FX _sound_FX;
        [FoldoutGroup("FX")] public float dropParticleScale = 1;

        private HidingWaveUnit _waveUnit;
        private Hiding_GameState_Model _model;

        private bool _isCompleted;

        private bool IsNear()
        {
            MainMenu_GameState_Model.selectedActivityCategory.TryGetSetting_Float("hiding_dropDIstance", 1, out var hiding_dropDIstance);

            var ourPosition = transform.position;
            var targetPosition = _waveUnit.dropZone.transform.position;

            ourPosition.z = 0;
            targetPosition.z = 0;

            return Vector3.Distance(ourPosition, targetPosition) < hiding_dropDIstance;
        }

        public void Construct(HidingWaveUnit waveUnit, Hiding_GameState_Model model)
        {
            _waveUnit = waveUnit;
            Construct(model);
        }

        public void Construct(Hiding_GameState_Model model)
        {
            _model = model;
        }

        private void Awake()
        {
            if (skeletonGraphic == null) { skeletonGraphic = Get<SkeletonGraphic>(); }

            _canvas.sortingLayerName = "Background";
            _initialAnchoredPosition = _rectTransform.anchoredPosition;

            skeletonGraphic.AnimationState.ClearTracks();
            skeletonGraphic.AnimationState.SetAnimation(0, "idle", true);

            _move_FX = GetComponent<Move_FX>();
            _sound_FX = GetComponent<Sound_FX>();
        }

        private void OnDestroy()
        {
            _canvasGroup.DOKill();
        }

        public void Validate(SelfValidationResult result)
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvas = GetComponent<Canvas>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_isCompleted == true) { return; }

            _rectTransform.DOKill();
            _canvasGroup.blocksRaycasts = false;

            _canvas.sortingLayerName = "Front";

            OnClick();

            if (_sound_FX != null) { _sound_FX?.Play(Sound_Effect.StartDrag); }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_isCompleted == true) { return; }

            Vector3 positon = eventData.position;
            positon = Camera.main.ScreenToWorldPoint(positon);
            positon.z = 0;

            _rectTransform.position = positon;
        }

        public async void OnEndDrag(PointerEventData eventData)
        {
            if (_isCompleted == true) { return; }
            if (_waveUnit == null)
            {
                ResetPosition();
                return;
            }
            if (_waveUnit.dropZone == null)
            {
                ResetPosition();
                return;
            }

            if (IsNear())
            {
                _isCompleted = true;

                try
                {
                    var clip = await _sound_FX.PlayAsync(Sound_Effect.Success);
                    _move_FX.Success();
                    Move_FX.MakeDoneParticle(_waveUnit.dropZone.transform.position, dropParticleScale);

                    transform.DOMove(_waveUnit.dropZone.transform.position, 0.25f);
                    await transform.DOScale(0.25f, 0.25f).OnComplete(() =>
                    {
                        transform.DOScale(0, 0);
                    }).AsyncWaitForCompletion();
                } catch (Exception ex)
                {
                    CustomLogger.instance?.LogException(ex);
                }
                _waveUnit.OnCompleted();
            }
            else
            {
                var clip = await _sound_FX.PlayAsync(Sound_Effect.Failure);
                _move_FX.Fail();
                await AsyncHelper.DelayFloat(clip.length);

                ResetPosition();
            }
        }

        public async void OnPointerClick(PointerEventData eventData)
        {
            if (_isCompleted == true) { return; }
            if (_waveUnit == null)
            {
                ResetPosition();
                return;
            }
            if (_waveUnit.dropZone == null)
            {
                ResetPosition();
                return;
            }

            _isCompleted = true;

            Get<GraphicRaycaster>().enabled = false;

            await _rectTransform.DOAnchorPos(appearPosition, 0.5f).SetEase(Ease.OutBack).AsyncWaitForCompletion();
            _canvas.sortingOrder = 999;
            _canvas.sortingLayerName = "Front";

            OnClick();

            try
            {
                await _rectTransform.DOMove(_waveUnit.dropZone.skeletonGraphicRect.position, 1).AsyncWaitForCompletion();
                var clip = await _sound_FX.PlayAsync(Sound_Effect.Success);
                _move_FX.Success();
                await transform.DOScale(0.25f, 0.25f).OnComplete(async () =>
                {
                    transform.DOScale(0, 0);
                }).AsyncWaitForCompletion();
            } catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }

            _waveUnit.OnCompleted();
        }

        private void OnClick()
        {
            _model.click.GetComponent<RectTransform>().anchoredPosition = _rectTransform.anchoredPosition;
            _model.click.AnimationState.ClearTracks();
            _model.click.AnimationState.SetAnimation(0, "action", false);
        }

        private void ResetPosition()
        {
            _rectTransform.DOAnchorPos(_initialAnchoredPosition, 0.5f).SetEase(Ease.OutBack);
            _canvasGroup.blocksRaycasts = true;

            _canvas.sortingLayerName = "Background";
        }

        public void SetVisibility(bool isVisible)
        {
            _canvasGroup.blocksRaycasts = isVisible;
            _canvasGroup.DOFade(isVisible.ToInt(), 1f);
        }
    }
}