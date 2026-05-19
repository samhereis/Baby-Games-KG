using System;
using _Project.Scripts.Sound;
using Coocking;
using DG.Tweening;
using FX;
using Gameplay;
using Helpers;
using Identifiers;
using Sirenix.OdinInspector;
using Sounds;
using Spine.Unity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Loggers;
using UnityEngine;

namespace CoockingSalade
{
    public class CoockingSalade_Decoration : CoockingSalade_StateBase
    {
        public List<Dropable_General> decorationSprites;
        public List<Dropable_Basic> items;
        private bool _hasDropped;
        public Transform vilka;
        public SkeletonAnimation _plateAnimation;
        public Panel_World panel;

        public SoundQueue _soYummySound;

        [SerializeField] private HintHand_Drag _hintHand_Drag;

        public override async Task Enter()
        {
            await base.Enter();

            _model.stateEnd_FX?.DoFX();

            decorationSprites = GetComponentsInChildren<Dropable_General>(true).ToList();
            items = FindObjectsOfType<Dropable_Basic>(true).ToList();
            _hintHand_Drag = GetComponent<HintHand_Drag>();

            _model.currentState.ChangeValue(this);
            _model.onCompleteButtonPressed += OnComplete;

            foreach (var item in items)
            {
                if (item.GetComponent<BoxCollider>() == false)
                {
                    var box = item.gameObject.AddComponent<BoxCollider>();
                    box.size = new Vector3(2, 2, 0);
                }
                item.onStartDrag -= OnStartDrag;
                item.onStartDrag += OnStartDrag;
                item.onFinish += OnDropped;
            }

            vilka?.DOMoveY(25, 1);

            _plateAnimation.transform.DOScale(1.35f, 1);

            _hintHand_Drag.SetIsActive(true);
            _hintHand_Drag.objects.Clear();
            _hintHand_Drag.objects.AddRange(items.Select(x => x.transform));

            SetAnimation();

            foreach (var item in items) { item.gameObject.SetActive(true); }
            if (panel != null)
            {
                panel?.PrepareForAnimation(items.Select(x => x.GetComponent<PanelItem>()).ToList());
                await panel?.Appear();
                panel?.AnimateItems();
            }
            else
            {
                foreach (var item in items)
                {
                    item.transform.DOMoveX(8, 1);
                }
            }
        }

        public override async Task Exit()
        {
            try
            {
                await base.Exit();

                foreach (var item in items)
                {
                    item.onStartDrag -= OnStartDrag;
                    item.onFinish -= OnDropped;
                }
            } catch (Exception e)
            {
                CustomLogger.instance?.LogException(e);
            }
        }

        public override void Tick()
        {
            base.Tick();
            foreach (var item in items)
            {
                item.boxCollider.enabled = true;
            }
        }

        [Button]
        private async void SetAnimation()
        {
            _plateAnimation.AnimationState.ClearTracks();
            _plateAnimation.AnimationName = "idle";
            await AsyncHelper.NextFrame();
            _plateAnimation.AnimationName = "action";
            await AsyncHelper.NextFrame();
            _plateAnimation.AnimationState.SetAnimation(0, "idle", true);
        }

        private int sortingOrder = 0;
        private void OnStartDrag(Dropable_Basic dropable)
        {
            sortingOrder++;
            dropable.GetComponentInChildren<SpriteRenderer>().sortingOrder = sortingOrder;
        }

        private void OnDropped(Dropable_Basic dropable)
        {
            if (_hasDropped == false)
            {
                _model.requestCompleteButtonShow?.Invoke();
            }

            if (_hasDropped) { return; }
            _hasDropped = true;
        }

        private async void OnComplete()
        {
            await Sound_FX.PlayAsync_Static(_soYummySound);

            await panel.HideItems();
            await panel.Disppear();

            _model.onFinish?.Invoke();
        }
    }
}