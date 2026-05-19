using DataClasses;
using DG.Tweening;
using Helpers;
using Modes.Puzzle;
using Sirenix.OdinInspector;
using System;
using System.Threading.Tasks;
using _Project.Scripts.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace CarTuning
{
    public class CarTuning_PartUI_Color : CarTuning_PartUI_Base
    {
        [SerializeField] private RectTransform _holder;

        [SerializeField] private string _colorIndex;
        public Drawable drawable;

        [ShowInInspector] private static bool _hasWon = false;
        [ShowInInspector] private static bool _isDrawableSet = false;
        [ShowInInspector] private static CarTuning_PartUI_Color _currentChosen;

        private void OnEnable()
        {
            if (_model != null && _model.currentCar.value != null)
            {
                drawable = _model.currentCar.value.drawable;
            }

            _hasWon = false;
            _isDrawableSet = false;
            _currentChosen = null;
        }

        protected override void Update()
        {
            if (Pointer.current.press.isPressed == false)
            {
                _isDragging = false;
                _currentChosen = null;
            }
            else
            {
                if (_isDragging == true)
                {
                    _holder.position = Pointer.current.position.ReadValue();
                }
            }

            if (drawable.percentageOfColoring > 50 && _hasWon == false)
            {
                _model.onStateCompleted?.Invoke();
                _hasWon = true;
            }
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (_currentChosen == null) { _currentChosen = this; }
            if (_currentChosen != this) { return; }

            Sprite sprite = null;

            try
            {
                sprite = _model.currentCar.value._colors.Find(x => x.name == _model.currentColor.value).sprite;
            }
            catch (Exception ex)
            {
                if (sprite == null)
                {
                    sprite = _model.currentCar.value._colors.Find(x => x.name == "white").sprite;
                }
            }

            if (sprite != null)
            {
                _model.currentCar.value.ChangeColor(_colorIndex);
                if (_isDrawableSet == false || drawable.isActive == false)
                {
                    drawable.gameObject.SetActive(true);
                    drawable.Initialize(sprite);
                    _isDrawableSet = true;
                }
            }

            drawable.GetComponent<BoxCollider2D>().enabled = true;
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
        }

        public override async void OnEndDrag(PointerEventData eventData)
        {
            if (_currentChosen != this) { return; }

            drawable.GetComponent<BoxCollider2D>().enabled = false;

            GoBack();

            var sprite = _model.currentCar.value._colors.Find(x => x.name == _colorIndex).sprite;
            if (drawable.currentDrawableTexture != null)
            {
                var origTexture = sprite.texture.GetReadableCopy();
                ClearablesHolder.instance.clearableTextures.SafeAdd(origTexture);

                var origTextureArray = origTexture.GetPixels32();
                Destroy(origTexture);

                await Task.Run(() =>
                {
                    for (int i = 0; i < origTextureArray.Length; i++)
                    {
                        if (i >= drawable.currentPixelsColorArray.Length)
                        {
                            continue;
                        }

                        Color32 orig_color = origTextureArray[i];
                        if (orig_color.a < 0.1f) { continue; }

                        Color32 drawable_color = drawable.currentPixelsColorArray[i];
                        if (drawable_color.a < 0.1f)
                        {
                            drawable.currentPixelsColorArray[i] = orig_color;
                        }
                    }
                });
            }
        }

        private void GoBack()
        {
            _holder.DOAnchorPos3D(Vector3.zero, 0.25f).SetEase(Ease.OutBack);
            _isDragging = false;
        }
    }
}