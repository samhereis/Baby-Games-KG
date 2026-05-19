using CustomAttributes;
using DG.Tweening;
using FX;
using Helpers;
using Identifiers;
using Loggers;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Coocking
{
    public class CoockingIceCream_Form : CoockingIceCream_StateBase
    {
        public Dropable_Basic[] droppables;

        public HintHand_Drag hintHand_Drag;

        [Fg_De] public Dropable_Basic currentSelectable;
        [Fg_De] public Coocking_IceCream currentForm;

        public override async Task Enter()
        {
            await base.Enter();

            await _controller.ChangeBackground(0);

            foreach (var item in droppables)
            {
                item.onStartDrop += OnStartDrop;
                item.doPunchAnimation = false;
            }

            _controller.panel_World.PrepareForAnimation(droppables.Select(x => x.GetComponent<PanelItem>()).ToList());
            await _controller.panel_World.Appear();
            _controller.panel_World.AnimateItems();

            try
            {
                hintHand_Drag = GetComponent<HintHand_Drag>();
                hintHand_Drag?.SetIsActive(true);

                hintHand_Drag.objects.Clear();
                hintHand_Drag.targets.Clear();
                hintHand_Drag.objects.AddRange(droppables.Select(x => x.transform));
                hintHand_Drag.targets.AddRange(droppables.Select(x => x.targetPosition.secondary.GetRandom()));
            } catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }
        }

        public override async Task Exit()
        {
            foreach (var item in droppables)
            {
                item.onStartDrop -= OnStartDrop;
                item.boxCollider.enabled = false;
                if (item != currentSelectable) { item.transform.DOScale(0, 0.25f).OnComplete(() => Destroy(item.gameObject)); }
            }
            await base.Exit();
        }

        private void OnStartDrop(Dropable_Basic dropable_Basic)
        {
            dropable_Basic.transform.DOKill();
            dropable_Basic.transform.DOScale(dropable_Basic.targetPosition.transform.localScale, 1);

            if (currentSelectable == null)
            {
                _model.requestCompleteButtonShow?.Invoke();
                _model.onCompleteButtonPressed += NextState;
            }
            else
            {
                currentSelectable.DoReset();
                currentSelectable.transform.DOScale(currentSelectable.GetComponent<PanelItem>().holderScale, 1);
            }

            currentSelectable = dropable_Basic;
            currentForm = currentSelectable.GetComponent<Coocking_IceCream>();
        }

        private async void NextState()
        {
            foreach (var item in droppables) { item.boxCollider.enabled = false; }
            _controller.panel_World.currentPanelItems.Remove(currentSelectable.GetComponent<PanelItem>());
            await _controller.panel_World.HideItems();
            _nextState = _nextStateOnWin;
        }
    }
}