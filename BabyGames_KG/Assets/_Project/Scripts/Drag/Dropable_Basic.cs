using _Project.Scripts.Sound;
using CustomAttributes;
using DG.Tweening;
using Gameplay;
using Helpers;
using Loggers;
using Observables;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Project._Modes.Coloring;
using _Project.Scripts.GameFeel;
using Modes.Coloring;
using Services;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Coocking
{
    [RequireComponent(typeof(ObjectJuicer))]
    public class Dropable_Basic : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler
    {
        public enum Dropable_Basic_Mode { Standart, Copy }
        public enum OnCompleteMode { Hide, Destroy }

        public ObservableValue<bool> isDropped;

        public Action<Dropable_Basic> onPointerDown;
        public Action<Dropable_Basic> onPointeUpStart;
        public Action<Dropable_Basic> onPointeUpEnd;
        public Action<Dropable_Basic> onStartDrag;
        public Action<Dropable_Basic> onStartDrop;
        public Action<Dropable_Basic> onFinish;
        public Action<Dropable_Basic> onRestore;

        public Action<Dropable_Basic, Dropable_Basic> onCopyCreated;
        public Action<Dropable_Basic, Dropable_Basic> onCopyStartDrop;
        public Action<Dropable_Basic, Dropable_Basic> onCopyFinish;

        [Space]
        public Dropable_Basic_Mode mode = Dropable_Basic_Mode.Standart;
        public OnCompleteMode onCompleteMode = OnCompleteMode.Hide;

        [Space]
        public PlacesHolder targetPosition;
        public List<Transform> wrongAnswers = new();

        [Space]
        [Fg_Se] public Vector3 initialPosition;
        [Fg_Se] public float dropDistance = 2f;
        [Fg_Se] public float moveDuration = 0.25f;
        [Fg_Se] public bool autoPlace = false;
        [Fg_Se] public bool autoMoveToTarget = true;
        [Fg_Se] public bool autoSearchForWrongPosition = false;

        [FoldoutGroup("FX")] public bool doPunchAnimation = true;
        [FoldoutGroup("FX")] public float dropParticleScale = 1;
        [FoldoutGroup("FX")] public float dragParticleScale = 1;

        [Space]
        [FoldoutGroup("FX")] public Sound_FX _soundFX;
        [FoldoutGroup("FX")] public Move_FX _moveFX;
        [FoldoutGroup("FX")] public TrailParticle trailParticle;
        [FoldoutGroup("FX")] public ObjectJuicer objectJuicer;

        [Inject] private Content _content;

        [Fg_De, SerializeField] private bool _isLinkedToCompleteButton;
        [Fg_De, SerializeField] public Dropable_Basic copy;
        [Fg_De, SerializeField] public bool canDrag = true;

        private bool _hasInteracted = false;
        private BoxCollider _boxCollider;

        public BoxCollider boxCollider
        {
            get
            {
                if (_boxCollider == null) { _boxCollider = GetComponent<BoxCollider>(); }
                return _boxCollider;
            }
        }

        public bool CanPlace()
        {
            if (targetPosition == null) { return false; }
            return Vector3.Distance(transform.position, targetPosition.GetNearestPosition(transform.position)) < dropDistance;
        }

        private void Awake()
        {
            if (_moveFX == null) _moveFX = GetComponent<Move_FX>();
            if (_soundFX == null) _soundFX = GetComponent<Sound_FX>();
            if (objectJuicer == false) { objectJuicer = GetComponent<ObjectJuicer>(); }
            if (objectJuicer == false) { objectJuicer = gameObject.AddComponent<ObjectJuicer>(); }
            if (trailParticle == null)
            {
                trailParticle = gameObject.AddComponent<TrailParticle>();
                trailParticle.dropParticleScale = dragParticleScale;
                trailParticle.enabled = false;
            }
        }

        private void OnDisable()
        {
            Deactivate();
        }

        public void Activate()
        {
            CompleteButton.instance?.actionsOnComplete.Add(OnCompleteButtonAsync);
            _isLinkedToCompleteButton = true;
        }

        public void Deactivate()
        {
            CompleteButton.instance?.actionsOnComplete.Remove(OnCompleteButtonAsync);
            _isLinkedToCompleteButton = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (boxCollider.enabled == false) { return; }

            if (_hasInteracted == false)
            {
                initialPosition = transform.localPosition;
                _hasInteracted = true;
            }

            if (mode == Dropable_Basic_Mode.Copy)
            {
                copy = Instantiate(this, transform.parent);
                copy.boxCollider.enabled = false;
                onCopyCreated?.Invoke(this, copy);
            }

            if (autoSearchForWrongPosition)
            {
                var list = FindObjectsByType<Dropable_Basic>(FindObjectsSortMode.None);

                foreach (var item in list)
                {
                    if (item == null || item.targetPosition == null) { continue; }
                    foreach (var item1 in item.targetPosition.secondary) { wrongAnswers.SafeAdd(item1); }
                }
            }

            onPointerDown?.Invoke(this);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (boxCollider.enabled == false) { return; }
            if (canDrag == false) { return; }

            if (_soundFX != null) { _soundFX?.Play(Sound_Effect.StartDrag); }
            if (trailParticle != null) { trailParticle.enabled = true; }

            onStartDrag?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (boxCollider.enabled == false) { return; }
            if (canDrag == false) { return; }

            var sp = new Vector3(eventData.position.x, eventData.position.y, 0f);
            var world = Camera.main != null ? Camera.main.ScreenToWorldPoint(sp) : transform.position;
            world.z = 0f;
            transform.position = world;

            if (autoPlace && CanPlace()) { Place(false); }
        }

        public async void OnPointerUp(PointerEventData eventData)
        {
            canDrag = false;
            onPointeUpStart?.Invoke(this);

            if (boxCollider.enabled == true)
            {
                if (autoPlace == false && CanPlace()) { Place(); }
                else
                {
                    if (mode == Dropable_Basic_Mode.Copy)
                    {
                        transform.localPosition = initialPosition;
                        if (copy != null) { Destroy(copy.gameObject); }
                    }
                    else
                    {
                        boxCollider.enabled = false;
                        await TryPlayFailureSounds();
                        boxCollider.enabled = true;
                        await transform.DOLocalMove(initialPosition, moveDuration).AsyncWaitForCompletion();
                    }

                    onRestore?.Invoke(this);
                }
            }

            if (trailParticle != null) { trailParticle.enabled = false; }

            canDrag = true;
            onPointeUpEnd?.Invoke(this);
        }

        public async void Place(bool waitForAudioToPlace = true)
        {
            canDrag = false;
            boxCollider.enabled = false;

            isDropped.ChangeValue(true);
            onStartDrop?.Invoke(this);
            onCopyStartDrop?.Invoke(this, copy);

            if (doPunchAnimation) { Move_FX.MakeDonePunch(transform); }
            Move_FX.MakeDoneParticle(targetPosition.lasNearestPosition.transform.position, dropParticleScale);

            if (autoMoveToTarget)
            {
                if (_soundFX != null)
                {
                    var clip = await _soundFX?.PlayAsync(Sound_Effect.Success);
                    if (waitForAudioToPlace) { await AsyncHelper.DelayFloat(clip.length); }
                    _soundFX?.Play(Sound_Effect.MoveToTarget_Fast);
                }

                if (mode == Dropable_Basic_Mode.Copy)
                {
                    var copyPosition = copy.transform.position;

                    copy.transform.position = transform.transform.position;
                    transform.position = copyPosition;
                    copy.transform.DOMove(targetPosition.lasNearestPosition.transform.position, moveDuration);

                }
                else { transform.DOMove(targetPosition.lasNearestPosition.transform.position, moveDuration); }
            }

            if (trailParticle != null) { trailParticle.enabled = false; }

            onCopyFinish?.Invoke(this, copy);
            onFinish?.Invoke(this);
        }

        public async void DoReset()
        {
            isDropped.ChangeValue(false);

            transform.DOKill();

            if (mode == Dropable_Basic_Mode.Copy)
            {
                var scale = transform.localScale;

                transform.localPosition = initialPosition;
                transform.localScale = scale;
                if (copy != null) Destroy(copy.gameObject);
            }
            else { await transform.DOLocalMove(initialPosition, moveDuration).AsyncWaitForCompletion(); }

            if (trailParticle != null) { trailParticle.enabled = false; }
            boxCollider.enabled = true;
        }

        public async void OnCompleteButton()
        {
            if (gameObject == null) { return; }
            await OnCompleteButtonAsync();
        }

        public async Task OnCompleteButtonAsync()
        {
            if (boxCollider != null) { boxCollider.enabled = false; }

            switch (onCompleteMode)
            {
                case OnCompleteMode.Hide:
                {
                    await transform.DOScale(0, 0.25f).AsyncWaitForCompletion();
                    gameObject.SetActive(false);
                    break;
                }
                case OnCompleteMode.Destroy:
                {
                    await transform.DOScale(0, 0.25f).AsyncWaitForCompletion();
                    Destroy(gameObject);
                    break;
                }
                default:
                {
                    await transform.DOScale(0, 0.25f).AsyncWaitForCompletion();
                    gameObject.SetActive(false);
                    break;
                }
            }
        }

        private async Task TryPlayFailureSounds()
        {
            try
            {
                foreach (var item in wrongAnswers)
                {
                    if (item == this) { continue; }

                    if (Vector3.Distance(transform.position, item.position) < dropDistance)
                    {
                        var clip = await _soundFX?.PlayAsync(Sound_Effect.Failure);
                        _moveFX.Fail(transform, clip.length);
                        await AsyncHelper.DelayFloat(clip.length);

                        break;
                    }
                }
            } catch (Exception ex) { CustomLogger.instance?.LogException(ex); }
        }
    }
}