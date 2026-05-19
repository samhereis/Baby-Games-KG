using DG.Tweening;
using FX;
using Gameplay;
using Identifiers;
using Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Coocking
{
    public class CookingCake_Decoring_Berries : CoockingBurger_StateBase
    {
        public Transform holder;
        public Transform cake;

        public List<DroppableGeneral_SimpleController> droppable = new();
        public List<Dropable_General> items = new();

        public float itemsScale = 0.6f;

        [SerializeField] private HintHand_Drag _hintHand_Drag;

        [Space]
        public Transform oldPanel;
        public Panel_World panel_World;

        public override async Task Enter()
        {
            await base.Enter();

            items = droppable.Select(x => x.GetComponent<Dropable_General>()).ToList();

            holder.DOMoveX(0, 1);
            items.ForEach(d =>
            {
                d.hasDropped.AddListener(OnDecorDropped);
                d.placesHolder.transform.DOScale(0, 0);
            });

            oldPanel?.gameObject?.SetActive(false);
            panel_World.PrepareForAnimation(items.Select(x => x.GetComponent<PanelItem>()).ToList());
            await panel_World.Appear();
            panel_World.AnimateItems();

            items.ForEach(d =>
            {
                d.hasDropped.AddListener(OnDecorDropped);
                d.placesHolder.transform.DOScale(itemsScale, 1);
            });

            _hintHand_Drag = GetComponent<HintHand_Drag>();
            _hintHand_Drag.SetIsActive(true);
        }

        public override async Task Exit()
        {
            await base.Exit();

            if (_isDone == false)
            {
                holder.DOMoveX(25, 1);
            }
        }

        private void OnDecorDropped(bool obj)
        {
            foreach (var item in items)
            {
                if (item.hasDropped.value)
                {
                    item.GetComponentInChildren<BoxCollider>().enabled = false;

                    item.lasNearestPosition.DOScale(0, 0);
                    item.placesHolder.secondary.Remove(item.lasNearestPosition);
                }
            }

            if (items.TrueForAll(x => x.hasDropped.value))
            {
                Next();
            }

            return;
        }

        private void Next()
        {
            _isDone = true;

            if (_model != null)
            {
                _model.onCompleteButtonPressed -= Next;
            }

            if (_nextStateOnWin != null)
            {
                items.ForEach(d =>
                {
                    d.transform.SetParent(null, true);
                });

                DiService.Get<StateEnd_FX>()?.DoFX();
                _nextState = _nextStateOnWin;
            }
            else
            {
                _model?.onFinish?.Invoke();
            }
        }
    }
}