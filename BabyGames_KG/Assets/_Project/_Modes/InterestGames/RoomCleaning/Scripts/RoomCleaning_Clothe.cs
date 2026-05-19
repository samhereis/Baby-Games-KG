using DG.Tweening;
using Gameplay;
using Observables;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace InterestGames
{
    public class RoomCleaning_Clothe : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public ObservableValue<bool> isDropped;
        public Action<RoomCleaning_Clothe> onFinish;

        public SpriteRenderer hint;

        public BoxCollider boxCollider;
        public PlacesHolder targetPosition;
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
            var position = Camera.main != null ? Camera.main.ScreenToWorldPoint(sp) : transform.position;
            position.z = 0f;
            transform.position = position;
        }

        public async void OnPointerUp(PointerEventData eventData)
        {
            if (Vector3.Distance(transform.position, targetPosition.position) < dropDistance)
            {
                boxCollider.enabled = false;
                isDropped.ChangeValue(true);

                await transform.DOMove(targetPosition.position, moveDuration).AsyncWaitForCompletion();
                onFinish?.Invoke(this);
            }
            else
            {
                await transform.DOMove(initialPosition, moveDuration).AsyncWaitForCompletion();
            }
        }
    }
}
