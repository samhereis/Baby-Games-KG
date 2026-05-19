using DG.Tweening;
using Identifiers;
using RTLTMPro;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SwitchButton : IdentifierBase
    {
        public Action<SwitchButton> onSelected;

        public Button button;
        public TextMeshProUGUI priceText;
        public CanvasGroup selectedCanvas;

        public bool isSelected { get; private set; }

        private void OnEnable()
        {
            button.onClick.AddListener(Select);
        }

        private void OnDisable()
        {
            button.onClick.RemoveListener(Select);
        }

        public void SetData(string price)
        {
            if (priceText is RTLTextMeshPro rTLTextMeshPro)
            {
                rTLTextMeshPro.text = price;
            }
            else
            {
                priceText.text = price;
            }
        }

        public void Select()
        {
            Select(true);

            isSelected = true;
            onSelected?.Invoke(this);
        }

        public void Select(bool isSelectedStatus)
        {
            isSelected = isSelectedStatus;
            if (isSelected) { selectedCanvas.DOFade(1, 0.25f); }
            else { selectedCanvas.DOFade(0, 0.25f); }
        }
    }
}