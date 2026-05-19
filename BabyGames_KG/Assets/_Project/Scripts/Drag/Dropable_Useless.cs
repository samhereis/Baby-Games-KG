using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Coocking
{
    public class Dropable_Useless : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public BoxCollider boxCollider;
        public Vector3 initialPosition;
        public float dropDistance = 2f;
        public float moveDuration = 0.25f;

        private bool _hasInteracted = false;

        private void Awake()
        {
            boxCollider = GetComponent<BoxCollider>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_hasInteracted == false)
            {
                initialPosition = transform.position;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            var sp = new Vector3(eventData.position.x, eventData.position.y, 0f);
            var world = Camera.main != null ? Camera.main.ScreenToWorldPoint(sp) : transform.position;
            world.z = 0f;
            transform.position = world;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            transform.DOMove(initialPosition, moveDuration);
        }
    }
}
