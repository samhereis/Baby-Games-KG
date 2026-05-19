using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public sealed class ObjectRotator_UserInput : MonoBehaviour, IDragHandler, IBeginDragHandler
    {
        public Action<PointerEventData> onRotate;
        public Action<PointerEventData> onBeginRotate;

        [Required]
        [SerializeField] private Transform[] _rotatedObject;

        [FoldoutGroup("Settings"), SerializeField] private bool _canRotate = true;

        [FoldoutGroup("Debug"), SerializeField] private Vector2 _lastPosition;

        public void OnBeginDrag(PointerEventData eventData)
        {
            onBeginRotate?.Invoke(eventData);
            _lastPosition = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            onRotate?.Invoke(eventData);

            if (_canRotate)
            {
                var dir = _lastPosition - eventData.position;

                foreach (Transform t in _rotatedObject) { t?.Rotate(0, dir.x, 0); }

                _lastPosition = eventData.position;
            }
        }
    }
}