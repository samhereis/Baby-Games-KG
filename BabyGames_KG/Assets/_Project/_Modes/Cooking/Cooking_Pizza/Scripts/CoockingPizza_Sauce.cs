using CustomAttributes;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Coocking
{
    public class CoockingPizza_Sauce : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        public static Action<CoockingPizza_Sauce> onSelected;

        public SpriteRenderer icon;
        public Sprite sauceSprite;

        [SerializeField, Fg_De] private bool _active = true;

        private void OnEnable()
        {
            onSelected += OnSauceSelected;
        }

        private void OnDisable()
        {
            onSelected -= OnSauceSelected;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onSelected?.Invoke(this);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            transform.DOKill();
            transform.DOScale(0.75f, 0.25f);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            transform.DOKill();
            transform.DOScale(1f, 0.25f);
        }

        private void OnSauceSelected(CoockingPizza_Sauce selected)
        {
            if (selected != this)
            {
                icon.DOColor(Color.gray, 0.5f);
            }
            else
            {
                icon.DOColor(Color.white, 0.5f);
            }
        }
    }
}
