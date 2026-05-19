using DG.Tweening;
using FX;
using Identifiers;
using Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Coocking
{
    public class CoockingIceCream_Sirop : CoockingIceCream_StateBase
    {
        public CoockingIceCream_Form form;

        public List<Dropable_Basic> sirops = new();
        public List<Transform> currentPosition = new();

        public Transform holder;
        public HintHand_Drag hintHand_Drag;

        public override async Task Enter()
        {
            await base.Enter();

            foreach (var item in sirops)
            {
                item.onStartDrag += OnStartDrag;
                item.onCopyFinish += OnFinsih;

                item.onCopyCreated += (x, y) =>
                {
                    var toRemove = currentPosition.First();
                    x.transform.DOScale(toRemove.transform.localScale * toRemove.transform.parent.localScale.x, 0.25f);
                    x.transform.DORotate(toRemove.transform.eulerAngles, 0.25f);
                };

                var initialScale = item.transform.localScale;
                var initialRotation = item.transform.eulerAngles;
                item.onRestore += (x) =>
                {
                    x.transform.DOScale(initialScale, 0.25f);
                    x.transform.DORotate(initialRotation, 0.25f);
                };

                item.doPunchAnimation = false;
            }

            holder.DOMoveX(0, 1);

            _controller.panel_World.PrepareForAnimation(sirops.Select(x => x.GetComponent<PanelItem>()).ToList());
            await _controller.panel_World.Appear();
            _controller.panel_World.AnimateItems();

            currentPosition.AddRange(form.currentForm.siropPosition.secondary);
            UpdateElements();
        }

        private void OnStartDrag(Dropable_Basic asd)
        {
            asd.targetPosition = form.currentForm.siropPosition;
        }

        private void UpdateElements()
        {
            try
            {
                form.currentForm.siropPosition.secondary.Clear();
                form.currentForm.siropPosition.secondary.Add(currentPosition.First());
                currentPosition.First().gameObject.SetActive(true);

                hintHand_Drag = GetComponent<HintHand_Drag>();
                hintHand_Drag?.SetIsActive(true);

                hintHand_Drag.objects.Clear();
                hintHand_Drag.targets.Clear();
                hintHand_Drag.objects.AddRange(sirops.Select(x => x.transform));
                hintHand_Drag.targets.Add(currentPosition.First());
            } catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }
        }

        private async void OnFinsih(Dropable_Basic dropable, Dropable_Basic copy)
        {
            if (copy == null) { return; }
            var lastPosition = dropable.targetPosition.lasNearestPosition;

            if (lastPosition.GetComponentInChildren<PanelItem>() is PanelItem panelItem)
            {
                panelItem.GetComponent<SpriteRenderer>().DOFade(0, 0.5f).OnComplete(() =>
                {
                    Destroy(panelItem.gameObject);
                });
            }

            if (lastPosition.GetComponentInChildren<SpriteRenderer>() is SpriteRenderer spriteRenderer)
            {
                spriteRenderer.DOFade(0, 0.25f).OnComplete(() => { spriteRenderer.enabled = false; });

                copy.GetComponentInChildren<SpriteRenderer>().sortingLayerID = spriteRenderer.sortingLayerID;
                copy.GetComponentInChildren<SpriteRenderer>().sortingLayerName = spriteRenderer.sortingLayerName;
                copy.GetComponentInChildren<SpriteRenderer>().sortingOrder = spriteRenderer.sortingOrder;

                copy.transform.SetParent(lastPosition);

                dropable.transform.localScale = dropable.GetComponent<PanelItem>().holderScale;
                dropable.transform.localEulerAngles = Vector3.zero;

                copy.transform.localScale = Vector3.one;
                copy.transform.localEulerAngles = spriteRenderer.transform.eulerAngles;
            }

            currentPosition.Remove(lastPosition);
            if (currentPosition.Count > 0)
            {
                dropable.boxCollider.enabled = true;
                UpdateElements();
            }
            else
            {
                Win();
            }
        }

        private async void Win()
        {
            await _controller.panel_World.HideItems();
            _nextState = _nextStateOnWin;
        }
    }
}