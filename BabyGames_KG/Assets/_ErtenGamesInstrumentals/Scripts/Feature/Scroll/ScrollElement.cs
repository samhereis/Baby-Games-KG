#if DoTweenInstalled
using DG.Tweening;
#endif

using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(DragEvents), typeof(Image))]
    public sealed class ScrollElement : MonoBehaviour
#if OdinInspectorInstalled
        , ISelfValidator
#endif
    {
        public static bool isInAction = false;
        private bool _canMove = false;
        private bool canMove { get => _canMove; set { _canMove = value; isInAction = value; } }
        private bool _isScrolling = false;
        public bool isScrolling { get => _isScrolling; set { _isScrolling = value; isInAction = value; } }

        [Header("Components")]
        [Required]
        [SerializeField] private ScrollSnapRect _scrollSnapRect;

        [Required]
        [SerializeField] private DragEvents _dragEvents;

        [Required]
        [SerializeField] private Image _image;

#if DoTweenInstalled
        [Header("Settings")]
        [SerializeField] private float _enableScale = 1.2f;
        [SerializeField] private float _disableScale = 1f;
#endif

        [Header("Debug")]
        [SerializeField] private Vector3 _initialPosition;

        [SerializeField] private float _start;
        [SerializeField] private float _startParentPosition;

        public Action onEnable;
        public Action onDisable;

#if OdinInspectorInstalled
        public void Validate(SelfValidationResult selfValidationResult)
        {
            if (_dragEvents == null)
            {
                selfValidationResult.AddWarning("Drag Events is null").WithFix(() =>
                {
                    _dragEvents = GetComponent<DragEvents>();
                });
            }

            if (_image == null)
            {
                selfValidationResult.AddWarning("Image is null").WithFix(() =>
                {
                    _image = GetComponent<Image>();
                });
            }
        }
#endif

        private void OnEnable()
        {
            RegisterEvents();

            if (_scrollSnapRect == null)
            {
                _scrollSnapRect = GetComponentInParent<ScrollSnapRect>();
                if (_scrollSnapRect == null) _scrollSnapRect = FindFirstObjectByType<ScrollSnapRect>();
            }

            _scrollSnapRect?.RegidterElement(this);
        }

        private void OnDisable()
        {
            DeregisterEvents();
            _scrollSnapRect?.DeregidterElement(this);
        }

        private void Update()
        {
            if (canMove && _isScrolling == false) transform.position = Input.mousePosition;
        }

        private void RegisterEvents()
        {
            _dragEvents.onSwipeDown += SetCanMove;
            _dragEvents.onBeggingDrag += OnBegginLeftDrag;
            _dragEvents.onSwipeLeft += OnDragHorizontal;
            _dragEvents.onSwipeRight += OnDragHorizontal;

            _dragEvents.onEndDrag += OnEndDrag;
        }

        private void DeregisterEvents()
        {
            _dragEvents.onSwipeDown -= SetCanMove;
            _dragEvents.onBeggingDrag -= OnBegginLeftDrag;
            _dragEvents.onSwipeLeft -= OnDragHorizontal;
            _dragEvents.onSwipeRight -= OnDragHorizontal;

            _dragEvents.onEndDrag -= OnEndDrag;
        }

        private void SetCanMove()
        {
            canMove = true;

            if (_initialPosition == Vector3.zero) _initialPosition = transform.localPosition;

            _dragEvents.onSwipeDown -= SetCanMove;
            _dragEvents.onSwipeLeft -= OnDragHorizontal;
            _dragEvents.onSwipeRight -= OnDragHorizontal;

            _dragEvents.onEndDrag += SetCantMove;
        }

        private void SetCantMove()
        {
            canMove = false;

#if DoTweenInstalled
            transform.DOLocalMove(_initialPosition, 1);
#endif

            _dragEvents.onSwipeDown += SetCanMove;
            _dragEvents.onSwipeLeft += OnDragHorizontal;
            _dragEvents.onSwipeRight += OnDragHorizontal;
        }

        private void OnBegginLeftDrag()
        {
            _start = Input.mousePosition.x;
            _startParentPosition = transform.parent.position.x;
        }

        private void OnEndDrag()
        {
            canMove = false;
            isScrolling = false;
        }

        private void OnDragHorizontal()
        {
            if (canMove == false)
            {
                _isScrolling = true;

#if DoTweenInstalled
                transform.parent.DOMoveX(_startParentPosition + (Input.mousePosition.x - _start), 0);
#endif
            }
        }

        public void Enable()
        {
#if DoTweenInstalled
            transform.DOKill();

            transform.DOScale(_enableScale, 1f);
            onEnable?.Invoke();
#endif
        }

        public void Disable()
        {
#if DoTweenInstalled
            transform.DOKill();

            transform.DOScale(_disableScale, 1f);
            onDisable?.Invoke();
#endif
        }
    }
}