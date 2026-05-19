using DG.Tweening;
using Observables;
using Sounds;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Coocking
{
    public class CookingFeeding_Mixer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public ObservableValue<bool> isReady;

        [SerializeField] private Transform _tool;
        [SerializeField] private SpriteRenderer _testo;
        [SerializeField] private SpriteRenderer _toRotate;
        [SerializeField] private Transform _toRotate_Hand;
        [SerializeField] private Transform _hand_Icon;
        [SerializeField] private Transform _hand_Target;

        [SerializeField] private float _idleThreshold = 0.25f;
        [SerializeField] private float _growSpeed = 0.25f;
        [SerializeField] private float _rotationSpeed = 0.25f;
        [SerializeField] private float _rotationSpeed_Hand = 0.25f;
        [SerializeField] private AudioSource _mixerAudioSource;
        [SerializeField] private SoundAdvanced _mixerSound;

        [SerializeField] private Vector2 _moveClampX;
        [SerializeField] private Vector2 _moveClampY;
        private bool _hasStoppedMoving = false;
        private Vector3 _lastPointerPos;
        private float _lastMoveTime;
        private bool _pointerOver = false;
        private bool _isDragging = false;

        public void OnPointerEnter(PointerEventData eventData)
        {
            _pointerOver = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _pointerOver = false;
            if (_isDragging && !_hasStoppedMoving)
            {
                _hasStoppedMoving = true;
                Stop();
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDragging = true;
            _lastPointerPos = eventData.position;
            _lastMoveTime = Time.time;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector3 currentPos = eventData.position;
            if (currentPos != _lastPointerPos)
            {
                _lastPointerPos = currentPos;
                _lastMoveTime = Time.time;

                var position = _tool.transform.position;
                position.x = Mathf.Clamp(Camera.main.ScreenToWorldPoint(currentPos).x, _moveClampX.x, _moveClampX.y);
                position.y = Mathf.Clamp(Camera.main.ScreenToWorldPoint(currentPos).y, _moveClampY.x, _moveClampY.y);
                _tool.position = position;

                if (_testo.transform.localScale.x < 1f)
                {
                    _testo.transform.localScale += Vector3.one * (_growSpeed * Time.deltaTime);
                    _testo.DOFade(_testo.transform.localScale.x, 0.25f);
                    _testo.transform.DORotate(new Vector3(0, 0, 360 / _testo.transform.localScale.x), 0.25f);
                }
                else
                {
                    _testo.DOFade(1, 0.5f);
                }

                if (_hasStoppedMoving)
                {
                    Stop();
                    _hasStoppedMoving = false;
                }
                else
                {
                    if (_pointerOver) Do();
                    else Stop();
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;

            if (_testo.transform.localScale.x > 0.98f)
            {
                isReady.ChangeValue(true);
                transform.DOMoveY(-25, 1f);
                _tool.DOMoveY(250, 1f);
            }
        }

        private void Update()
        {
            if (_isDragging == false)
            {
                Stop();
            }

            if (_toRotate != null) { _toRotate?.transform.Rotate(0f, 0f, _rotationSpeed * Time.deltaTime); }
            if (_toRotate_Hand != null && _hand_Target != null && _hand_Icon)
            {
                _toRotate_Hand?.transform.Rotate(0f, 0f, _rotationSpeed_Hand * Time.deltaTime);
                _hand_Icon.transform.position = _hand_Target.position;
            }
        }

        public void Show()
        {
            transform.localScale = Vector3.zero;
            gameObject.SetActive(true);
            transform.DOScale(0.5f, 1f);

            _testo.gameObject.SetActive(true);

            var toolSize = _tool.transform.localScale;
            _tool.localScale = Vector3.zero;
            _tool.gameObject.SetActive(true);
            _tool.DOScale(toolSize, 1f);
        }

        private async void Do()
        {
            if (_mixerAudioSource == null) { return; }

            if (_mixerAudioSource.isPlaying == false)
            {
                _mixerAudioSource.clip = await _mixerSound.GetSound();
                _mixerAudioSource.Play();
            }
        }

        private void Stop()
        {
            if (_mixerAudioSource == null) { return; }
            _mixerAudioSource.Stop();
        }
    }
}