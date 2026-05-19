using UnityEngine;

namespace CarTuning
{
    public class CarPart_NoAnimation : CarPart_Base
    {
        public SpriteRenderer spriteRenderer;

        private void Awake()
        {
            gameObject.SetActive(false);
            if (spriteRenderer == null) { spriteRenderer = GetComponentInChildren<SpriteRenderer>(); }
        }
    }
}