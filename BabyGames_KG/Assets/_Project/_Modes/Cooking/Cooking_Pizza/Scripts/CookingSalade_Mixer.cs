using DG.Tweening;
using Observables;
using Sounds;
using Spine.Unity;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Coocking
{
    public class CookingSalade_Mixer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public ObservableValue<bool> isReady;

        [SerializeField] private Transform _tool;
        [SerializeField] private SkeletonAnimation _miska;
        [SerializeField] private Transform _testo;

        [SerializeField] private float _idleThreshold = 0.25f;
        [SerializeField] private float _growSpeed = 0.25f;
        [SerializeField] private AudioSource _mixerAudioSource;
        [SerializeField] private SoundAdvanced _mixerSound;

        [SerializeField] private Vector2 _moveClamp;
        private bool _hasStoppedMoving = false;
        private Vector3 _lastPointerPos;
        private float _lastMoveTime;
        private bool _pointerOver = false;
        private bool _isDragging = false;

        private void Awake()
        {
            _testo.localScale = Vector3.zero;
        }

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
                position.x = Mathf.Clamp(Camera.main.ScreenToWorldPoint(currentPos).x, _moveClamp.x, _moveClamp.y);
                _tool.position = position;

                if (_testo.localScale.x < 1f)
                {
                    _testo.localScale += Vector3.one * (_growSpeed * Time.deltaTime);
                }
                else
                {
                    _testo.localScale = Vector3.one;
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
            _miska.AnimationState.ClearTracks();
            _miska.AnimationName = "";

            if (_testo.localScale.x > 0.98f)
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
        }

        public void Show()
        {
            transform.localScale = Vector3.zero;
            gameObject.SetActive(true);
            transform.DOScale(0.5f, 1f);

            var toolSize = _tool.transform.localScale;
            _tool.localScale = Vector3.zero;
            _tool.gameObject.SetActive(true);
            _tool.DOScale(toolSize, 1f);
        }

        private async void Do()
        {
            if (_miska.AnimationName != "round_testo")
            {
                _miska.AnimationName = "round_testo";
            }
            _miska.timeScale = 1f;

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

            _miska.timeScale = 0f;
            _mixerAudioSource.Stop();
        }
    }
}
