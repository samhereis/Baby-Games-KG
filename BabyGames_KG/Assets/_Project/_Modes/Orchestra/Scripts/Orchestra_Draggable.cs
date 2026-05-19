using _Project._Modes.Orchestra.Scripts;
using _Project.Scripts.Sound;
using AllIn1SpriteShader;
using CustomAttributes;
using DG.Tweening;
using Extentions;
using Helpers;
using Identifiers;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Project._Modes.Coloring;
using _Project.Scripts.GameFeel;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Modes.Sorting
{
    //TODO: delete 18.05.2026
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(Move_FX))]
    [RequireComponent(typeof(Sound_FX))]
    public class Orchestra_Draggable : IdentifierBase, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Image _mainImage;

        [Space]
        [SerializeField] private Image _icon;

        [SerializeField] private List<Image> _allIconImages = new();
        [SerializeField] private RectTransform _iconRectTransform;

        [Space]
        [SerializeField] private bool _autoSet = false;
        [SerializeField] private float _sizingDuration = 0.25f;

        [Space]
        [FoldoutGroup("FX"), SerializeField] private Move_FX _move_FX;
        [FoldoutGroup("FX"), SerializeField] private Sound_FX _sound_FX;
        [FoldoutGroup("FX")] public ObjectJuicer objectJuicer;
        [FoldoutGroup("FX")] public float dropParticleScale = 3;

        [Space]
        [Fg_De, SerializeField] private bool _isDropped;
        [Fg_De, SerializeField] private bool _isInitialized;

        public RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private AllIn1Shader _allIn1Shader;
        private OrchestraWaveUnit _orchestraWaveData;

        private Vector3 _initialPosition = Vector3.zero;
        private Vector3 _initialScale = Vector3.zero;

        private OrchestraController _orchestraController;

        public async void Initialize(OrchestraWaveUnit orchestraWave, OrchestraController sortingController)
        {
            _orchestraController = sortingController;
            _orchestraWaveData = orchestraWave;
            _icon.sprite = _orchestraWaveData.dropZone.spriteRenderer[0].sprite;

            foreach (var item in _orchestraWaveData.dropZone.spriteRenderer)
            {
                if (item.sprite == _icon.sprite)
                {
                    continue;
                }

                var icon = Instantiate(_icon, _icon.transform.parent);
                icon.sprite = item.sprite;
                _allIconImages.Add(icon);
            }

            await AsyncHelper.NextFrame();
            _initialPosition = _rectTransform.anchoredPosition;
            _initialScale = _rectTransform.localScale;

            ClearSizes();

            _move_FX = Get<Move_FX>();
            _sound_FX = Get<Sound_FX>();
            if (objectJuicer == false) { objectJuicer = GetComponent<ObjectJuicer>(); }
            if (objectJuicer == false) { objectJuicer = gameObject.AddComponent<ObjectJuicer>(); }

            objectJuicer.StartJamming();

            _autoSet = _orchestraWaveData.autoSet;

            _isInitialized = true;
        }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _allIn1Shader = _icon.GetComponent<AllIn1Shader>();

            _icon.material = Resources.Load<Material>("Materials/Wave_Draggable");
            _allIn1Shader.ForceSetNewMaterial(new Material(_icon.material));

            _allIn1Shader.SetGrayScale(false);
        }

        private async void Update()
        {
            if (_isInitialized == false) { return; }

            foreach (var item in _allIconImages)
            {
                item.rectTransform.sizeDelta = _icon.rectTransform.sizeDelta;
            }

            if (_isDropped) { return; }
            if (_autoSet) { await TryDrop(); }
        }

        public async void OnBeginDrag(PointerEventData eventData)
        {
            _canvasGroup.blocksRaycasts = false;

            if (_sound_FX != null) { _sound_FX?.Play(Sound_Effect.StartDrag); }
            objectJuicer.StopJamming(true);

            if (_orchestraWaveData.forceRightAnswer != null) { await SyncSizes(_orchestraWaveData.forceRightAnswer.transform); }
            else { await SyncSizes(_orchestraWaveData.dropZone.transform); }

        }

        public void OnDrag(PointerEventData eventData)
        {
            _rectTransform.position = eventData.position;

            _orchestraWaveData.dropZone.IsDroppingCorrectly(_orchestraController.IsDraggingCorrectly(_orchestraWaveData));
        }

        public async void OnEndDrag(PointerEventData eventData)
        {
            if (_isDropped) { return; }

            _canvasGroup.blocksRaycasts = true;

            _mainImage.raycastTarget = false;
            if (_autoSet == false)
            {
                if (await TryDrop() == false) { await GoBack(); }
            }
            else { await GoBack(); }
            _mainImage.raycastTarget = true;
        }

        private async Task<bool> TryDrop()
        {
            if (_orchestraController.IsDraggingCorrectly(_orchestraWaveData) == true)
            {
                _isDropped = true;

                _canvasGroup.interactable = false;
                _mainImage.raycastTarget = false;

                var clip = await _sound_FX.PlayAsync(Sound_Effect.Success);
                Move_FX.MakeDoneParticle(_orchestraWaveData.dropZone.transform.position, dropParticleScale);

                _rectTransform.DOMove(Camera.main.WorldToScreenPoint(_orchestraWaveData.dropZone.transform.position), _sizingDuration);

                _iconRectTransform.DOSizeDelta(new Vector2(_icon.sprite.texture.width, _icon.sprite.texture.height), _sizingDuration).SetEase(Ease.OutBack);
                await _rectTransform.transform.DOScale(_orchestraWaveData.dropZone.transform.localScale, _sizingDuration).SetEase(Ease.OutBack).AsyncWaitForCompletion();

                _orchestraWaveData.isComplete = true;
                _orchestraWaveData.onComplete?.Invoke(_orchestraWaveData);

                transform.localScale = Vector3.zero;

                return true;
            }

            return false;
        }

        private async Task GoBack()
        {
            _isDropped = false;

            objectJuicer.StartJamming();
            foreach (var item in _orchestraController.model.currentOrchestraWaveData.waveUnits)
            {
                var distance = Vector2.Distance(Camera.main.ScreenToWorldPoint(_rectTransform.position), item.dropZone.transform.position);
                if (distance < 2)
                {
                    var clip = await _sound_FX.PlayAsync(Sound_Effect.Failure);
                    _move_FX.Fail();
                    await AsyncHelper.DelayFloat(clip.length);

                    break;
                }
            }

            ClearSizes();
            _rectTransform.DOAnchorPos(_initialPosition, 0.25f);
        }

        private async Task SyncSizes(Transform syncWith)
        {
            _iconRectTransform.DOKill();
            _rectTransform.DOKill();

            _iconRectTransform.DOSizeDelta(new Vector2(_icon.sprite.texture.width, _icon.sprite.texture.height) * 1.1f, _sizingDuration).SetEase(Ease.OutBack);
            await _rectTransform.transform.DOScale(syncWith.transform.localScale * 1.1f, _sizingDuration).SetEase(Ease.OutBack).AsyncWaitForCompletion();

            foreach (var item in _allIconImages)
            {
                item.rectTransform.sizeDelta = _icon.rectTransform.sizeDelta;
            }
        }

        private void ClearSizes()
        {
            _iconRectTransform.DOKill();
            _rectTransform.DOKill();

            _iconRectTransform.DOSizeDelta(new Vector2(_rectTransform.sizeDelta.x, _rectTransform.sizeDelta.y), _sizingDuration).SetEase(Ease.OutBack);
            _rectTransform.transform.DOScale(_initialScale, _sizingDuration).SetEase(Ease.OutBack);
        }
    }
}