using Coocking;
using DG.Tweening;
using FX;
using Helpers;
using Identifiers;
using Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace InterestGames
{
    public class Makeup_MakeupItemsBase : Makeup_StateBase
    {
        public Panel_World pannel_world;

        public List<Dropable_Basic> droppables;
        public Makeup_Controller controller;

        public HintHand_Drag hintHand_Drag;

        public override async Task PreInittialize()
        {
            await base.PreInittialize();
            transform.DOLocalMoveX(25, 0);
        }

        public override async Task Enter()
        {
            await base.Enter();

            foreach (var item in droppables)
            {
                item.onFinish -= OnDrop;
                item.onFinish += OnDrop;
                item.onFinish -= DisableHint;
                item.onFinish += DisableHint;

                item.boxCollider.enabled = true;
            }

            transform.DOLocalMoveX(0, 1);

            hintHand_Drag.objects.Clear();
            hintHand_Drag.targets.Clear();
            hintHand_Drag.objects.AddRange(droppables.Select(x => x.boxCollider.transform).ToList());
            foreach (var item in droppables)
            {
                foreach (var item1 in item.targetPosition.secondary)
                {
                    hintHand_Drag.targets.SafeAdd(item1);
                }

                if (item.GetComponent<PanelItem>() == null) { item.gameObject.AddComponent<PanelItem>(); }
            }
            hintHand_Drag.SetIsActive(true);

            pannel_world.PrepareForAnimation(droppables.Select(x => x.GetComponent<PanelItem>()).ToList());
            pannel_world.AnimateItems();
        }

        public override async Task Exit()
        {
            foreach (var item in droppables)
            {
                item.onFinish -= OnDrop;
                item.onFinish -= DisableHint;

                item.boxCollider.enabled = false;

                if (_isDone)
                {
                    if (item.copy != null)
                    {
                        item.copy.OnCompleteButton();
                        Destroy(item.copy.gameObject, 5);
                    }

                    Destroy(item.gameObject, 5);
                }
            }

            if (_isDone) { droppables.Clear(); }

            await base.Exit();
        }

        protected virtual void OnDrop(Dropable_Basic basic)
        {
            controller.hairSkin[3] = controller.hairSkin[droppables.IndexOf(basic)];
        }

        protected void DisableHint(Dropable_Basic basic)
        {
            hintHand_Drag.objects.Clear();
            hintHand_Drag.targets.Clear();
            hintHand_Drag.SetIsActive(false);
        }

        protected virtual void TryWin()
        {
            controller.Build();

            _model.completeButtonShowRequested?.Invoke();

            _model.onCompleteButtonClicked -= OnCompleteButtonClicked;
            _model.onCompleteButtonClicked += OnCompleteButtonClicked;
        }

        protected async virtual void OnCompleteButtonClicked()
        {
            if (pannel_world != null)
            {
                foreach (var item in droppables)
                {
                    item.boxCollider.enabled = false;
                }
                await pannel_world.HideItems();
            }

            _model.onCompleteButtonClicked -= OnCompleteButtonClicked;

            _isDone = true;
            _nextState = _nextStateOnWin;
        }
    }
}