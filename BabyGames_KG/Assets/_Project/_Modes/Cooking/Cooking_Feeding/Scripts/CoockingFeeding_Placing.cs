using _Project._Modes.Orchestra.Scripts.SO;
using DG.Tweening;
using FX;
using Gameplay;
using Identifiers;
using Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Coocking
{
    public class CoockingFeeding_Placing : CoockingFeeding_StateBase
    {
        public List<Dropable_General> items = new();
        public List<PanelItem> panelItems = new();
        public Panel_World panel_World;

        [Space]
        public Transform miska;
        public Transform miskaTarget;

        [Inject] private Orchestra_Data _orchestra_Data;
        [SerializeField] private HintHand_Drag _hintHand_Drag;

        public override async Task Enter()
        {
            DiService.Inject(this);

            await base.Enter();

            _orchestra_Data.MakeConfetti();

            miska.transform.DOMove(miskaTarget.position, 1);
            miska.transform.DOScale(miskaTarget.localScale, 1);

            foreach (var item in items)
            {
                item.hasDropped.AddListener(OnDropped);
            }

            await transform.DOMoveX(0f, 1).AsyncWaitForCompletion();

            panel_World.PrepareForAnimation(panelItems);
            await panel_World.HideItems();
            await panel_World.Appear();
            panel_World.AnimateItems();

            _hintHand_Drag = GetComponent<HintHand_Drag>();
            _hintHand_Drag?.SetIsActive(true);
        }

        public override async Task Exit()
        {
            transform.DOMoveY(-25f, 1);

            await base.Exit();
        }

        public override void ForceWin()
        {
            base.ForceWin();
            _nextState = _nextStateOnWin;
        }

        private async void OnDropped(bool obj)
        {
            if (items.TrueForAll(x => x.hasDropped.value))
            {
                foreach (var item in items)
                {
                    item.hasDropped.RemoveListener(OnDropped);
                }

                _nextState = _nextStateOnWin;
            }

            foreach (var item in items)
            {
                if (item.hasDropped.value == true)
                {
                    item.transform.parent.Find("Plate")?.DOScale(0, 0.25f);
                }
            }
        }
    }
}