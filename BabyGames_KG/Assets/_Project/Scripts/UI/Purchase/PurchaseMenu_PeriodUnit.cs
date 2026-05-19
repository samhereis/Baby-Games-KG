using System;
using System.Collections.Generic;
using CustomAttributes;
using DG.Tweening;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI.Purchase
{
    public class PurchaseMenu_PeriodUnit : MonoBehaviour
    {
        public Action<PurchaseMenu_PeriodUnit> onChoose;

        [SerializeField] private List<CanvasGroup> _states = new();
        [SerializeField] private List<TextMeshProUGUI> _texts = new();

        [SerializeField] private Button _button;

        private void OnEnable()
        {
            _button.onClick.AddListener(OnChose);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnChose);
        }

        public void EnableState(int index)
        {
            for (int i = 0; i < _states.Count; i++)
            {
                if (i == index) { _states[i].DOFade(0, 0.25f); }
                else { _states[i].DOFade(1, 0.25f); }
            }

            if (index == 0) { _texts.ForEach(t => t.DOColor(Color.black, 0.25f)); }
            else { _texts.ForEach(t => t.DOColor(Color.white, 0.25f)); }
        }

        private void OnChose()
        {
            onChoose?.Invoke(this);
        }
    }
}