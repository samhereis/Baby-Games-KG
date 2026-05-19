using DG.Tweening;
using FX;
using Helpers;
using Identifiers;
using Services;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Modes.Puzzle
{
    public class Puzzle_PuzzleState : StateMachine_StateBase
    {
        [SerializeField] private PuzzleActivity_Identifier _puzzleActivity;
        [SerializeField] private ObjectDividerBase _objectDivider;
        [SerializeField] private HintHand_Drag _hintHand;

        private Vector3 _mousePosition;
        private Vector3 _offset;
        private PuzzlePiece _currentDraggable;

        private SpriteRenderer _wholeImage_Copy;

        private void Awake()
        {
            if (_puzzleActivity == null) { _puzzleActivity = GetComponentInParent<PuzzleActivity_Identifier>(true); }
            if (_puzzleActivity == null) { _puzzleActivity = FindObjectOfType<PuzzleActivity_Identifier>(true); }
            if (_objectDivider == null) { _objectDivider = _puzzleActivity.GetComponentInChildren<ObjectDividerBase>(true); }
            if (_hintHand == null) { _hintHand = GetComponent<HintHand_Drag>(); }
        }

        public async override Task PreInittialize()
        {
            await base.PreInittialize();

            gameObject.SetActive(true);
            foreach (var item in GetComponentsInChildren<PuzzlePiece>(true))
            {
                item.gameObject.SetActive(false);
            }

            foreach (var item in GetComponentsInChildren<PuzzlePiece>(true))
            {
                item.gameObject.SetActive(true);
                await AsyncHelper.NextFrame();

                item.Preinitialize(_puzzleActivity);
                item.gameObject.SetActive(false);

                item.transform.localScale = item.transform.localScale;
            }
        }

        public override async Task Enter()
        {
            await base.Enter();
            await _objectDivider.Divide();

            _objectDivider.onComplete += OnComplete;
            _objectDivider.onPlaced += OnPlaced;

            _wholeImage_Copy = Instantiate(_objectDivider.puzzle.wholeImage, _objectDivider.puzzle.wholeImage.transform).GetComponent<SpriteRenderer>();
            foreach (var item in _wholeImage_Copy.GetComponents<Component>())
            {
                if (item is SpriteRenderer) { continue; }
                if (item is Transform) { continue; }
                Destroy(item);
            }

            _wholeImage_Copy.transform.localPosition = Vector3.zero;
            _wholeImage_Copy.material = Resources.Load<Material>("Materials/Wave_DragZone");
            _objectDivider.puzzle.wholeImage.GetComponent<SpriteRenderer>().DOFade(0, 0);
        }

        public override async Task Exit()
        {
            await base.Exit();

            _objectDivider.onComplete -= OnComplete;
            _objectDivider.onPlaced -= OnPlaced;
        }

        public override void Tick()
        {
            base.Tick();

            var pointer = Pointer.current;
            if (pointer == null) return;

            if (pointer.press.wasPressedThisFrame)
            {
                TryCatch();
            }

            if (_currentDraggable != null && _currentDraggable.isCompleted) { return; }
            if (pointer.press.isPressed)
            {
                TryDrag(_currentDraggable);

                if (_objectDivider.CanPlace(_currentDraggable))
                {
                    _objectDivider.Place(_currentDraggable);
                }
            }

            if (_currentDraggable != null && _currentDraggable.isCompleted) { return; }
            if (pointer.press.wasReleasedThisFrame)
            {
                if (_currentDraggable?.isCompleted == false)
                {
                    _objectDivider.PutBack(_currentDraggable);
                }
            }
        }

        protected void TryCatch()
        {
            _mousePosition = Camera.main.ScreenToWorldPoint(Pointer.current.position.ReadValue());
            if (Physics2D.OverlapPoint(_mousePosition) is BoxCollider2D boxCollider2D && boxCollider2D.TryGetComponent(out PuzzlePiece puzzlePiece))
            {
                _currentDraggable = puzzlePiece;
                if (_currentDraggable != null)
                {
                    _currentDraggable.transform.DOKill();
                    _currentDraggable.OnBegginDrag();
                    _offset = _currentDraggable.transform.position - _mousePosition;
                }
            }
            else
            {
                _currentDraggable = null;
            }
        }

        private void TryDrag(PuzzlePiece puzzlePiece)
        {
            if (puzzlePiece != null)
            {
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Pointer.current.position.ReadValue());
                puzzlePiece.transform.position = mousePosition + _offset;
            }
        }

        private void OnPlaced(PuzzlePiece piece)
        {
            _hintHand?.objects.Remove(piece.transform);
        }

        private async void OnComplete()
        {
            DiService.Get<StateEnd_FX>()?.DoFX();

            _objectDivider.puzzle.wholeImage.GetComponent<SpriteRenderer>().DOFade(1, 0).OnComplete(() =>
            {
                Destroy(_wholeImage_Copy.gameObject);
            });

            foreach (var item in GetComponentsInChildren<PuzzlePiece>(true))
            {
                item.SetVisibility(false, 1);
            }

            _objectDivider.puzzle.mainImage[0].color = Color.white;

            await AsyncHelper.DelayFloat(2f);
            _nextState = _nextStateOnWin;
        }
    }
}