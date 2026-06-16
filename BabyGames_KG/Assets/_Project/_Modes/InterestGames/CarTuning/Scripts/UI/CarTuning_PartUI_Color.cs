using System;
using _Project._Modes.Puzzle.Scripts;
using CarTuning;
using CustomAttributes;
using DG.Tweening;
using Modes.Puzzle;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace _Project._Modes.InterestGames.CarTuning.Scripts.UI
{
    public class CarTuning_PartUI_Color : CarTuning_PartUI_Base
    {
        [SerializeField] private RectTransform _holder;
        [SerializeField] private string _colorIndex;

        public Drawable drawable;
        public DrawableP2D drawableP2D;

        [ShowInInspector] private static bool _hasWon = false;
        [ShowInInspector] private static bool _isDrawableSet = false;
        [ShowInInspector] private static CarTuning_PartUI_Color _currentChosen;

        private void OnEnable()
        {
            if (_model != null && _model.currentCar.value != null)
            {
                drawable = _model.currentCar.value.drawable;
                drawableP2D = _model.currentCar.value.drawable.GetComponent<DrawableP2D>();
            }

            _hasWon = false;
            _isDrawableSet = false;
            _currentChosen = null;
        }

        protected override void Update()
        {
            if (drawableP2D == null) { return; }

            if (Pointer.current.press.isPressed == false)
            {
                _isDragging = false;
                _currentChosen = null;
            }
            else
            {
                if (_isDragging) { _holder.position = Pointer.current.position.ReadValue(); }
            }

            if (drawableP2D.percentageOfColoring > 50 && _hasWon == false)
            {
                _model.onStateCompleted?.Invoke();
                _hasWon = true;
            }
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (_currentChosen == null) { _currentChosen = this; }
            if (_currentChosen != this) { return; }

            var colors = _model.currentCar.value._colors;
            var sprite = colors.Find(x => x.name == _model.currentColor.value)?.sprite
                         ?? colors.Find(x => x.name == "white")?.sprite;

            if (sprite != null)
            {
                _model.currentCar.value.ChangeColor(_colorIndex);

                if (_isDrawableSet == false || drawableP2D.isActive == false)
                {
                    drawable.gameObject.SetActive(true);
                    drawableP2D.Initialize(sprite);
                    _isDrawableSet = true;
                }

                drawableP2D.SetBrushColor(GetComponent<Image>().color);
            }

            GetComponent<Image>().raycastTarget = false;
            drawableP2D.ignoreFinger = false; // lets CwHitScreen2D paint naturally
            _isDragging = true;
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (_currentChosen != this)
            {
                _isDragging = false;
                return;
            }
            _isDragging = true;
            // CwHitScreen2D handles painting via its own Update loop (GuiLayers = 0)
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (_currentChosen != this) { return; }

            GetComponent<Image>().raycastTarget = true;
            drawableP2D.ignoreFinger = true;

            GoBack();

            var colorSprite = _model.currentCar.value._colors.Find(x => x.name == _colorIndex)?.sprite;
            drawableP2D.FillTransparentWithSprite(colorSprite);
        }

        private void GoBack()
        {
            _holder.DOAnchorPos3D(Vector3.zero, 0.25f).SetEase(Ease.OutBack);
            _isDragging = false;
        }
    }
}