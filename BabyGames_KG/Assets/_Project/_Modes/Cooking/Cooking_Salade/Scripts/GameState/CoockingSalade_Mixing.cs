using CustomAttributes;
using DG.Tweening;
using FX;
using Helpers;
using Services;
using Sirenix.OdinInspector;
using Sounds;
using Spine.Unity;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CoockingSalade
{
    [RequireComponent(typeof(BoxCollider))]
    public class CoockingSalade_Mixing : CoockingSalade_StateBase, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Transform _plate;
        [SerializeField] private SkeletonAnimation _plateAnimation;
        [SerializeField] private Transform _vegerables;

        [SerializeField] private float _idleThreshold = 0.25f;
        [SerializeField] private float _mixTime = 3;

        [SerializeField, FoldoutGroup("Sound")] private SoundAdvanced _mixingSound;
        [SerializeField, FoldoutGroup("Sound")] private AudioSource _audioSource;

        private Vector3 _lastPointerPos;
        private bool _isDragging = false;

        [Fg_De, SerializeField] private float _lastMoveTime;

        public override async Task Enter()
        {
            await base.Enable();

            await AsyncHelper.NextFrame();

            _plate.gameObject.SetActive(false);
            _vegerables.gameObject.SetActive(false);
            _plateAnimation.gameObject.SetActive(true);

            _plate.transform.DOMoveX(0, 1);
            _plateAnimation.transform.DOMoveX(0, 1);
            _plateAnimation.transform.DOMoveY(-1.25f, 1);
            _plateAnimation.transform.DOScale(1.1f, 1);

            if (_audioSource == null)
            {
                if (TryGetComponent<AudioSource>(out AudioSource audioSource))
                {
                    _audioSource = audioSource;
                }
                else
                {
                    _audioSource = gameObject.AddComponent<AudioSource>();
                }
            }
            if (_audioSource != null)
            {
                _audioSource.clip = await _mixingSound.GetSound();
                _audioSource.loop = true;
                _audioSource.DOFade(0, 0.25f).OnComplete(() =>
                {
                    _audioSource.Play();
                });
            }
        }

        public override async Task Disable()
        {
            if (_lastMoveTime < _mixTime)
            {
                _plateAnimation.gameObject.SetActive(false);
            }

            await base.Disable();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDragging = true;
            _lastPointerPos = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector3 currentPos = eventData.position;
            if (currentPos != _lastPointerPos)
            {
                _lastPointerPos = currentPos;
                _lastMoveTime += Time.deltaTime;

                Do();
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Stop();

            if (isDone) { return; }

            _isDragging = false;
            _plateAnimation.AnimationState.ClearTracks();
            _plateAnimation.AnimationName = "";

            if (_lastMoveTime > _mixTime)
            {
                DiService.Get<StateEnd_FX>()?.DoFX();
                _nextState = _nextStateOnWin;
                isDone = true;
            }
        }

        private void Update()
        {
            if (_isDragging == false)
            {
                Stop();
            }
        }

        private void Do()
        {
            if (_plateAnimation.AnimationName != "action")
            {
                _plateAnimation.AnimationState.ClearTracks();
                _plateAnimation.AnimationState.SetAnimation(0, "action", true);
            }

            _plateAnimation.timeScale = 1;

            if (_audioSource != null)
            {
                _audioSource.DOFade(1, 0.25f);
            }
        }

        private void Stop()
        {
            _plateAnimation.timeScale = 0;

            if (_audioSource != null)
            {
                _audioSource.DOFade(0, 0.25f);
            }
        }
    }
}
