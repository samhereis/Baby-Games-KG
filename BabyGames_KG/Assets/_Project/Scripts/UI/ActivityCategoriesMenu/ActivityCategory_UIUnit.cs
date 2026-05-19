using CustomAttributes;
using DataClasses;
using DG.Tweening;
using Helpers;
using Services;
using SO;
using Sounds;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Identifiers.UI
{
    public class ActivityCategory_UIUnit : IdentifierBase
    {
        public Action<ActivityCategory_UIUnit> onChoose;

        [Space]
        [SerializeField][Re_Fg_Co] private Button _button;
        [SerializeField][Re_Fg_Co] private RectTransform _holder;

        [Space]
        [SerializeField][Fg_Se] private float _dissolveFactor = 3f;
        [SerializeField][Fg_Se] private float _appearFactor = 1.5f;
        [SerializeField][Fg_Se] private float _smoothTime = 0.1f;
        [SerializeField][Fg_Se] private Sound _clickSound;

        [field: SerializeField][Fg_De] public ActivityCategory_SO activityCategory { get; private set; }
        [field: SerializeField][Fg_De] public Sound categorySound { get; private set; }
        [SerializeField][Fg_De] private Vector2 _currentPosition;

        [Inject] private ISoundPlayer _soundPlayer;

        public void Initialize(ActivityCategory_SO activityCategory)
        {
            this.activityCategory = activityCategory;

            DiService.Inject(this);
        }

        private void OnEnable()
        {
            _holder.anchoredPosition = Vector3.zero;
            _button.onClick.AddListener(OnChoose);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnChoose);
            _holder.anchoredPosition = Vector3.zero;
        }

        private void Update()
        {
            _currentPosition = Camera.main.ScreenToWorldPoint(transform.position);
            _currentPosition.x = Mathf.Abs(_currentPosition.x);

            float scaleFactor = _dissolveFactor / (_currentPosition.x / _appearFactor);

            Vector3 newScale = Vector3.one;
            newScale.x = Mathf.Clamp(scaleFactor, 1, 1.5f);
            newScale.y = Mathf.Clamp(scaleFactor, 1, 1.5f);

            transform.localScale = Vector3.Lerp(transform.localScale, newScale, 0.1f);
        }

        private async void OnChoose()
        {
            onChoose?.Invoke(this);
        }

        public async Task PlayChoseAnimation()
        {
            _soundPlayer?.TryPlay(_clickSound);
            _holder.DOAnchorPosY(Screen.height / 1.75f, 0.5f).SetEase(Ease.InBack);
            await AsyncHelper.DelayFloat(0.4f);
        }
    }
}