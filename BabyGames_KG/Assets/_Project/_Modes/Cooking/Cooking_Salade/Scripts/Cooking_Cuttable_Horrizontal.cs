using DG.Tweening;
using Gameplay;
using Helpers;
using Observables;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _Project.Scripts.GameFeel;
using _Project.Scripts.Sound;
using Sounds;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CoockingSalade
{
    public class Cooking_Cuttable_Horrizontal : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Action<Cooking_Cuttable_Horrizontal> onStartedPlacing;

        public ObservableValue<bool> isCut = new("isCompleted");
        public ObservableValue<bool> isPlaced = new("isPlaced");

        public List<SpriteRenderer> pieces = new();

        public List<Cooking_CutPiece_Horizontal> cutPieces = new();
        public Cooking_CutPiece_Horizontal currentPiece;
        public BoxCollider currentCollider;

        public Transform indicator;
        public Transform indicator_End;

        public PlacesHolder targetPlace;
        public float dropDistance = 3;
        public Vector3 dropScale = Vector3.one;

        [FoldoutGroup("Has Full Piece")] public List<SpriteRenderer> forceAltPieces = new();
        [FoldoutGroup("Has Full Piece")] public int dragSorting = 0;
        [FoldoutGroup("Has Full Piece")] public int normalSorting = 0;

        [FoldoutGroup("Audio")] public Sound cutSound;
        [FoldoutGroup("Audio")] public Sound_FX _soundFX;

        public Vector3 initialScale = Vector3.one;

        public Vector3 initialPositionLocal = Vector3.one;
        [SerializeField] private int _currentPieceIndex = 0;

        [Button]
        private void Validate()
        {
            pieces = GetComponentsInChildren<SpriteRenderer>().ToList();
        }

        private void Awake()
        {
            indicator?.DOScale(Vector3.zero, 0.25f);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            initialPositionLocal = transform.localPosition;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            MoveWithPointer(eventData.position);
            foreach (var s in forceAltPieces) { s.sortingOrder = dragSorting; }
        }

        public void OnDrag(PointerEventData eventData)
        {
            MoveWithPointer(eventData.position);
            foreach (var s in forceAltPieces) { s.sortingOrder = dragSorting; }
        }

        public async void OnEndDrag(PointerEventData eventData)
        {
            var worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
            worldPos.z = 0f;

            if (Vector3.Distance(worldPos, targetPlace.main.position) < dropDistance)
            {
                onStartedPlacing?.Invoke(this);

                transform.DOScale(dropScale, 1f);
                await transform.DOMove(targetPlace.main.position + Vector3.up * 5f, 1f).SetEase(Ease.OutBack).AsyncWaitForCompletion();
                ResetSorting();
                transform.DOMove(targetPlace.main.position, 0.25f).SetEase(Ease.OutBack);

                isPlaced.ChangeValue(true);
            }
            else
            {
                transform.DOLocalMove(initialPositionLocal, 0.25f).SetEase(Ease.OutBack);
            }

            void ResetSorting()
            {
                foreach (var s in forceAltPieces) { s.sortingOrder = normalSorting; }
            }
        }

        [Button]
        public async Task Initialize(bool animateScale = true)
        {
            foreach (var item in forceAltPieces) { item.gameObject.SetActive(false); }
            cutPieces = GetComponentsInChildren<Cooking_CutPiece_Horizontal>(true).ToList();

            foreach (var item in cutPieces) { item.cutSound = cutSound; }

            if (currentCollider == null) currentCollider = GetComponent<BoxCollider>();
            currentCollider.enabled = false;

            if (animateScale)
            {
                initialScale = transform.localScale;
                transform.localScale = Vector3.zero;
            }

            gameObject.SetActive(true);

            if (animateScale)
            {
                await transform.DOScale(initialScale, 1f).SetEase(Ease.OutBack).AsyncWaitForCompletion();
            }

            await GoToNext();
        }

        private async void OnDashCompleted(bool value)
        {
            if (cutPieces.TrueForAll(x => x.isCompleted.value == true))
            {
                foreach (var item in cutPieces) item.isCompleted.RemoveListener(OnDashCompleted);
                isCut.ChangeValue(true);
                return;
            }

            _currentPieceIndex++;
            await GoToNext();
        }

        [Button]
        private async Task GoToNext()
        {
            currentPiece = cutPieces[_currentPieceIndex];
            foreach (var item in cutPieces)
            {
                if (item == currentPiece)
                {
                    item.isCompleted.AddListener(OnDashCompleted);
                    item.gameObject.SetActive(true);

                    await AsyncHelper.NextFrame();
                    currentPiece.Initialize(indicator);
                }
                else
                {
                    item.gameObject.SetActive(false);
                    item.isCompleted.RemoveListener(OnDashCompleted);
                }
            }
        }

        private void MoveWithPointer(Vector2 screenPos)
        {
            var p = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
            p.z = 0f;
            transform.position = p;
        }
    }
}