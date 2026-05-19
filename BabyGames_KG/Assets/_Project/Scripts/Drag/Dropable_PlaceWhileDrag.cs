using _Project.Scripts.Sound;
using CustomAttributes;
using DG.Tweening;
using Helpers;
using Identifiers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Project.Scripts.GameFeel;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gameplay
{
    [RequireComponent(typeof(ObjectJuicer))]
    public class Dropable_PlaceWhileDrag : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Action<Dropable_PlaceWhileDrag, GameObject> onCopyAdded;

        public Action<Dropable_PlaceWhileDrag> onMouseDown;
        public Action<Dropable_PlaceWhileDrag> onMouseUpAsButton;
        public Action<Dropable_PlaceWhileDrag> onMouseUp;

        public Action<Dropable_PlaceWhileDrag> onStartDrag;
        public Action<Dropable_PlaceWhileDrag> onEndDrag;

        public BoxCollider _boxCollider;

        [Space]
        public PlacesHolder placesHolder;
        public Transform parentToSet;
        public List<GameObject> _placePrefabs = new();

        [Space]
        public Sound_FX _soundFX;
        public ObjectJuicer _objectJuicer;

        [Space]
        public float dropDistance = 2;

        [Fg_De] public List<GameObject> copies = new();
        [Fg_De] public Vector3 initialPosition;
        [Fg_De] public Transform lasNearestPosition;
        [Fg_De] public Transform lasSpawedTo;
        [Fg_De] public Vector3 offset;
        [Fg_De] public bool _isDragging = false;

        public Vector3 nearestPosition
        {
            get
            {
                lasNearestPosition = placesHolder.secondary[0];

                float minSqrDistance = (lasNearestPosition.position - transform.position).sqrMagnitude;

                for (int i = 1; i < placesHolder.secondary.Count; i++)
                {
                    float sqrDist = (placesHolder.secondary[i].position - transform.position).sqrMagnitude;
                    if (sqrDist < minSqrDistance)
                    {
                        minSqrDistance = sqrDist;
                        lasNearestPosition = placesHolder.secondary[i];
                    }
                }
                return lasNearestPosition.position;
            }
        }

        private void Start()
        {
            initialPosition = transform.localPosition;
            if (TryGetComponent<PanelItem>(out var panelItem)) { initialPosition = panelItem.initialPosition; }

            if (_boxCollider == null) { _boxCollider = GetComponentInChildren<BoxCollider>(true); }

            if (_soundFX == null) { _soundFX = GetComponent<Sound_FX>(); }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_boxCollider.enabled == false) { return; }

            transform.DOKill();
            onMouseDown?.Invoke(this);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_boxCollider.enabled == false) { return; }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_boxCollider.enabled == false) { return; }

            if (_soundFX != null) { _soundFX.Play(Sound_Effect.StartDrag); }

            _isDragging = true;
            Move(eventData.position);
            onStartDrag?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_boxCollider.enabled == false) { return; }
            if (_isDragging == false) { return; }

            Move(eventData.position);

            var distance = Vector3.Distance(transform.position, nearestPosition);
            var canPlace = distance < dropDistance;

            if (lasNearestPosition != lasSpawedTo)
            {
                Place();
                lasSpawedTo = lasNearestPosition;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_isDragging == false) { return; }
            _isDragging = false;

            if (_boxCollider.enabled == false) { return; }

            PlaceBack();

            _isDragging = false;
            onMouseUp?.Invoke(this);
        }

        public void Move(Vector3 uiPosition)
        {
            var position = Camera.main.ScreenToWorldPoint(uiPosition);

            position.z = 0;
            transform.position = position + offset;
        }

        public void Place()
        {
            foreach (var item in lasNearestPosition.GetComponentsInChildren<PanelItem>())
            {
                if (item is PanelItem panelItem)
                {
                    return;
                }
            }

            var placePrefab = _placePrefabs.GetRandom();

            var position = lasNearestPosition.position;
            var distance = Vector3.Distance(transform.position, position);
            var canPlace = distance < dropDistance;
            var copyScale = placePrefab.transform.localScale;

            var copy = Instantiate(placePrefab, lasNearestPosition);
            copy.transform.localScale = Vector3.zero;
            copy.transform.position = position;
            copy.transform.localEulerAngles = new Vector3(0, 0, UnityEngine.Random.Range(-359, 359));
            copy.transform.DOScale(copyScale, 0.5f).SetEase(Ease.OutBack);

            if (_soundFX != null) { _soundFX.Play(Sound_Effect.StartDrag); }

            onCopyAdded?.Invoke(this, copy);
            copies.SafeAdd(copy);
        }

        public async void PlaceBack()
        {
            await PlaceBackAsync();
        }

        public async Task PlaceBackAsync()
        {
            _boxCollider.enabled = false;

            onEndDrag?.Invoke(this);

            transform.DOKill();
            await transform.DOLocalMove(initialPosition, 0.5f).SetEase(Ease.OutBack).AsyncWaitForCompletion();

            _boxCollider.enabled = true;
        }
    }
}