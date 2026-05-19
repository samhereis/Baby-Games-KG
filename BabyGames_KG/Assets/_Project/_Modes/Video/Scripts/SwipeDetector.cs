using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Video
{
    public class SwipeDetector : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public Action onSwipeUp;
        public Action onSwipeDown;

        public float swipeThreshold = 50f;

        private Vector2 _startPos;

        public void OnPointerDown(PointerEventData eventData)
        {
            _startPos = eventData.position;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Vector2 endPos = eventData.position;
            float deltaY = endPos.y - _startPos.y;

            if (deltaY > swipeThreshold)
            {
                onSwipeUp?.Invoke();
            }
            else
            {
                onSwipeDown?.Invoke();
            }
        }
    }
}