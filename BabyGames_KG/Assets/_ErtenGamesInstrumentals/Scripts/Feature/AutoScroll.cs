using Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class AutoScroll : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Scrollbar _scrollBar;

        [Header("Settings")]
        [SerializeField] private float _startDelay = 3;
        [SerializeField] private float _scrollSpeed = 1;

        [Header("Debug")]
        [SerializeField] private bool _isScrolling = false;
        [SerializeField] private bool _isReverse = false;

        private void Validate()
        {
            if (_scrollBar == null) { _scrollBar = GetComponentInChildren<Scrollbar>(); }
        }

        private void OnEnable()
        {
            Validate();

            _scrollBar.value = 0;
            _isReverse = true;

            StartAutoscroll(_startDelay);
        }

        private void Update()
        {
            if (_isScrolling == true)
            {
                if (_scrollBar.value >= 1)
                {
                    if (_isReverse == true) _isReverse = false; else _isReverse = true;
                }
                else if (_scrollBar.value <= 0)
                {
                    if (_isReverse == false) _isReverse = true; else _isReverse = false;
                }

                Scroll(_scrollSpeed * (_isReverse == true ? -1f : 1f));
            }
        }

        public void Scroll(float value)
        {
            _scrollBar.value += value * Time.deltaTime;
        }

        public async void StartAutoscroll(float delay = 0)
        {
            await AsyncHelper.DelayFloat(delay);
            _isScrolling = true;
        }
    }
}