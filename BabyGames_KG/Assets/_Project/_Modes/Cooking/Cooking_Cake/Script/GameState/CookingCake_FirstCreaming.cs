using DG.Tweening;
using FX;
using Gameplay;
using Identifiers;
using Loggers;
using Modes.Puzzle;
using ModestTree;
using Services;
using Sirenix.OdinInspector;
using Spine;
using Spine.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _Project._Modes.Puzzle.Scripts;
using CustomAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Coocking
{
    public class CookingCake_FirstCreaming : CoockingBurger_StateBase
    {
        public Transform[] toHide;

        public Transform korjHolder;
        public Transform creamsHolder;

        [Space]
        public DroppableGeneral_SimpleController[] creams;
        public Dropable_General[] items;

        public List<Sprite> creamsSprites = new();
        public SpriteRenderer cakeSpriteRenderer;
        [Re_Fg_Co] public Drawable drawable;
        [Re_Fg_Co] public DrawableP2D drawableP2D;
        public Transform korjOutline;

        [Header("Skins")]
        [SpineSkin] public string creamUpperSkinInitial;
        [SpineSkin] public string[] creamUpperSkins;
        [SerializeField] public SkeletonAnimation spineObject;
        public Skin combinedSkin;

        [Space]
        [SerializeField] private HintHand_Drag _hintHand_Drag;
        [SerializeField] private HintHand_DrawSimple _hintHand_Draw;
        [SerializeField] private HintHand_DrawDots _hintHand_DrawDots;

        [Space]
        [SerializeField] private Color _creamInactiveFolor = Color.gray;

        [Space]
        public Transform oldPanel;
        public Panel_World panel_World;

        public Dropable_General currentCream { get; private set; }
        public int creamIndex { get; private set; }


        public override async Task Enter()
        {
            await base.Enter();

            items = creams.Select(x => x.GetComponent<Dropable_General>()).ToArray();
            drawableP2D = drawable.GetComponent<DrawableP2D>();

            await _controller.ShowCurtain(true, 0.25f);
            await _controller.ChangeBackground(_backgroundIndex);

            try
            {
                Build_Initial(creamUpperSkinInitial);

                foreach (var item in toHide)
                {
                    item.DOMoveX(-25, 1);
                }

                korjHolder.DOKill();


                int index = 0;
                foreach (var item in items)
                {
                    item.spriteRenderer.sprite = creamsSprites[index];

                    item.onStartDrag -= OnFirstDrag;
                    item.onStartDrag += OnFirstDrag;

                    item.onMouseUp -= OnDraggableMouseUp;
                    item.onMouseUp += OnDraggableMouseUp;

                    item.onDropEnd -= OnCreamSelected;
                    item.onDropEnd += OnCreamSelected;

                    item.onMouseUpAsButton -= OnCreamSelected;
                    item.onMouseUpAsButton += OnCreamSelected;

                    index++;
                }

                spineObject.transform.SetParent(null);
                spineObject.transform.DOMoveX(0, 0);
                korjHolder.DOMoveX(0, 1);
                creamsHolder.DOLocalMoveX(8, 1);

                await _controller.ShowCurtain(false);

                oldPanel?.gameObject?.SetActive(false);
                panel_World.PrepareForAnimation(creams.Select(x => x.GetComponent<PanelItem>()).ToList());
                await panel_World.Appear();
                panel_World.AnimateItems();
            } catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
                await _controller.ShowCurtain(false);
            }
        }

        public override async Task Exit()
        {
            await base.Exit();
            creamsHolder.DOLocalMoveX(25, 1);
            drawableP2D.gameObject.SetActive(false);
        }

        public override void Tick()
        {
            if (Pointer.current.press.isPressed == false)
            {
                drawableP2D.enabled = true;
                return;
            }

            if (_isDone == true) { return; }

            base.Tick();

            if (drawableP2D.percentageOfColoring < 20)
            {
                _model.requestCompleteButtonShow?.Invoke();
                _model.onCompleteButtonPressed -= Next;
                _model.onCompleteButtonPressed += Next;
            }
        }

        private void OnFirstDrag(Dropable_General controller)
        {
            drawableP2D.enabled = false;
        }

        private void OnDraggableMouseUp(Dropable_General controller)
        {
            drawableP2D.enabled = true;
        }

        private void OnCreamSelected(Dropable_General controller)
        {
            drawableP2D.enabled = true;

            currentCream = controller;
            currentCream.PlaceBack();
            creamIndex = items.IndexOf(currentCream);

            if (_hintHand_Drag != null)
            {
                _hintHand_Drag.SetIsActive(false);
                Destroy(_hintHand_Drag);
            }

            if (_hintHand_DrawDots != null)
            {
                _hintHand_DrawDots.SetIsActive(false);
                Destroy(_hintHand_Draw);
            }

            foreach (var item in items)
            {
                item.spriteRenderer.DOColor(currentCream == item ? Color.white : _creamInactiveFolor, 0.5f);
                item.transform.Find("Outline")?.gameObject.SetActive(currentCream == item);
            }

            controller.PlaceBack();

            drawableP2D.gameObject.SetActive(true);

            drawableP2D.drawMode = DrawableP2D.DrawMode.Reveal;
            cakeSpriteRenderer.sprite = creamsSprites[creamIndex];
            drawableP2D.Initialize(cakeSpriteRenderer.sprite);

            if (_hintHand_DrawDots != null)
            {
                _hintHand_DrawDots.SetIsActive(true);
                _hintHand_DrawDots.drawable = drawableP2D.transform.parent;
                _hintHand_DrawDots.outline = korjOutline;
            }

            _model.requestCompleteButtonHide?.Invoke();
        }

        [Button]
        public void Build_Initial(string skinName)
        {
            combinedSkin = new Skin("combinedSkin");

            AddSkin(skinName);

            spineObject.skeleton.SetSkin(combinedSkin);
            spineObject.skeleton.SetSlotsToSetupPose();
        }

        private void AddSkin(string skinName)
        {
            if (string.IsNullOrEmpty(skinName) == false)
            {
                var skin = spineObject.skeleton.Data.FindSkin(skinName);
                combinedSkin.AddSkin(skin);
            }
        }

        private async void Next()
        {
            Build_Initial(creamUpperSkins[creamIndex]);
            drawableP2D.gameObject.SetActive(false);

            _isDone = true;
            DiService.Get<StateEnd_FX>()?.DoFX();
            await panel_World.HideItems();
            _nextState = _nextStateOnWin;
        }
    }
}