using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace InterestGames
{
    public class RoomCleaning_Platok : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public BoxCollider2D drawable;
        public BoxCollider2D drawable1;
        public BoxCollider2D drawable2;
        public BoxCollider2D drawable3;

        public Vector3 initialPosition;
        public float dropDistance = 2f;
        public float moveDuration = 0.25f;
        private bool _hasInteracted = false;

        private void Awake()
        {
            drawable1.enabled = false;
            drawable2.enabled = false;
            drawable3.enabled = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            drawable1.enabled = true;
            drawable2.enabled = true;
            drawable3.enabled = true;

            if (_hasInteracted == false)
            {
                initialPosition = transform.localPosition;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            var sp = new Vector3(eventData.position.x, eventData.position.y, 0f);
            var position = Camera.main != null ? Camera.main.ScreenToWorldPoint(sp) : transform.position;
            position.z = 0f;
            transform.position = position;
        }

        public async void OnPointerUp(PointerEventData eventData)
        {
            drawable1.enabled = false;
            drawable2.enabled = false;
            drawable3.enabled = false;

            await transform.DOLocalMove(initialPosition, moveDuration).AsyncWaitForCompletion();
        }
    }
}