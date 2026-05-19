using DataClasses;
using DG.Tweening;
using FX;
using Gameplay;
using Identifiers;
using Services;
using Sounds;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Coocking
{
    public class CookingCake_Decoring_Candles : CoockingBurger_StateBase
    {
        public Transform holder;

        public List<DroppableGeneral_SimpleController> droppable = new();
        public List<Dropable_General> items = new();
        public List<Dropable_General> copies = new();

        [SerializeField] private HintHand_Drag _hintHand_Drag;

        [Space]
        public Transform oldPanel;
        public Panel_World panel_World;

        [Space]
        [SerializeField] private Sound _candleSound;
        [Inject] private ISoundPlayer _soundPlayer;

        public override async Task Enter()
        {
            await base.Enter();

            items = droppable.Select(x => x.GetComponent<Dropable_General>()).ToList();

            holder.DOMoveX(0, 1);
            items.ForEach(d =>
            {
                d.hasDropped.AddListener(OnDecorDropped);
                d.onCopyAdded += OnCopyAdded;
                d.placesHolder.transform.DOScale(0, 0);
            });

            oldPanel?.gameObject?.SetActive(false);
            panel_World.PrepareForAnimation(items.Select(x => x.GetComponent<PanelItem>()).ToList());
            await panel_World.Appear();
            panel_World.AnimateItems();

            items.ForEach(d =>
            {
                d.hasDropped.AddListener(OnDecorDropped);
                d.placesHolder.transform.DOScale(1, 1);
            });

            _hintHand_Drag = GetComponent<HintHand_Drag>();
            _hintHand_Drag.SetIsActive(true);

            DiService.Inject(this);
        }

        public override async Task Exit()
        {
            items.ForEach(d =>
            {
                d.hasDropped.RemoveListener(OnDecorDropped);
                d.onCopyAdded -= OnCopyAdded;
            });

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
                    item.lasNearestPosition.DOScale(0, 0);
                    item.placesHolder.secondary.Remove(item.lasNearestPosition);
                }

                item.copies.ForEach(x =>
                {
                    x.GetComponent<Collider>().enabled = false;
                });
            }

            if (items.TrueForAll(x => x.placesHolder.secondary.Count < 1))
            {
                Next();
            }

            return;
        }

        private void OnCopyAdded(Dropable_General general1, Dropable_General general2)
        {
            general2.GetComponent<Collider>().enabled = false;
            copies.Add(general2);
            general2.transform.SetParent(null);
        }

        private async void Next()
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
                await panel_World.HideItems();
                _nextState = _nextStateOnWin;
            }
            else
            {
                foreach (var item in copies)
                {
                    item.skeletonAnimation.loop = false;
                    item.skeletonAnimation.AnimationName = "action";
                }

                foreach (var item in items)
                {
                    item._boxCollider.enabled = false;
                }

                _model?.onFinish?.Invoke();
            }

            _soundPlayer?.TryPlay(_candleSound);
        }
    }
}