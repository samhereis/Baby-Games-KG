using _Project.Scripts.Sound;
using DG.Tweening;
using Gameplay;
using Helpers;
using Observables;
using Sirenix.OdinInspector;
using Sounds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _Project.Scripts.GameFeel;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CoockingSalade
{
    public class Cooking_Cuttable : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Action<Cooking_Cuttable> onStartedPlacing;

        public ObservableValue<bool> isCut = new("isCompleted");
        public ObservableValue<bool> isPlaced = new("isPlaced");

        public List<SpriteRenderer> pieces = new();

        public List<Cooking_CutPiece> cutPieces = new();
        public Cooking_CutPiece currentPiece;
        public BoxCollider currentCollider;

        public Transform indicator;
        public Transform indicator_end;

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
            if (currentCollider.enabled == false) { return; }

            MoveWithPointer(eventData.position);
            foreach (var forceAltPiece in forceAltPieces) forceAltPiece.sortingOrder = dragSorting;

            if (_soundFX != null) { _soundFX.Play(Sound_Effect.StartDrag); }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (currentCollider.enabled == false) { return; }

            MoveWithPointer(eventData.position);
            foreach (var forceAltPiece in forceAltPieces) forceAltPiece.sortingOrder = dragSorting;
        }

        public async void OnEndDrag(PointerEventData eventData)
        {
            if (currentCollider.enabled == false) return;

            var worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
            worldPos.z = 0;

            if (Vector3.Distance(worldPos, targetPlace.main.position) < dropDistance)
            {
                onStartedPlacing?.Invoke(this);

                _soundFX?.Play(Sound_Effect.Success);
                Move_FX.MakeDoneParticle(transform.position, 3);

                transform.DOScale(dropScale, 1);
                await transform.DOMove(targetPlace.main.position + Vector3.up * 5, 1).SetEase(Ease.OutBack).AsyncWaitForCompletion();
                Reset_Sorting();
                transform.DOMove(targetPlace.main.position, 0.25f).SetEase(Ease.OutBack);

                isPlaced.ChangeValue(true);
            }
            else
            {
                transform.DOLocalMove(initialPositionLocal, 0.25f).SetEase(Ease.OutBack);
                _soundFX?.Play(Sound_Effect.Failure);
            }

            void Reset_Sorting()
            {
                foreach (var forceAltPiece in forceAltPieces) forceAltPiece.sortingOrder = normalSorting;
            }
        }

        [Button]
        public async void Initialize(bool animate = true)
        {
            foreach (var item in forceAltPieces) item.gameObject.SetActive(false);

            cutPieces = GetComponentsInChildren<Cooking_CutPiece>(true).ToList();
            foreach (var item in cutPieces) item.cutSound = cutSound;

            if (currentCollider == null) currentCollider = GetComponent<BoxCollider>();
            currentCollider.enabled = false;

            gameObject.SetActive(true);

            if (animate)
            {
                initialScale = transform.localScale;
                transform.localScale = Vector3.zero;
                await transform.DOScale(initialScale, 1).SetEase(Ease.OutBack).AsyncWaitForCompletion();
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
            var position = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
            position.z = 0;
            transform.position = position;
        }
    }
}