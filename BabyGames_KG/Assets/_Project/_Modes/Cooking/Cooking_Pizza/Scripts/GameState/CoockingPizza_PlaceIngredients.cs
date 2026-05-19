using CoockingSalade;
using DG.Tweening;
using FX;
using Gameplay;
using Identifiers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Coocking
{
    public class CoockingPizza_PlaceIngredients : CoockingSalade_StateBase
    {
        public Transform holder;
        public Transform pizza;
        public List<Dropable_PlaceWhileDrag> items = new();

        public Panel_World panel_World;

        [Space]
        public bool _showPannelSprite;
        public Transform panelSprite;

        [SerializeField] private HintHand_DrawDots _hintHand_DDrawDots;

        private bool _hasPlacedAny = false;
        private static int _sortingLayer = 0;

        public override async Task Enter()
        {
            if (holder == null) { holder = transform; }
            holder.DOMoveY(0, 1);

            foreach (var item in items)
            {
                item.parentToSet = pizza.transform;
                item.placesHolder.gameObject.SetActive(true);
                item.onCopyAdded += OnIngredientPlaced;
            }

            await base.Enter();

            if (_showPannelSprite == false && panelSprite != null)
            {
                panelSprite?.gameObject.SetActive(false);
            }

            panel_World.PrepareForAnimation(items.Select(x => x.GetComponentInParent<PanelItem>()).ToList());
            await panel_World.Appear();
            panel_World.AnimateItems();

            _hintHand_DDrawDots = GetComponent<HintHand_DrawDots>();
            _hintHand_DDrawDots?.SetIsActive(true);
        }

        public override async Task Exit()
        {
            await base.Exit();

            foreach (var item in items)
            {
            }
        }

        private void OnIngredientPlaced(Dropable_PlaceWhileDrag drag, GameObject copy)
        {
            _sortingLayer++;

            foreach (var item in copy.GetComponentsInChildren<SpriteRenderer>())
            {
                item.sortingOrder = _sortingLayer;
            }

            if (_hasPlacedAny == true)
            {
                return;
            }

            _hasPlacedAny = true;

            _model.requestCompleteButtonShow?.Invoke();
            _model.onCompleteButtonPressed += NextState;
        }

        private async void NextState()
        {
            await panel_World.HideItems();

            if (_showPannelSprite == false && panelSprite != null)
            {
                await panelSprite.transform.DOLocalMoveX(25, 0).AsyncWaitForCompletion();
                panelSprite?.gameObject.SetActive(true);
            }

            _nextState = _nextStateOnWin;
        }
    }
}
