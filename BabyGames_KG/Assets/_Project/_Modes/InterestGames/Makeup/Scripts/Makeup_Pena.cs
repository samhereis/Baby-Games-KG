using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace InterestGames
{
    public class Makeup_Pena : MonoBehaviour, IPointerExitHandler
    {
        public Action onMouseEnter;

        public bool isActive = false;
        public enum State { None, Appeared, Watered, Washed }

        public SpriteRenderer spriteRenderer;
        public SpriteRenderer waterDrop;
        public BoxCollider boxCollider;

        public State state = State.None;

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            boxCollider = GetComponentInChildren<BoxCollider>();
            spriteRenderer.transform.localScale = Vector3.zero;
            waterDrop.transform.localScale = Vector3.zero;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (isActive == false) return;
            onMouseEnter?.Invoke();
        }
    }
}
