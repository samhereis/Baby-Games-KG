using DG.Tweening;
using FX;
using Gameplay;
using Loggers;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Coocking
{
    public class CoockingBurger_Ingredients : CoockingBurger_StateBase, ISelfValidator
    {
        public Transform burgetTarget;
        public Transform podnosTarget;

        public Transform burgerTransform;
        public Transform podnos;
        public Transform plate;

        public List<DroppableGeneral_SimpleController> droppables = new();
        public List<Dropable_General> items = new();
        public Transform droppablesHolder;
        public PlacesHolder placesHolder;

        public SpriteRenderer hintSingle;

        private static List<SpriteRenderer> _hintCopies = new();

        [Space]
        public HintHand_Drag _hintHand_Drag;

        public void Validate(SelfValidationResult result)
        {
            items = droppables.Select(x => x._dropable).ToList();
        }

        public override async Task Disable()
        {
            await base.Disable();

            foreach (var item in _hintCopies)
            {
                try { Destroy(item.gameObject); } catch (Exception ex) { CustomLogger.instance?.LogException(ex); }
            }

            if (_isDone == false) { droppablesHolder.DOMoveY(25, 0); }
            else { hintSingle.transform.DOScale(0, 1); }
        }

        public override async Task Enter()
        {
            await base.Enter();

            if (_hintHand_Drag == null) { _hintHand_Drag = GetComponent<HintHand_Drag>(); }

            podnos.DOMove(podnosTarget.position, 1);
            podnos.DOScale(podnosTarget.localScale, 0.25f).SetEase(Ease.InOutBack);
            podnos.DORotate(podnosTarget.eulerAngles, 0.25f).SetEase(Ease.InOutBack);

            droppablesHolder.DOMove(Vector3.zero, 1);
            burgerTransform.DOMove(burgetTarget.position, 1);

            items = droppablesHolder.GetComponentsInChildren<Dropable_General>(true).ToList();
            foreach (var item in items)
            {
                item.hasDropped.RemoveListener(OnIngredientDropped);
                item.hasDropped.AddListener(OnIngredientDropped);

                item.onDropStart -= OnDrop;
                item.onDropStart += OnDrop;

                item.objectJuicer.StartJamming();
            }

            plate.DOMove(burgetTarget.position, 1);

            if (items.Count < 2)
            {
                var drop = items[0];

                hintSingle.sprite = drop.spriteRenderer.sprite;
                hintSingle.transform.localPosition = drop.placesHolder.secondary[0].localPosition;
                hintSingle.transform.eulerAngles = drop.transform.eulerAngles;
                hintSingle.sortingOrder = drop.spriteRenderer.sortingOrder - 1;

                hintSingle.transform.DOScale(drop.transform.localScale, 1);
            }
            else
            {
                int index = 0;
                foreach (var item in items)
                {
                    var copy = Instantiate(hintSingle, hintSingle.transform.parent);
                    var position = item.placesHolder.secondary[index];

                    copy.sprite = item.spriteRenderer.sprite;
                    copy.transform.localPosition = position.localPosition;
                    copy.transform.eulerAngles = item.transform.eulerAngles;
                    copy.sortingOrder = item.spriteRenderer.sortingOrder - 1;

                    copy.transform.DOScale(item.transform.localScale, 1);
                    copy.transform.SetParent(position, true);

                    _hintCopies.Add(copy);

                    index++;
                }
            }

            _hintHand_Drag.SetIsActive(true);
            _hintHand_Drag.objects.AddRange(items.Select(x => x.transform));
            _hintHand_Drag.targets.Add(plate);
        }

        public override async Task Exit()
        {
            await base.Exit();

            foreach (var item in items)
            {
                item.hasDropped.RemoveListener(OnIngredientDropped);
            }
        }

        private void OnDrop(Dropable_General controller)
        {
            controller.GetComponentInChildren<BoxCollider>().enabled = false;
        }

        private void OnIngredientDropped(bool obj)
        {
            try
            {
                foreach (var item in items)
                {
                    item.objectJuicer.StopJamming();

                    if (item.hasDropped.value)
                    {
                        item.GetComponentInChildren<BoxCollider>().enabled = false;
                        item.onDropStart -= OnDrop;

                        item.lasNearestPosition.DOScale(0, 0);
                        item.placesHolder.secondary.Remove(item.lasNearestPosition);
                    }
                }
            } catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }

            if (items.Count < 1 || items.TrueForAll(x => x.hasDropped.value))
            {
                _isDone = true;
                _nextState = _nextStateOnWin;
            }
        }
    }
}