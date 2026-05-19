using _Project.Scripts.Sound;
using DataClasses;
using DG.Tweening;
using Helpers;
using Helpers.ImageCroppers;
using Identifiers;
using Loggers;
using Services;
using Sounds;
using Spine.Unity;
using System;
using System.Linq;
using System.Threading.Tasks;
using _Project._Modes.Coloring;
using _Project.Scripts.Data;
using _Project.Scripts.GameFeel;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace WhoLivesWhere
{
    public class WLW_Character : IdentifierBase, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public enum WLW_CharacterState { OutsideSeat, InsideSeat }

        public Action<WLW_Character> onDrop;
        public Action<WLW_Character> onSet;

        public SpriteRenderer newCopy;

        private Vector3 _offset;

        [Space]
        [SerializeField] private SkeletonAnimation _skeletonAnimation;
        [SerializeField] private Transform _targetPosition;
        [SerializeField] private Transform _targetParent;
        [SerializeField] private Sound _audioClip;
        [SerializeField] private Material _hintMaterial;

        [Space]
        [SerializeField] private bool _autoPlace = false;
        [SerializeField] private float _distanceToAutoPlace;
        [SerializeField] private int _dropSortingLater = 1;
        [SerializeField] private int _draggingSortingLater = 1;
        [SerializeField] private int _normalSortingLayer = 1;
        [SerializeField] private float _copyScale = 0.5f;

        [SerializeField] private float _seatScale = 0.5f;
        [SerializeField] private float _seatCopyScale = 0.5f;
        [SerializeField] private float _homeScale = 0.5f;
        [SerializeField] private float _homeCopyScale = 0.5f;

        [SerializeField] private WLW_CharacterState _characterState = WLW_CharacterState.OutsideSeat;

        [Space]
        [SerializeField, FoldoutGroup("FX")] private Sound_FX _sound_FX;
        [SerializeField, FoldoutGroup("FX")] private Move_FX _move_FX;
        [FoldoutGroup("FX")] public TrailParticle trailParticle;
        [FoldoutGroup("FX")] public ObjectJuicer objectJuicer;

        [FoldoutGroup("FX")] public bool doPunchAnimation = true;
        [FoldoutGroup("FX")] public float dropParticleScale = 1;
        [FoldoutGroup("FX")] public float dragParticleScale = 1;

        [Space]
        [SerializeField] private LayerMask _captureLayer;

        private Vector3 _initialPosition;
        private MaterialPropertyBlock _mpb;

        [Inject] private ISoundPlayer _soundPlayer;

        private void Awake()
        {
            DiService.Inject(this);

#if UNITY_ANDROID
            _seatCopyScale += (1 - _seatCopyScale) / 2;
            _homeCopyScale += (1 - _homeCopyScale) / 2;
#endif

            if (objectJuicer == false) { objectJuicer = GetComponent<ObjectJuicer>(); }
            if (objectJuicer == false) { objectJuicer = gameObject.AddComponent<ObjectJuicer>(); }
            if (trailParticle == null)
            {
                trailParticle = gameObject.AddComponent<TrailParticle>();
                trailParticle.dropParticleScale = dragParticleScale;
                trailParticle.enabled = false;
            }
            objectJuicer.StartJamming();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            var sp = new Vector3(eventData.position.x, eventData.position.y, 0f);
            var world = Camera.main != null ? Camera.main.ScreenToWorldPoint(sp) : transform.position;
            world.z = 0f;
            _offset = transform.position - world;

            if (_sound_FX != null) { _sound_FX?.Play(Sound_Effect.StartDrag); }
            trailParticle.enabled = true;
            objectJuicer.StopJamming(true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (Get<Collider>().enabled == false) { return; }

            Vector3 sp = new Vector3(eventData.position.x, eventData.position.y, 0f);
            Vector3 mousePosition = Camera.main != null ? Camera.main.ScreenToWorldPoint(sp) : transform.position;
            mousePosition.z = 0f;
            transform.position = mousePosition + _offset;
            SetSortingLayer(_draggingSortingLater);

            TryPlace();
        }

        public async void OnPointerUp(PointerEventData eventData)
        {
            trailParticle.enabled = false;

            if (Get<Collider>().enabled)
            {
                var audio = await _sound_FX.PlayAsync(Sound_Effect.Failure);
                _move_FX.Fail(transform, audio.length / 2);
                await AsyncHelper.DelayFloat(audio.length);

                Get<Collider>().enabled = true;
                objectJuicer.StartJamming();

                PutBack();
            }
        }

        private async void TryPlace()
        {
            var canPlace = _targetPosition != null && Vector2.Distance(transform.position, _targetPosition.position) <= _distanceToAutoPlace;

            if (canPlace)
            {
                onDrop?.Invoke(this);

                trailParticle.enabled = false;
                Move_FX.MakeDoneParticle(_targetParent.position);
                var audio = await _sound_FX.PlayAsync(Sound_Effect.Success);

                SetDraggableStatus(false);
                SetInPlace();
            }
        }

        public async void SetInPlace()
        {
            SetSortingLayer(_normalSortingLayer);
            transform.SetParent(_targetParent, true);

            Get<MeshRenderer>().sortingLayerName = newCopy == null ? "Default" : "Front";

            if (_characterState == WLW_CharacterState.InsideSeat)
            {
                var hintSpriteRenderer = _targetParent.GetComponentInChildren<SpriteRenderer>();
                if (hintSpriteRenderer != null)
                {
                    hintSpriteRenderer.sortingOrder -= 1;
                    hintSpriteRenderer?.transform.DOScale(0, 0).SetDelay(0.55f);
                }
                transform.DOScale(_homeScale, 0.5f).SetEase(Ease.OutBack).AsyncWaitForCompletion();
            }
            else
            {
                newCopy?.DOFade(0, 0.5f).SetDelay(1f);
                transform.DOScale(_seatScale, 0.5f).SetEase(Ease.OutBack).AsyncWaitForCompletion();
            }

            await transform.DOLocalMove(Vector3.zero, 0.5f).AsyncWaitForCompletion();

            if (newCopy == null)
            {
                _soundPlayer.TryPlay(_audioClip);
                Get<SkeletonAnimation>().loop = false;
                Get<SkeletonAnimation>().AnimationName = "after_train";
            }
            else
            {
                Destroy(newCopy.gameObject);
                _characterState = WLW_CharacterState.InsideSeat;
            }

            onSet?.Invoke(this);

            if (newCopy != null) { newCopy.gameObject.SetActive(false); }
        }

        public void PutBack()
        {
            SetSortingLayer(_dropSortingLater);

            if (_characterState == WLW_CharacterState.OutsideSeat)
            {
                transform.DOLocalMove(_initialPosition, 0.5f).SetEase(Ease.OutBack);
            }
            else
            {
                transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.OutBack);
            }
        }

        public async Task SetTargetPosition(Transform targetPosition, Transform parentToSet)
        {
            _initialPosition = transform.localPosition;
            _targetParent = parentToSet;
            _targetPosition = targetPosition;

            try
            {
                _targetParent.gameObject.SetActive(true);
                if (newCopy != null) { newCopy.sprite = await GetHintSprite(); }
                else
                {
                    var hintSpriteRenderer = _targetParent.GetComponentInChildren<SpriteRenderer>();
                    if (hintSpriteRenderer != null)
                    {
                        hintSpriteRenderer.sprite = await GetHintSprite();
                        hintSpriteRenderer.transform.DOScale(_homeCopyScale, 0.5f);
                    }
                }
            } catch (Exception ex) { CustomLogger.instance?.LogException(ex); }
        }

        public async Task SetCopy(Transform newCopy)
        {
            newCopy.localScale = Vector3.zero;
            newCopy.localPosition = Vector3.zero;

            foreach (var item in newCopy.GetComponentsInChildren<SkeletonAnimation>(true))
            {
                Destroy(item);
            }

            foreach (var item in newCopy.GetComponentsInChildren<Component>(true))
            {
                if (item is not Transform) { Destroy(item); }
            }

            var copyRenderer = newCopy.gameObject.AddComponent<SpriteRenderer>();
            this.newCopy = copyRenderer;
            copyRenderer.material = _hintMaterial;

            await SetTargetPosition(newCopy.transform, newCopy.transform.parent);
            newCopy.transform.DOLocalMove(new Vector3(0, 0, 0), 1f);
            newCopy.transform.DOScale(_seatCopyScale, 1f);
        }

        public void SetSortingLayer(int layer)
        {
            if (newCopy != null) return;
            _skeletonAnimation.GetComponent<MeshRenderer>().sortingOrder = layer;
        }

        public void SetDraggableStatus(bool isDraggable)
        {
            TryGetAll_List<Collider>().ForEach(x =>
            {
                if (x != null) { x.enabled = isDraggable; }
            });
        }

        private Sprite _hintSprite;
        public async Task<Sprite> GetHintSprite()
        {
            if (_hintSprite != null) { return _hintSprite; }

            var result = await SpineHelper.CaptureMeshRenderersSnapshot(TryGetAll_List<MeshRenderer>(), _captureLayer, Screen.width, Screen.height);
            ClearablesHolder.instance.clearableTextures.SafeAdd(result);

            result = await ImageCropper_Implementation2.CropAlphas_Rectangular(result, 5, 5);
            ClearablesHolder.instance.clearableTextures.SafeAdd(result);

            await result.ClampAlphas(0.5f);
            await result.SetColor(Color.black);

            _hintSprite = Sprite.Create(result, new Rect(0, 0, result.width, result.height), new Vector2(0.5f, 0.5f));

            return null;
        }
    }
}