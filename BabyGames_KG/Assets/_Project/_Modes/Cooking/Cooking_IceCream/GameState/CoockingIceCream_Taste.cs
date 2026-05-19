using CustomAttributes;
using DG.Tweening;
using FX;
using Gameplay;
using Helpers;
using Identifiers;
using Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Coocking
{
    public class CoockingIceCream_Taste : CoockingIceCream_StateBase
    {
        public CoockingIceCream_Form form;
        public Panel_World panel;

        public HintHand_Drag hintHand_Drag;

        [SerializeField, Fg_De] private PlacesHolder formPlace;
        [SerializeField, Fg_De] private Coocking_IceCream currentForm;
        [SerializeField, Fg_De] private List<Transform> formPlacePositions_Copy = new();
        [SerializeField, Fg_De] private int currentPositionIndex = 0;
        [SerializeField, Fg_De] private List<Dropable_Basic> placed = new();

        public override async Task Enter()
        {
            await base.Enter();

            currentForm = form.currentSelectable.GetComponent<Coocking_IceCream>();
            formPlace = currentForm.iceCreamBallPositions;

            panel.PrepareForAnimation(currentForm.panelItems);
            panel.AnimateItems();

            formPlacePositions_Copy.AddRange(formPlace.secondary);
            formPlace.secondary.Clear();
            formPlace.secondary.Add(formPlacePositions_Copy.First());
            foreach (var item in currentForm.iceCreamBalls)
            {
                item.mode = Dropable_Basic.Dropable_Basic_Mode.Copy;
                item.GetComponentInChildren<SpriteRenderer>().sortingOrder += 100;
                item.targetPosition = formPlace;

                item.onPointeUpStart += OnPointeUpStart;
                item.onPointeUpEnd += OnPointeUpEnd;
                item.onCopyFinish += OnDropped;

                if (item.GetComponentInParent<MeshRenderer>() is MeshRenderer meshRenderer)
                {
                    meshRenderer.enabled = false;
                }

                item.onCopyCreated += (x, y) =>
                {
                    var toRemove = formPlacePositions_Copy.First();
                    x.transform.DOScale(toRemove.transform.localScale / item.transform.parent.localScale.x, 0.25f);
                    x.transform.DORotate(toRemove.transform.eulerAngles, 0.25f);
                };

                var initialScale = item.transform.localScale;
                var initialRotation = item.transform.eulerAngles;
                item.onRestore += (x) =>
                {
                    x.transform.DOScale(initialScale, 0.25f);
                    x.transform.DORotate(initialRotation, 0.25f);
                };
            }

            try
            {
                hintHand_Drag = GetComponent<HintHand_Drag>();
                hintHand_Drag?.SetIsActive(true);

                hintHand_Drag.objects.Clear();
                hintHand_Drag.targets.Clear();
                hintHand_Drag.objects.AddRange(currentForm.iceCreamBalls.Select(x => x.transform));
                hintHand_Drag.targets.AddRange(currentForm.iceCreamBalls.Select(x => x.targetPosition.secondary.GetRandom()));

                var toRemove = formPlacePositions_Copy.First().Find("TasteHint");
                toRemove.gameObject.SetActive(true);
            } catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }
        }

        public override async Task Exit()
        {
            foreach (var item in currentForm.iceCreamBalls)
            {
                item.onPointeUpStart -= OnPointeUpStart;
                item.onPointeUpEnd -= OnPointeUpEnd;
                item.onCopyFinish -= OnDropped;
                item.boxCollider.enabled = false;
            }

            await base.Exit();
        }

        private void OnPointeUpStart(Dropable_Basic dropable)
        {
            dropable.mode = Dropable_Basic.Dropable_Basic_Mode.Standart;

            try
            {
                if (formPlacePositions_Copy.TrueForAll(x => x.GetComponent<SpriteRenderer>() == null))
                {
                    dropable.GetComponentInChildren<SpriteRenderer>().sortingOrder = formPlacePositions_Copy.Count;
                }
            } catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }
        }

        private async void OnPointeUpEnd(Dropable_Basic dropable)
        {
            try
            {
                dropable.mode = Dropable_Basic.Dropable_Basic_Mode.Copy;

                if (dropable?.copy == null) { return; }
                if (dropable.CanPlace() == false)
                {
                    var copy = dropable.copy;
                    await AsyncHelper.DelayFloat(1);
                    if (placed.Contains(copy) == false) { copy.transform.DOScale(0, 0.25f); }
                }
            } catch (Exception e)
            {
                CustomLogger.instance?.LogException(e);
            }
        }

        private async void OnDropped(Dropable_Basic dropable, Dropable_Basic copy)
        {
            var toRemove = formPlacePositions_Copy.First();
            toRemove?.gameObject?.SetActive(true);

            var hintGameobject = toRemove.Find("TasteHint");
            hintGameobject?.gameObject?.SetActive(false);

            formPlacePositions_Copy.Remove(toRemove);
            copy.transform.parent = toRemove.transform;

            if (copy.transform.parent.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
            {
                var sortingOrderToSet = spriteRenderer.sortingOrder;
                var copySpriteRenderer = copy.GetComponentInChildren<SpriteRenderer>();
                copySpriteRenderer.sortingOrder = sortingOrderToSet;
            }

            dropable.transform.localScale = Vector3.one;
            dropable.transform.localEulerAngles = Vector3.zero;

            copy.transform.localScale = Vector3.one;
            copy.transform.localEulerAngles = Vector3.zero;
            placed.SafeAdd(copy);

            if (formPlacePositions_Copy.Count > 0)
            {
                dropable.boxCollider.enabled = true;

                formPlace.secondary.Clear();
                formPlace.secondary.Add(formPlacePositions_Copy.First());

                hintGameobject = formPlacePositions_Copy.First().Find("TasteHint");
                hintGameobject?.gameObject?.SetActive(true);
            }
            else
            {
                foreach (var item in currentForm.iceCreamBalls) { item.boxCollider.enabled = false; }
                await panel.HideItems();
                _nextState = _nextStateOnWin;
            }
        }
    }
}