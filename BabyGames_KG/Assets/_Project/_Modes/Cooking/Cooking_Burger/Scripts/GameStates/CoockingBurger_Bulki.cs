using CustomAttributes;
using DG.Tweening;
using FX;
using Gameplay;
using Helpers;
using Identifiers;
using Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Coocking
{
    public class CoockingBurger_Bulki : CoockingBurger_StateBase
    {
        public Transform burgersPannels;
        public Transform plate;

        public List<DroppableGeneral_SimpleController> droppables = new();
        public List<Dropable_General> items = new();

        public List<Sprite> bulkis = new();
        public List<Sprite> bulkis_Upper = new();

        public Transform bulkaParent;
        public SpriteRenderer bulkaSpriteRenderer;
        public SpriteRenderer bulkaUpperSpriteRenderer;

        [Space]
        public Transform oldPanel;
        public Panel_World panel_World;

        [Fg_De] public int bulkaIndex = -1;
        private Sprite _bulkaUpper;

        private Dropable_General _currentBulka;

        public override async Task Disable()
        {
            burgersPannels.DOMoveY(25f, 0);

            if (_isDone == false)
            {
                bulkaParent.gameObject.SetActive(false);
            }

            _model.onCompleteButtonPressed += OnCompleteButtonClicked;

            await base.Disable();
        }

        public override async Task Enter()
        {
            await base.Enter();

            plate.DOMove(Vector3.zero, 1f);

            oldPanel?.gameObject?.SetActive(false);
            panel_World.PrepareForAnimation(items.Select(x => x.GetComponent<PanelItem>()).ToList());
            await panel_World.Appear();
            await panel_World.AnimateItems_Async();

            foreach (var item in items)
            {
                item.onDropEnd -= OnSelected;
                item.onDropEnd += OnSelected;

                item.objectJuicer.StartJamming();
            }

            _model.onCompleteButtonPressed -= OnCompleteButtonClicked;
            bulkaIndex = -1;
        }

        public override async Task Exit()
        {
            await base.Exit();
        }

        private async void OnSelected(Dropable_General controller)
        {
            bool isFirst = bulkaIndex < 0;

            controller.onDropStart -= OnSelected;
            _currentBulka = controller;

            bulkaIndex = items.IndexOf(_currentBulka);
            _bulkaUpper = bulkis_Upper[bulkaIndex];
            bulkaUpperSpriteRenderer.sprite = _bulkaUpper;

            bulkaParent.gameObject.SetActive(true);
            await bulkaSpriteRenderer.DOFade(0, 0.25f).AsyncWaitForCompletion();
            bulkaParent.transform.localScale = Vector3.zero;
            bulkaSpriteRenderer.sprite = bulkis[bulkaIndex];

            bulkaSpriteRenderer.DOFade(1, 0);
            bulkaParent.transform.DOScale(1, 0.25f).SetEase(Ease.OutBack);

            foreach (var item in items)
            {
                item.PlaceBack();
                item.objectJuicer.StopJamming();
            }

            if (isFirst)
            {
                _model.requestCompleteButtonShow?.Invoke();
            }
        }

        private void OnCompleteButtonClicked()
        {
            _model.onCompleteButtonPressed -= OnCompleteButtonClicked;
            _isDone = true;
            _nextState = _nextStateOnWin;

            foreach (var item in items)
            {
                if (item != null)
                {
                    item.onDropEnd -= OnSelected;

                    item._boxCollider?.SetEnabled(false);
                    item.gameObject.SetActive(false);
                    Destroy(item.gameObject, 2);
                }
            }
        }
    }
}