using System;
using DG.Tweening;
using FX;
using Helpers;
using Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Coocking;
using Gameplay;
using Identifiers;
using Loggers;
using UnityEngine;

namespace ColorfulTrain
{
    [RequireComponent(typeof(HintHand_Drag))]
    public class ColorfulTrain_CollectingState : ColorfulTrain_StateBase
    {
        [SerializeField] private List<Transform> _toEnable = new();
        [SerializeField] private List<Transform> _tDisable = new();

        public List<ColorfulTrain_Wheel> seats = new();
        public List<Dropable_Basic> dropables = new();

        public Panel_World panel;

        public HintHand_Drag hintHand_Drag;

        public override async Task Enable()
        {
            hintHand_Drag = GetComponent<HintHand_Drag>();

            foreach (Transform t in _toEnable) { t.DOScaleY(1, 1); }
            foreach (Transform t in _tDisable) { t.DOScaleY(0, 0); }

            hintHand_Drag.targets.Clear();
            hintHand_Drag.objects.Clear();

            await base.Enable();
        }

        public override async Task Enter()
        {
            await base.Enter();

            _model.train.seats = seats;
            List<PanelItem> temp = new();

            for (var i = 0; i < _model.train.seats.Count; i++)
            {
                var item = _model.train.seats[i];
                var droppable = dropables[i];
                var dropablePanelItem = droppable.GetComponent<PanelItem>();

                item.currentPanel_Copy = droppable;
                item.Initialize();

                droppable.isDropped.value = false;
                droppable.canDrag = true;
                droppable.boxCollider.enabled = true;
                droppable.autoPlace = true;
                droppable.transform.localScale = item.spriteRenderer.transform.localScale;
                droppable.targetPosition = item.Get<PlacesHolder>();
                droppable.GetComponent<SpriteRenderer>().sprite = item.spriteRenderer.sprite;
                droppable.onFinish += OnEndDrop;
                dropablePanelItem.holderScale = item.spriteRenderer.transform.localScale;

                temp.Add(dropablePanelItem);

                hintHand_Drag.targets.Add(item.spriteRenderer.transform);
                hintHand_Drag.objects.Add(droppable.transform);
            }

            PlayerActions_DataHolder.ResetTime();

            if (panel == null) { panel = MonobehaviorHelper.Find<Panel_World>(); }
            panel.PrepareForAnimation(temp);
            await panel.Appear();
            await panel.AnimateItems_Async();

            foreach (var item in dropables) { item.objectJuicer?.StartJamming(); }

            await AsyncHelper.DelayFloat(1f);
        }

        public override Task Exit()
        {
            foreach (var item in _model.train.seats)
            {
                item.objectJuicer?.StopJamming();
            }
            return base.Exit();
        }

        private async void OnEndDrop(Dropable_Basic obj)
        {
            try
            {
                obj.objectJuicer?.StopJamming();

                foreach (var item in _model.train.seats)
                {
                    if (item.currentPanel_Copy.isDropped.value)
                    {
                        hintHand_Drag.targets.Remove(item.spriteRenderer.transform);
                        hintHand_Drag.objects.Remove(obj.transform);
                    }
                }

                if (dropables.TrueForAll(x => x.isDropped.value))
                {
                    _nextState = _nextStateOnWin;

                    foreach (var item in _model.train.seats)
                    {
                        item.currentPanel_Copy.gameObject.SetActive(false);
                        item.PlaceToTrain(false);
                    }

                    await panel.Disppear();
                }
            } catch (Exception e) { CustomLogger.instance?.LogException(e); }
        }

        public override void ForceWin()
        {
            foreach (var item in dropables)
            {
                item.targetPosition.lasNearestPosition = item.targetPosition.secondary[0];
                item.Place();
            }
        }
    }
}