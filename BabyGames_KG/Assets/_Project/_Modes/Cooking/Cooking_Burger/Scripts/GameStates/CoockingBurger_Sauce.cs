using DG.Tweening;
using Gameplay;
using Identifiers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Coocking
{
    public class CoockingBurger_Sauce : CoockingBurger_StateBase, ISelfValidator
    {
        public List<DroppableGeneral_SimpleController> droppables = new();
        public List<Dropable_General> items = new();

        public List<SpriteRenderer> droppables_Ready = new();
        public Transform droppablesHolder;

        [field: SerializeField] public int sauceIndex { get; private set; } = -1;

        private Vector3 _initialScale;

        [Space]
        public Transform oldPanel;
        public Panel_World panel_World;

        public void Validate(SelfValidationResult result)
        {
            items = droppables.Select(x => x._dropable).ToList();
        }

        public override async Task Enable()
        {
            await base.Enable();

            foreach (var item in droppables_Ready)
            {
                _initialScale = item.transform.localScale;
                item.transform.localScale = Vector3.zero;
                item.gameObject.SetActive(false);
            }
        }

        public override async Task Enter()
        {
            await base.Enter();

            oldPanel?.gameObject?.SetActive(false);
            panel_World.PrepareForAnimation(items.Select(x => x.GetComponent<PanelItem>()).ToList());
            await panel_World.Appear();
            panel_World.AnimateItems();

            foreach (var item in items)
            {
                item.onDropEnd -= OnIngredient_Drop;
                item.onDropEnd += OnIngredient_Drop;
            }

            _model.onCompleteButtonPressed -= OnCompleteButtonPressed;
            _model.onCompleteButtonPressed += OnCompleteButtonPressed;

            sauceIndex = -1;
        }

        public override async Task Disable()
        {
            await base.Disable();
            if (_isDone == false) { droppablesHolder.DOMoveY(25, 0); }

            droppablesHolder.DOMoveY(25, 0.25f);
        }

        public override async Task Exit()
        {
            await base.Exit();

            foreach (var item in items)
            {
                item.onDropEnd -= OnIngredient_Drop;
            }

            _model.onCompleteButtonPressed -= OnCompleteButtonPressed;
        }

        private void OnIngredient_Drop(Dropable_General obj)
        {
            bool isFirst = sauceIndex < 0;
            sauceIndex = items.IndexOf(obj);

            foreach (var item in items) { item.PlaceBack(); }

            for (int i = 0; i < droppables_Ready.Count; i++)
            {
                if (i == sauceIndex)
                {
                    droppables_Ready[i].gameObject.SetActive(true);
                    droppables_Ready[i].transform.DOScale(_initialScale, 0.25f).SetEase(Ease.OutBack);
                }
                else
                {
                    droppables_Ready[i].transform.DOScale(0, 0.25f).SetEase(Ease.OutBack).OnComplete(() =>
                    {
                        droppables_Ready[i].gameObject.SetActive(false);
                    });
                }
            }

            if (isFirst)
            {
                _model.requestCompleteButtonShow?.Invoke();
            }
        }

        private async void OnCompleteButtonPressed()
        {
            _model.onCompleteButtonPressed -= OnCompleteButtonPressed;

            _isDone = true;

            await panel_World.HideItems();
            await panel_World.Disppear();

            _nextState = _nextStateOnWin;

            foreach (var item in items)
            {
                item._boxCollider.enabled = false;
                Destroy(item.gameObject, 2);
                item.transform.DOMoveX(25, 0.25f).OnComplete(() =>
                {
                    item.gameObject.SetActive(false);
                });
            }
        }
    }
}