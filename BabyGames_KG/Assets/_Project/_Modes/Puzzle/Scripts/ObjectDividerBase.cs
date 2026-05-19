using _Project.Scripts.Sound;
using Agents;
using DG.Tweening;
using Helpers;
using Services;
using Sounds;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Modes.Puzzle
{
    [RequireComponent(typeof(Move_FX))]
    [RequireComponent(typeof(Sound_FX))]
    public class ObjectDividerBase : MonoBehaviour
    {
        public Action<PuzzlePiece> onPlaced;
        public Action onComplete;

        public List<PuzzlePiece> _spawnedItems = new();
        public List<PuzzlePiece> _puzzlePieces = new();

        [SerializeField] private float autoMoveDistance = 2f;
         [field: SerializeField]  private float disposeAnimation_ScaleUp { get; set; } = 1.1f;
        [SerializeField] protected float _animationDuration = 0.25f;

        [SerializeField] protected AnimationAgent _animator;

        [SerializeField] private List<PlacePosition_Identifier> _placePositions_Safe = new();

        [field: SerializeField] public Puzzle puzzle { get; set; }

        [Space]
        [SerializeField] private Move_FX _move_FX;
        [SerializeField] private Sound_FX _sound_FX;
        [SerializeField] private Sound _separateSound;

        public Vector2 screenBounds { get; set; }
        private bool _separated = false;

        private void Awake()
        {
            DiService.Inject(this);

            _move_FX = GetComponent<Move_FX>();
            _sound_FX = GetComponent<Sound_FX>();
        }

        private void Update()
        {
            if (_separated)
            {
                foreach (var puzzlePieces in _spawnedItems)
                {
                    if (puzzlePieces.isCompleted)
                    {
                        continue;
                    }

                    Vector3 viewPos = puzzlePieces.transform.position;
                    viewPos.x = Mathf.Clamp(viewPos.x, -screenBounds.x + puzzlePieces.boxCollider.size.x / 2, screenBounds.x - puzzlePieces.boxCollider.size.x / 2);
                    viewPos.y = Mathf.Clamp(viewPos.y, -screenBounds.y + puzzlePieces.boxCollider.size.y / 3.5f, screenBounds.y - puzzlePieces.boxCollider.size.y / 2.5f);
                    puzzlePieces.transform.position = viewPos;
                }
            }
        }

        public virtual async Task Divide()
        {
            screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width - 50, Screen.height - 50, Camera.main.transform.position.z));
            await AsyncHelper.Skip();
        }

        public async Task SeparateAsync()
        {
            bool done = false;
            _animator = GetComponent<AnimationAgent>();

            if (_animator != null)
            {
                _animator.onAnimationCallback += (eventName) =>
                {
                    if (eventName == "Separate")
                    {
                        Separate();
                        done = true;
                    }
                };

                _animator.animator.Play("Separation");
            }
            else
            {
                Separate();
                done = true;
            }

            while (done == false)
            {
                await AsyncHelper.Skip();
            }
        }

        protected async void DoOutlineAnimation()
        {
            foreach (var puzzlePiece in _spawnedItems)
            {
                puzzlePiece.gameObject.SetActive(true);
                puzzlePiece.SetVisibility(true, 1);
                await AsyncHelper.DelayFloat(_animationDuration);
            }
        }

        protected virtual async void Separate()
        {
            Sound_FX.audioPlayer.TryPlay(_separateSound);

            foreach (var puzzlePiece in _spawnedItems)
            {
                var position = GetRandomPosition();
                puzzlePiece.SetRandomPosition(position);
            }

            await AsyncHelper.DelayFloat(1f);
            _separated = true;
        }

        public bool CanPlace(PuzzlePiece puzzlePiece)
        {
            if (puzzlePiece == null) { return false; }
            if (puzzlePiece.isCompleted) { return false; }

            var distanceFromOriginalPosition = Vector3.Distance(puzzlePiece.transform.position, puzzlePiece.originalPosition);
            return distanceFromOriginalPosition < autoMoveDistance;
        }

        public async void Place(PuzzlePiece puzzlePiece)
        {
            if (puzzlePiece == null) { return; }
            if (puzzlePiece.isCompleted) { return; }

            puzzlePiece.isCompleted = true;

            puzzlePiece.transform.DOKill();
            puzzlePiece.boxCollider.enabled = false;

            var clip = await _sound_FX.PlayAsync(Sound_Effect.Success);
            _move_FX.Success(puzzlePiece.transform);

            puzzlePiece.OnPlaced();
            _puzzlePieces.Add(puzzlePiece);

            onPlaced?.Invoke(puzzlePiece);

            if (_puzzlePieces.Count >= _spawnedItems.Count)
            {
                _separated = false;
                onComplete?.Invoke();
            }
        }

        public async void PutBack(PuzzlePiece puzzlePiece)
        {
            if (puzzlePiece == null) { return; }
            if (puzzlePiece.isCompleted) { return; }

            puzzlePiece.transform.DOKill();
            puzzlePiece.boxCollider.enabled = false;

            var clip = await _sound_FX.PlayAsync(Sound_Effect.Failure);
            _move_FX.Fail(puzzlePiece.transform);
            await AsyncHelper.DelayFloat(clip.length);

            puzzlePiece.GoToOffset();

            puzzlePiece.boxCollider.enabled = true;
            await AsyncHelper.DelayFloat(0.25f);
        }

        public async Task DoEndingAnimation()
        {
            float animationDuration = 0.1f;

            foreach (var position in _spawnedItems)
            {
                Vector3 originalScale = position.transform.localScale;

                position.transform.DOScale(originalScale * disposeAnimation_ScaleUp, animationDuration).OnComplete(() =>
                {
                    position.transform.DOScale(originalScale, animationDuration);
                });

                await AsyncHelper.DelayFloat(animationDuration);
            }
        }

        public async Task DisposeAsync()
        {
            await AsyncHelper.Skip();

            foreach (var position in _spawnedItems)
            {
                position.DOKill();
                Destroy(position.gameObject);
            }
        }

        public Vector3 GetRandomPosition()
        {
            _placePositions_Safe.RemoveNulls();

            var positionIdentifier = _placePositions_Safe.GetRandom();

            if (positionIdentifier == null)
            {
                Debug.LogWarning("Could not find position!");

                return new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), 0);
            }

            _placePositions_Safe.Remove(positionIdentifier);

            return positionIdentifier.GetPosition();
        }
    }
}