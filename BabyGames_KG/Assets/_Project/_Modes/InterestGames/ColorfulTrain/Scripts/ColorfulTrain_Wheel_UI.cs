using _Project.Scripts.Sound;
using DG.Tweening;
using Helpers;
using Loggers;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ColorfulTrain
{
    //TODO: delete 15.05.2026
    public class ColorfulTrain_Wheel_UI : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
    {
        public CanvasGroup canvasGroup;
        public RectTransform holder;
        public Image image;

        public Vector3 initialScale = new(1f, 1f, 1f);
        public Vector3 dragScale = new(0.25f, 0.25f, 0.25f);
        public Vector3 dropScale = new(0.75f, 0.75f, 0.75f);
        public float dropDistance = 2;
        public bool autoPlace = true;

        [Space]
        public Move_FX move_FX;

        private ColorfulTrain_Wheel _seat;

        [Button]
        public void Validate()
        {
            initialScale = holder.localScale;
        }

        public void Construct(ColorfulTrain_Wheel seat)
        {
            _seat = seat;
            image.sprite = _seat.spriteRenderer.sprite;
        }

        private void OnEnable()
        {
            holder.localScale = dragScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (canvasGroup.interactable == false) { return; }
            PlayerActions_DataHolder.ResetTime();

            Sound_FX.Play_Static(Sound_Effect.StartDrag);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (canvasGroup.interactable == false) { return; }
            holder.position = eventData.position;

            if (autoPlace == CanPlace(eventData))
            {
                TryPlace(eventData);
            }
        }

        public async void OnEndDrag(PointerEventData eventData)
        {
            if (canvasGroup.interactable == false) { return; }
            _seat.canDrag = false;
            canvasGroup.interactable = false;

            if (CanPlace(eventData))
            {
                TryPlace(eventData);
            }
            else
            {
                try
                {
                    //var clip = await _seat._soundFX?.PlayAsync(Sound_Effect.Failure);
                    //move_FX?.Fail(holder, clip.length);
                    //await AsyncHelper.DelayFloat(clip.length);
                    //
                    //_seat._soundFX?.Play(Sound_Effect.MoveToTarget_Fast);
                }
                catch (Exception ex)
                {
                    CustomLogger.instance?.LogException(ex);
                }

                holder.DOKill();
                holder.DOAnchorPos(Vector3.zero, 0.25f).SetEase(Ease.OutBack);
                await holder.DOScale(initialScale, 0.25f).SetEase(Ease.OutBack).AsyncWaitForCompletion();

                await AsyncHelper.DelayFloat(0.5f);
                _seat.canDrag = true;
                canvasGroup.interactable = true;
            }
        }

        private bool CanPlace(PointerEventData eventData)
        {
            var worlPos = Camera.main.ScreenToWorldPoint(eventData.position);
            worlPos.z = 0;

            var distance = Vector3.Distance(worlPos, _seat.iconPosition);
            return distance < dropDistance;
        }

        private async void TryPlace(PointerEventData eventData)
        {
            if (CanPlace(eventData))
            {
                canvasGroup.interactable = false;

                var clip = await _seat._soundFX?.PlayAsync(Sound_Effect.Success);

                holder.DOKill();
                holder.DOMove(Camera.main.WorldToScreenPoint(_seat.holder.position), clip.length).SetEase(Ease.OutBack);
                holder.DOScale(dropScale, clip.length).SetEase(Ease.OutBack);

                _seat.PlaceToTrain(false);
                canvasGroup.FadeDown();
            }
        }
    }
}