using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace UI
{
    public class DragEvents : MonoBehaviour
    {
        public static DragEvents instance;

        public Action onBeggingDrag;
        public Action onEndDrag;

        public Action onSwipeRight;
        public Action onSwipeLeft;
        public Action onSwipeUp;
        public Action onSwipeDown;

        [FoldoutGroup("Settings"), SerializeField] private float _distanceToUpDownDragDetect = 50;
        [FoldoutGroup("Settings"), SerializeField] private float _distanceToRightLeftDragDetect = 100;

        [FoldoutGroup("Debug"), SerializeField] private Vector2 _startPos;
        [FoldoutGroup("Debug"), SerializeField] private Vector2 _fingerPos;

        private void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _startPos = Input.mousePosition;
                onBeggingDrag?.Invoke();
            }

            if (Input.GetMouseButton(0))
            {
                _fingerPos = Input.mousePosition;
                CheckSwipe();
            }

            if (Input.GetMouseButtonUp(0))
            {
                onEndDrag?.Invoke();
            }
        }

        private void CheckSwipe()
        {
            if (verticalMove() > _distanceToUpDownDragDetect && verticalMove() > horizontalValMove())
            {
                if (_startPos.y - _fingerPos.y > 0) OnSwipeDown(); else if (_startPos.y - _fingerPos.y < 0) OnSwipeUp();
            }

            if (horizontalValMove() > _distanceToRightLeftDragDetect && horizontalValMove() > verticalMove())
            {
                if (_startPos.x - _fingerPos.x > 0) OnSwipeRight(); else if (_startPos.x - _fingerPos.x < 0) OnSwipeLeft();
            }

            _startPos = Input.mousePosition;
        }

        private float verticalMove()
        {
            return Mathf.Abs(_startPos.y - _fingerPos.y);
        }

        private float horizontalValMove()
        {
            return Mathf.Abs(_startPos.x - _fingerPos.x);
        }

        private void OnSwipeRight()
        {
            onSwipeRight?.Invoke();
        }

        private void OnSwipeLeft()
        {
            onSwipeLeft?.Invoke();
        }

        private void OnSwipeUp()
        {
            onSwipeUp?.Invoke();
        }

        private void OnSwipeDown()
        {
            onSwipeDown?.Invoke();
        }
    }
}