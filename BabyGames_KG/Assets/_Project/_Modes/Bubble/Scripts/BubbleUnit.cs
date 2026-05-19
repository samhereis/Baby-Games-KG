using _Project.Scripts.Sound;
using CustomAttributes;
using DG.Tweening;
using FX;
using Helpers;
using Identifiers;
using Sirenix.OdinInspector;
using Sounds;
using Spine.Unity;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bubble
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public class BubbleUnit : IdentifierBase, IPointerClickHandler
    {
        public Action<BubbleUnit> onBlew;
        public Action<BubbleUnit> onAutoDestroy;

        public Sprite icon;
        public bool isEmpty;
        public bool forceStop;

        [SerializeField] private SkeletonGraphic _skeletonGraphic;
        [SerializeField] private Image _icon;
        [SerializeField] private Image _mainImage;
        [SerializeField] private Vector2 _scaleRandom = Vector2.one * 0.85f;
        [SerializeField] private Vector2 _speed = Vector2.one * 0.85f;

        [SerializeField] private RectTransform _rectTransform;

        [SerializeField] private Sound _popSound;
        [SerializeField] private SoundQueue_Advanced _putSound;

        private BubbleContainerData _bubbleContainerData;

        [Fg_De, SerializeField] private Transform _targetPosition;
        [Fg_De, SerializeField] private HintHand_Click _hintHandClick;

        [Button]
        private void Validate()
        {
            if (_skeletonGraphic == null) { _skeletonGraphic = GetComponent<SkeletonGraphic>(); }
            if (_rectTransform == null) { _rectTransform = GetComponent<RectTransform>(); }
            if (_mainImage == null) { _mainImage = GetComponent<Image>(); }
            if (_icon == null) { _icon = GetComponent<Image>(); }

            _icon.sprite = icon;
            _icon.color = icon != null ? Color.white : new Color(1, 1, 1, 0);
        }

        public void Construct(BubbleContainerData bubbleContainerData, HintHand_Click hintHandClick)
        {
            _bubbleContainerData = bubbleContainerData;
            _hintHandClick = hintHandClick;

            _icon.sprite = icon;
            _icon.color = icon != null ? Color.white : new Color(1, 1, 1, 0);
        }

        private void Awake()
        {
            transform.localScale = Vector3.one * _scaleRandom.GetRandom();

            _skeletonGraphic.raycastTarget = false;
            _icon.raycastTarget = false;

            Get<Canvas>().sortingOrder = -1;
        }

        private void OnDestroy()
        {
            _rectTransform.DOKill();
            _hintHandClick?.targets?.Remove(this.transform);
        }

        private void Update()
        {
            if (forceStop == true) { return; }

            if (_mainImage.raycastTarget == false) { return; }

            _rectTransform.anchoredPosition3D += Vector3.up * (_speed.GetRandom() * Time.deltaTime);

            if (_rectTransform.anchoredPosition3D.y > Screen.height * 1.5f)
            {
                onAutoDestroy?.Invoke(this);
                Destroy(gameObject, 1);
            }

            if (_rectTransform.anchoredPosition3D.y > (Screen.height / 3) && _rectTransform.anchoredPosition3D.y < (Screen.height / 1.5f))
            {
                if (isEmpty == false) { _hintHandClick?.targets?.SafeAdd(this.transform); }
            }
            else
            {
                _hintHandClick?.targets?.Remove(this.transform);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _mainImage.raycastTarget = false;
            _skeletonGraphic.raycastTarget = false;
            _icon.raycastTarget = false;

            Sound_FX.audioPlayer.TryPlay(_popSound);
            DoAnimation();

            _hintHandClick?.targets?.Remove(this.transform);
            if (isEmpty) { return; }

            onBlew?.Invoke(this);
        }

        public void MakeCopy()
        {
            _mainImage.raycastTarget = false;
            _skeletonGraphic.raycastTarget = false;
            _icon.raycastTarget = false;

            var canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0.5f;
        }

        public void DoAnimation()
        {
            _skeletonGraphic.AnimationState.ClearTracks();
            _skeletonGraphic.AnimationState.SetAnimation(0, "action", false);
        }

        public async Task Complete()
        {
            Get<Canvas>().sortingOrder = 2;

            _targetPosition = _bubbleContainerData.positions.GetRandom();

            _icon.DOKill();
            _icon.transform.DOScale(1f, 2f);
            await _icon.GetComponent<RectTransform>().DOMove(new Vector2(Screen.width / 2, Screen.height / 2), 1f).AsyncWaitForCompletion();
            await _icon.GetComponent<RectTransform>().DOMove(_bubbleContainerData.container.GetComponent<RectTransform>().position + Vector3.up * 200, 0.25f).AsyncWaitForCompletion();

            Sound_FX.audioPlayer.TryPlay(_putSound);
            await _icon.GetComponent<RectTransform>().DOMove(_targetPosition.position, 0.25f).AsyncWaitForCompletion();
        }
    }
}