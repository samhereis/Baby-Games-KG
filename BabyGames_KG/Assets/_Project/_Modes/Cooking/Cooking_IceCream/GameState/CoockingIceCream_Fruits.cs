using DG.Tweening;
using FX;
using Helpers;
using Identifiers;
using Loggers;
using System;
using System.Linq;
using System.Threading.Tasks;
using DataClasses;
using Services;
using Sounds;
using UnityEngine;
using Zenject;

namespace Coocking
{
    public class CoockingIceCream_Fruits : CoockingIceCream_StateBase
    {
        public CoockingIceCream_Form form;

        public HintHand_Drag hintHand_Drag;
        public SoundQueue _yummySound;
        [Inject] private ISoundPlayer _soundPlayer;

        private bool didPrepare = false;

        public override async Task Disable()
        {
            if (didPrepare == true) { return; }

            foreach (var form in form.droppables)
            {
                var iceCream = form.GetComponent<Coocking_IceCream>();

                try
                {
                    foreach (var item in iceCream.fruitPositions)
                    {
                        foreach (var pos in item.secondary)
                        {
                            await item.GetComponent<SpriteRenderer>().DOFade(0, 0).AsyncWaitForCompletion();
                            item.GetComponent<SpriteRenderer>().enabled = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    CustomLogger.instance?.LogException(ex);
                }
            }

            await base.Disable();
            didPrepare = true;
        }

        public override async Task Enter()
        {
            await base.Enter();
            DiService.Inject(this);

            for (int i = 0; i < form.currentForm.fruits.Count; i++)
            {
                form.currentForm.fruits[i].targetPosition = form.currentForm.fruitPositions[i];
            }

            foreach (var item in form.currentForm.fruits)
            {
                foreach (var spriteRenderer in item.targetPosition.secondary)
                {
                    spriteRenderer.GetComponent<SpriteRenderer>().enabled = true;
                    spriteRenderer.GetComponent<SpriteRenderer>().DOFade(1, 0.25f);
                }
                    
                item.onStartDrag += (x) =>
                {
                    var toRemove = item.targetPosition.secondary.First();
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
                
                item.onFinish += OnItemDroped;
            }

            _controller.panel_World.PrepareForAnimation(form.currentForm.fruits.Select(x => x.GetComponent<PanelItem>()).ToList());
            await _controller.panel_World.Appear();
            _controller.panel_World.AnimateItems();

            try
            {
                hintHand_Drag = GetComponent<HintHand_Drag>();
                hintHand_Drag?.SetIsActive(true);

                if (hintHand_Drag)
                {
                    hintHand_Drag.objects.Clear();
                    hintHand_Drag.targets.Clear();
                    hintHand_Drag.objects.AddRange(form.currentForm.fruits.Select(x => x.transform));
                    hintHand_Drag.targets.AddRange(
                        form.currentForm.fruits.Select(x => x.targetPosition.secondary.GetRandom()));
                }
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }
        }

        private void OnItemDroped(Dropable_Basic basic)
        {
            var lastPosition = basic.targetPosition.lasNearestPosition;

            if (basic.GetComponentInChildren<SpriteRenderer>() is SpriteRenderer spriteRenderer)
            {
                if (lastPosition.transform.GetComponentInChildren<SpriteRenderer>() is SpriteRenderer lastPositionSpriteRenderer)
                {
                    var sortingOrderToSet = lastPositionSpriteRenderer.sortingOrder;
                    spriteRenderer.sortingOrder = sortingOrderToSet;

                    lastPositionSpriteRenderer.DOFade(1, 0.25f).OnComplete(() =>
                    {
                        lastPositionSpriteRenderer.enabled = false;
                    });
                }

                spriteRenderer.sortingLayerName = lastPosition.GetComponent<SpriteRenderer>().sortingLayerName;
                spriteRenderer.sortingOrder = lastPosition.GetComponent<SpriteRenderer>().sortingOrder;
                basic.transform.DOScale(lastPosition.transform.localScale, 1);
                basic.transform.DORotate(lastPosition.transform.eulerAngles, 1);
            }

            basic.targetPosition.secondary.Remove(lastPosition);

            if (form.currentForm.fruits.TrueForAll(x => x.isDropped.value == true))
            {
                _soundPlayer.TryPlay(_yummySound);
                _model.onFinish?.Invoke();
            }
        }
    }
}