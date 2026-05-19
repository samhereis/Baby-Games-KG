using DG.Tweening;
using Helpers;
using Modes.Puzzle;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace CarTuning
{
    public class CarTuning_Car_Identifier : MonoBehaviour
    {
        public Transform frontWheel_Position;
        public Transform backWheel_Position;

        public Transform frontLight_Position;
        public Transform backLight_Position;

        public Transform syreneLight_Position;

        public Drawable drawable;
        public List<SpriteRenderer> _colors = new();
        public List<Sprite> _corpuses = new();

        private void Awake()
        {
            ChangeColor("white");

            int index = 0;
            foreach (var item in _colors)
            {
                if (index == 0) { return; }
                item.sprite = _colors[0].sprite;

                index++;
            }
        }

        public void ChangeColor(string colorIndex)
        {
            foreach (var item in _colors)
            {
                item.DOFade((item.name == colorIndex).ToInt(), 1);
            }
        }

        [Button]
        private void Validate()
        {
            foreach (var item in GetComponentsInChildren<CarPart_Base>())
            {
                item.Validate();
            }

            int index = 0;
            foreach (var item in _colors)
            {
                item.sprite = _corpuses[index];
                item.color = Color.white;
                index++;
            }
        }
    }
}