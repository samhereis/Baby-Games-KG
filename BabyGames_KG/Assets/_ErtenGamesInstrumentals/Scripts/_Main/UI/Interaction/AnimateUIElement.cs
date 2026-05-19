using CustomAttributes;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UI.Interaction
{
    [DisallowMultipleComponent]
    public class AnimateUIElement : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
        [SerializeField] private Transform _target;

        [FoldoutGroup("Reaction"), SerializeField] private Vector3 _positionOnHoverMin = new Vector3(-20, -20, -20);
        [FoldoutGroup("Reaction"), SerializeField] private Vector3 _positionOnHoverMax = new Vector3(20, 20, 20);
        [FoldoutGroup("Reaction"), SerializeField] private Vector3 _rotationOnHoverMin = new Vector3(-5, -5, -5);
        [FoldoutGroup("Reaction"), SerializeField] private Vector3 _rotationOnHoverMax = new Vector3(5, 5, 5);
        [FoldoutGroup("Reaction"), SerializeField] private float _onOverScale = 0.75f;

        [FoldoutGroup("Settings"), SerializeField] private bool _rotate = false;
        [FoldoutGroup("Settings"), SerializeField] private bool _scale = true;
        [FoldoutGroup("Settings"), SerializeField] private bool _move = false;
        [FoldoutGroup("Settings"), SerializeField] private bool _randomize = true;
        [FoldoutGroup("Settings"), SerializeField] private Ease _ease = Ease.OutBack;

        [FoldoutGroup("Timing"), SerializeField] private float _animationDuration = 0.5f;

        [FoldoutGroup("Components on hover"), SerializeField] protected GameObject[] _gameObjectsToDisableOnHover;
        [FoldoutGroup("Components on hover"), SerializeField] protected GameObject[] _gameObjectsToEnableOnHover;
        [FoldoutGroup("Components on hover"), SerializeField] protected MonoBehaviour[] _componentsToDisableOnHover;
        [FoldoutGroup("Components on hover"), SerializeField] protected MonoBehaviour[] _componentsToEnableOnHover;

        [SerializeField] private AnimateButtonsEvents _events;

        [Fg_De, SerializeField] private Vector3 _initialScale = Vector3.one;

        private Vector3 GetPositionToSetOnHover()
        {
            if (_randomize == false) { return _positionOnHoverMin; }

            if (_positionOnHoverMin.z >= _positionOnHoverMax.x) { _positionOnHoverMin.x = _positionOnHoverMax.x - 1; }
            if (_positionOnHoverMin.x >= _positionOnHoverMax.x) { _positionOnHoverMin.x = _positionOnHoverMax.x - 1; }
            if (_positionOnHoverMin.y >= _positionOnHoverMax.x) { _positionOnHoverMin.x = _positionOnHoverMax.x - 1; }

            var x = Random.Range(_positionOnHoverMin.x, _positionOnHoverMax.x);
            var y = Random.Range(_positionOnHoverMin.y, _positionOnHoverMax.y);
            var z = Random.Range(_positionOnHoverMin.z, _positionOnHoverMax.z);

            return new Vector3(x, y, z);
        }

        private Vector3 GetRotataionToSetOnHover()
        {
            if (_randomize == false) { return _rotationOnHoverMin; }

            if (_rotationOnHoverMin.z >= _rotationOnHoverMax.x) { _rotationOnHoverMin.x = _rotationOnHoverMax.x - 1; }
            if (_rotationOnHoverMin.x >= _rotationOnHoverMax.x) { _rotationOnHoverMin.x = _rotationOnHoverMax.x - 1; }
            if (_rotationOnHoverMin.y >= _rotationOnHoverMax.x) { _rotationOnHoverMin.x = _rotationOnHoverMax.x - 1; }

            var x = Random.Range(_rotationOnHoverMin.x, _rotationOnHoverMax.x);
            var y = Random.Range(_rotationOnHoverMin.y, _rotationOnHoverMax.y);
            var z = Random.Range(_rotationOnHoverMin.z, _rotationOnHoverMax.z);

            return new Vector3(x, y, z);
        }

        private void Awake()
        {
            if (_target == null) { _target = transform; }
            _initialScale = _target.localScale;
        }

        private void OnEnable()
        {
            _target.localScale = _initialScale;
            OnExit();
        }

        private void OnDisable()
        {
            _target.localScale = _initialScale;
        }

        private void OnDestroy()
        {
            _target?.DOKill();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_scale) { _target.DOScale(_onOverScale, _animationDuration).SetEase(_ease); }
            if (_move) { _target.DOLocalMove(GetPositionToSetOnHover(), _animationDuration).SetEase(_ease); }
            if (_rotate) { _target.DOLocalRotate(GetRotataionToSetOnHover(), _animationDuration).SetEase(_ease); }

            OnHover();

            _events._onHover?.Invoke();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _events._onClick?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_scale) { _target.DOScale(_initialScale, _animationDuration).SetEase(_ease); }
            if (_move) { _target.DOLocalMove(Vector3.zero, _animationDuration).SetEase(_ease); }
            if (_rotate) { _target.DOLocalRotate(Vector3.zero, _animationDuration).SetEase(_ease); }

            OnExit();

            _events._onExit?.Invoke();
        }

        private void OnHover()
        {
            foreach (var obj in _gameObjectsToDisableOnHover)
            {
                obj?.SetActive(false);
            }

            foreach (var obj in _gameObjectsToEnableOnHover)
            {
                obj?.SetActive(true);
            }

            foreach (var component in _componentsToDisableOnHover)
            {
                if (component != null) { component.enabled = false; }
            }

            foreach (var component in _componentsToEnableOnHover)
            {
                if (component != null) { component.enabled = true; }
            }
        }

        private void OnExit()
        {
            foreach (var obj in _gameObjectsToDisableOnHover)
            {
                obj?.SetActive(true);
            }

            foreach (var obj in _gameObjectsToEnableOnHover)
            {
                obj?.SetActive(false);
            }

            foreach (var component in _componentsToDisableOnHover)
            {
                if (component != null) { component.enabled = true; }
            }

            foreach (var component in _componentsToEnableOnHover)
            {
                if (component != null) { component.enabled = false; }
            }
        }
    }

    [System.Serializable]
    internal class AnimateButtonsEvents
    {
        [SerializeField] internal UnityEvent _onHover = new UnityEvent();
        [SerializeField] internal UnityEvent _onClick = new UnityEvent();
        [SerializeField] internal UnityEvent _onExit = new UnityEvent();
    }
}