using UnityEngine;
using UnityEngine.UI;

namespace CarTuning
{
    public class CarTuning_PartUI_NoAnimation : CarTuning_PartUI_Base
    {
        [SerializeField] private Image _icon;

        public void Initialize(CarPart_NoAnimation carPart)
        {
            _icon.sprite = carPart.spriteRenderer.sprite;
            _carPart = carPart;
            Initialize();
        }
    }
}