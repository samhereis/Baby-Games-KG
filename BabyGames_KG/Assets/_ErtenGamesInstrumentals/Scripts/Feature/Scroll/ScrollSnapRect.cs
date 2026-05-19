#if DoTweenInstalled
using DG.Tweening;
#endif

using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ScrollSnapRect : MonoBehaviour
    {
        public enum Direction { Horizontal, Vertical }

        private Action _updateAction;

        [Header("Settings")]
        [SerializeField] private float _listValue = 100;
        [SerializeField] private Direction _direction = Direction.Horizontal;

        [Header("Componenets")]
        [SerializeField] private List<SingleScrollElement> _buttons = new List<SingleScrollElement>();
        [SerializeField] private RectTransform _contentRect;
        [SerializeField] private Image _center;

        [Header("Degub")]
        [SerializeField] private SingleScrollElement _selectedButton;
        [SerializeField] private float _contentVector;
        [SerializeField] float _nearestPos;
        [SerializeField] float _distance;
        [SerializeField] private float _substractFromFirst;
        [SerializeField] private float _substractFromLast;
        [SerializeField] private float _minPos;
        [SerializeField] private float _maxPos;

        private float minPosVertical => _maxPos = _buttons.First()._position.y - _substractFromFirst;
        private float maxPosVertical => _minPos = _buttons.Last()._position.y - _substractFromLast;

        private float minPosHorizontal => _maxPos = _buttons.First()._position.x - _substractFromFirst;
        private float maxPosHorizontal => _minPos = _buttons.Last()._position.x - _substractFromLast;

        private void OnEnable()
        {
            if (_direction == Direction.Horizontal) _updateAction = UpdateHorizontal; else _updateAction = UpdateVertical;
        }

        private void FixedUpdate()
        {
            if (_buttons.Count == 0 || _buttons == null || ScrollElement.isInAction) return;

            _nearestPos = float.MaxValue;

            foreach (var button in _buttons)
            {
                _distance = Vector3.Distance(_center.transform.position, button.button.transform.position);

                if (_distance < _nearestPos)
                {
                    _nearestPos = _distance;
                    _selectedButton = button;
                }
            }

            UpdateAllElements();
        }

        private void Update()
        {
            if (Input.GetMouseButtonUp(0) || ScrollElement.isInAction) _updateAction?.Invoke();
        }

        private bool _isUpdating = false;
        private async void UpdateAllElements()
        {
            if (_isUpdating == false)
            {
                _isUpdating = true;

                foreach (var button in _buttons)
                {
                    if (_selectedButton.button == button.button) button.button.Enable(); else button.button.Disable();
                    await AsyncHelper.Skip();
                }

                _isUpdating = false;
            }
        }

        public void RegidterElement(ScrollElement scrollElement)
        {
            _buttons.Add(new SingleScrollElement(scrollElement, -scrollElement.transform.localPosition));
        }

        public void DeregidterElement(ScrollElement scrollElement)
        {
            _buttons.Remove(new SingleScrollElement(scrollElement, -scrollElement.transform.localPosition));
        }

        private void UpdateVertical()
        {
#if DoTweenInstalled
            _contentRect.DOAnchorPosX(_selectedButton._position.y, 0.1f);
#endif
        }

        private void UpdateHorizontal()
        {
#if DoTweenInstalled
            _contentRect.DOAnchorPosX(_selectedButton._position.x - _substractFromFirst, 0.1f);
#endif
        }

        public void ListContent(int value)
        {
            _contentVector -= value * _listValue;
        }

        [ContextMenu(nameof(DebugView))]
        public void DebugView()
        {
            FixedUpdate();
            ListContent(1);
        }
    }

    [System.Serializable]
    internal class SingleScrollElement
    {
        internal SingleScrollElement(ScrollElement sentButton, Vector2 sentPosition) { button = sentButton; _position = sentPosition; }

        [SerializeField] internal ScrollElement button;
        [SerializeField] internal Vector2 _position;
    }
}