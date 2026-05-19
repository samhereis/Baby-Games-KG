using UnityEngine;
using UnityEngine.UI;

namespace CarTuning
{
    public class Princess_WearubgUI_NoAnimation : Princess_WearubgUI_Base
    {
        [SerializeField] private Image _image;

        public void Initialize(Princess_Dress_NoAnimation dressBase)
        {
            _wearing = dressBase;
            _image.sprite = dressBase.spriteRenderer.sprite;
        }
    }
}