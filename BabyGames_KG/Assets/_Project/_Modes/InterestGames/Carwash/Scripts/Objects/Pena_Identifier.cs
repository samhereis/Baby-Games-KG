using DG.Tweening;
using Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace Carwash
{
    public class Pena_Identifier : MonoBehaviour
    {
        [SerializeField] private Vector2 _scale = Vector2.one;
        [SerializeField] public SpriteRenderer spriteRenderer;
        [SerializeField] public SpriteRenderer spriteRenderer_Dirt;

        [SerializeField] public List<Sprite> sprites = new();
        [SerializeField] public List<Sprite> dirt = new();

        private float _currentScale = 1;
        public bool isDirty = false;
        public bool isPolished = false;
        public bool isVisible { get; private set; } = false;

        private static int _lastSortingLayer = 0;

        public bool IsDirty()
        {
            return isDirty;
        }

        private void Awake()
        {
            transform.localScale = Vector3.zero;
            spriteRenderer.sortingOrder = _lastSortingLayer++;

            spriteRenderer.sprite = sprites.GetRandom();
            spriteRenderer_Dirt.sprite = dirt.GetRandom();
            spriteRenderer_Dirt.DOFade(0, 0);
        }

        private void OnEnable()
        {
            transform.DOScale(_scale.GetRandom(), 1);
            transform.DORotate(new Vector3(0, 0, new Vector2(0, 360).GetRandom()), 0);

            isVisible = true;
        }

        public void MakeDirty()
        {
            if (isDirty) { return; }

            spriteRenderer.DOFade(0, 1);
            spriteRenderer_Dirt.DOFade(0.85f, 1);

            isDirty = true;
        }

        public void Fade()
        {
            spriteRenderer.transform.DOScale(0, 0.75f);
            spriteRenderer_Dirt.transform.DOScale(0, 0.75f);
            isVisible = false;
        }
    }
}