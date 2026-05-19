using UnityEngine;

namespace CarTuning
{
    public class Princess_Dress_NoAnimation : Princess_DressBase
    {
        public SpriteRenderer spriteRenderer;

        private void Awake()
        {
            if (spriteRenderer == null) { spriteRenderer = GetComponentInChildren<SpriteRenderer>(); }
        }
    }
}