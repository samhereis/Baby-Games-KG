using _Project.Scripts.Sound;
using CustomAttributes;
using DG.Tweening;
using Helpers;
using Identifiers;
using Loggers;
using Observables;
using Spine.Unity;
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
using static Interfaces.IDropable;

namespace Gameplay
{
    [RequireComponent(typeof(ObjectJuicer))]
    public class Dropable_General : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Inject] private Content _content;

        public Action<Dropable_General, Dropable_General> onCopyAdded;

        public Action<Dropable_General> onMouseDown;
        public Action<Dropable_General> onMouseUpAsButton;
        public Action<Dropable_General> onMouseUp;

        public Action<Dropable_General> onStartDrag;
        public Action<Dropable_General> onEndDrag;

        public Action<Dropable_General> onDropStart;
        public Action<Dropable_General> onDropEnd;
        public ObservableValue<bool> hasDropped = new("");

        [Fg_Se] public bool changeSorting = false;
        [Fg_Se] public float dropDuration = 0.25f;
        [Fg_Se] public float dropDistance = 2;
        [Fg_Se] public bool goBackOnMultipleMode = false;
        [Fg_Se] public bool deactivateOnStart;
        [Fg_Se] public bool autoPlace = false;
        [Fg_Se] public bool canClick = false;
        [Fg_Se] public bool calculateOffsetBasedOnBoxCollider_Drag = false;
        [Fg_Se] public bool calculateOffsetBasedOnBoxCollider_Drop = false;
        [Fg_Se] public bool autoSearchForWrongPosition = false;
        [field: SerializeField, Fg_Se] public Mode mode { get; set; }

        [Space]
        public PlacesHolder placesHolder;
        public List<Transform> wrongAnswers = new();

        [Space]
        public SpriteRenderer spriteRenderer;
        public SkeletonAnimation skeletonAnimation;
        public Transform parentToSet;
        public BoxCollider _boxCollider;

        [Space]
        [FoldoutGroup("FX")] public Sound_FX _soundFX;
        [FoldoutGroup("FX")] public Move_FX _moveFX;
        [FoldoutGroup("FX")] public TrailParticle trailParticle;
        [FoldoutGroup("FX")] public ObjectJuicer objectJuicer;

        [FoldoutGroup("FX")] public bool doPunchAnimation = true;
        [FoldoutGroup("FX")] public float dropParticleScale = 1;
        [FoldoutGroup("FX")] public float dragParticleScale = 1;

        [Space]
        public Camera cameraToUse;

        [Fg_De] public List<Dropable_General> copies = new();
        [Fg_De] public Vector3 initialPosition;
        [Fg_De] public Transform lasNearestPosition;
        [Fg_De] public Vector3 offset;
        [Fg_De] public bool canDrag = false;

        public static int? sortingToSet;

        private Camera cameraa => cameraToUse != null ? cameraToUse : Camera.main;

        public Vector3 nearestPosition
        {
            get
            {
                lasNearestPosition = placesHolder.secondary[0];

                float minSqrDistance = (lasNearestPosition.position - transform.position).sqrMagnitude;

                for (int i = 1; i < placesHolder.secondary.Count; i++)
                {
                    float sqrDist = (placesHolder.secondary[i].position - transform.position).sqrMagnitude;
                    if (sqrDist < minSqrDistance)
                    {
                        minSqrDistance = sqrDist;
                        lasNearestPosition = placesHolder.secondary[i];
                    }
                }
                return lasNearestPosition.position;
            }
        }

        private void Start()
        {
            initialPosition = transform.localPosition;
            if (TryGetComponent<PanelItem>(out var panelItem)) { initialPosition = panelItem.initialPosition; }

            if (_boxCollider == false) { _boxCollider = GetComponentInChildren<BoxCollider>(true); }
            if (_moveFX == false) { _moveFX = GetComponent<Move_FX>(); }
            if (_soundFX == false) { _soundFX = GetComponent<Sound_FX>(); }
            if (objectJuicer == false) { objectJuicer = GetComponent<ObjectJuicer>(); }
            if (objectJuicer == false) { objectJuicer = gameObject.AddComponent<ObjectJuicer>(); }
            if (trailParticle == false)
            {
                trailParticle = gameObject.AddComponent<TrailParticle>();
                trailParticle.dropParticleScale = dragParticleScale;
                trailParticle.enabled = false;
            }

            gameObject.SetActive(!deactivateOnStart);

            if (changeSorting) { sortingToSet = spriteRenderer.sortingOrder; }
        }

        private void OnDestroy()
        {
            sortingToSet = null;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_boxCollider.enabled == false) { return; }

            transform.DOKill();
            onMouseDown?.Invoke(this);

            if (calculateOffsetBasedOnBoxCollider_Drag)
            {
                offset = -_boxCollider.center / 2;
            }
            else
            {
                offset = Vector3.zero;
            }

            if (autoSearchForWrongPosition)
            {
                var list = FindObjectsByType<Dropable_General>(FindObjectsSortMode.None);

                foreach (var item in list)
                {
                    if (item == null || item.placesHolder == null) { continue; }
                    foreach (var item1 in item.placesHolder.secondary)
                    {
                        wrongAnswers.SafeAdd(item1);
                    }
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_boxCollider.enabled == false) { return; }
            if (trailParticle != null) { trailParticle.enabled = false; }
            if (canClick) { onMouseUpAsButton?.Invoke(this); }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_boxCollider.enabled == false) { return; }
            if (canDrag == false)
            {
                gameObject.SetActive(true);
                if (changeSorting)
                {
                    if (spriteRenderer != null) { spriteRenderer.sortingOrder = sortingToSet.Value; }
                }
            }

            if (_soundFX != null) { _soundFX.Play(Sound_Effect.StartDrag); }
            if (trailParticle != null) { trailParticle.enabled = true; }

            canDrag = true;
            Move(eventData.position);
            onStartDrag?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_boxCollider.enabled == false) { return; }
            if (canDrag == false) { return; }

            Move(eventData.position);

            var distance = Vector3.Distance(transform.position, nearestPosition);
            var canPlace = distance < dropDistance;
            if (autoPlace && canPlace)
            {
                canDrag = false;
                Place();
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (canDrag == false) { return; }
            canDrag = false;

            if (_boxCollider.enabled == false) { return; }
            if (trailParticle != null) { trailParticle.enabled = false; }

            Place();

            canDrag = false;
            onMouseUp?.Invoke(this);
        }

        public void Move(Vector3 uiPosition)
        {
            var ortho = cameraa.orthographic;

            cameraa.orthographic = true;
            var position = cameraa.ScreenToWorldPoint(uiPosition);
            cameraa.orthographic = ortho;

            position.z = 0;
            transform.position = position + offset;
        }

        public async void Place(Vector3? position = null)
        {
            var distance = Vector3.Distance(transform.position, nearestPosition);
            var canPlace = distance < dropDistance;

            if (calculateOffsetBasedOnBoxCollider_Drop == false) { offset = Vector3.zero; }
            if (trailParticle != null) { trailParticle.enabled = false; }

            if (position != null)
            {
                onDropStart?.Invoke(this);
                await transform.DOMove(position.Value, 0.25f).SetEase(Ease.OutBack).AsyncWaitForCompletion();
                hasDropped.ChangeValue(true);
                onDropEnd?.Invoke(this);
                return;
            }

            if (canPlace) { await Drop(); }
            else
            {
                _boxCollider.enabled = false;

                await TryPlayFailureSounds();

                if (_moveFX != null && _soundFX == null) { await AsyncHelper.DelayFloat(1f); }

                PlaceBack();
            }
        }

        public async Task Drop()
        {
            onDropStart?.Invoke(this);

            if (mode == Mode.Single)
            {
                transform.DOKill();
                if (doPunchAnimation) { Move_FX.MakeDonePunch(transform); }
                Move_FX.MakeDoneParticle(nearestPosition + offset, dropParticleScale);
                await transform.DOMove(nearestPosition + offset, dropDuration).SetEase(Ease.OutBack).AsyncWaitForCompletion();
            }
            else
            {
                var parent = transform.parent;
                if (parentToSet != null) { parent = parentToSet; }

                var copy = Instantiate(this, parent);
                copy.transform.position = transform.position;
                copy.deactivateOnStart = false;
                copy.gameObject.SetActive(true);

                copies?.Add(copy);
                onCopyAdded?.Invoke(this, copy);

                PlaceBack();

                copy.transform.DOKill();
                if (doPunchAnimation) { Move_FX.MakeDonePunch(transform); }
                Move_FX.MakeDoneParticle(nearestPosition + offset, dropParticleScale);
                await copy.transform.DOMove(nearestPosition + offset, dropDuration).SetEase(Ease.OutBack).AsyncWaitForCompletion();

                if (changeSorting)
                {
                    if (sortingToSet == null) { sortingToSet = spriteRenderer.sortingOrder; }

                    copy.spriteRenderer.sortingOrder = sortingToSet.Value;
                    if (copy.skeletonAnimation != null) { copy.skeletonAnimation.GetComponent<MeshRenderer>().sortingOrder = sortingToSet.Value; }
                    sortingToSet++;
                }
            }

            onDropEnd?.Invoke(this);
            hasDropped.ChangeValue(true);
        }

        public async void PlaceBack()
        {
            await PlaceBackAsync();
        }

        public async Task PlaceBackAsync()
        {
            _boxCollider.enabled = false;

            onEndDrag?.Invoke(this);

            if (mode == Mode.Single)
            {
                transform.DOKill();
                await transform.DOLocalMove(initialPosition, dropDuration).SetEase(Ease.OutBack).AsyncWaitForCompletion();
            }
            else
            {
                if (goBackOnMultipleMode)
                {
                    transform.DOKill();
                    await transform.DOLocalMove(initialPosition, dropDuration).SetEase(Ease.OutBack).AsyncWaitForCompletion();
                }
                else { gameObject.SetActive(false); }
            }

            _boxCollider.enabled = true;
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
            } catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }
        }
    }
}