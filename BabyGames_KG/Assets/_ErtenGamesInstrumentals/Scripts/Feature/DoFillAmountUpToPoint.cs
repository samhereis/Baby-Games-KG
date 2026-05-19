#if DoTweenInstalled
using DG.Tweening;
#endif

using Helpers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Helpers
{
    public class DoFillAmountUpToPoint : MonoBehaviour
    {
        private enum Origin { Left, Right }

        [FoldoutGroup("Settings"), SerializeField] private Image _filledImage;
        [FoldoutGroup("Settings"), SerializeField] private Image _fillToThisPoint;
        [FoldoutGroup("Settings"), SerializeField] private RectTransform _borderHolder;
        [FoldoutGroup("Settings"), SerializeField] private Origin _origin;

        [FoldoutGroup("Debug"), SerializeField] private float _percent;
        [FoldoutGroup("Debug"), SerializeField] private Vector3 _wordPos;
        [FoldoutGroup("Debug"), SerializeField] private float _value;
        [FoldoutGroup("Debug"), SerializeField] private int _screenWidth;
        [FoldoutGroup("Debug"), SerializeField] private Camera _camera_;

        private Camera _camera => _camera_ != null ? _camera_ : _camera_ = FindFirstObjectByType<Camera>(FindObjectsInactive.Include);

        public void Awake()
        {
            _filledImage.fillAmount = 1;

            Close(0);

            _screenWidth = Screen.width;
        }

        private async void OnEnable()
        {
            await AsyncHelper.NextFrame();

#if DoTweenInstalled
            _fillToThisPoint.rectTransform.DOAnchorPosX(0, 1);
#endif
        }

        private void Update()
        {
            Do();
        }

        private void Do()
        {
            _wordPos = _camera.WorldToScreenPoint(_fillToThisPoint.rectTransform.position);
            _value = _wordPos.x;

            if (_origin == Origin.Right) _value -= _screenWidth;

            _percent = NumberHelper.GetPercentageOf1(_value, _screenWidth);
            _filledImage.fillAmount = Mathf.Abs(_percent);
        }

        public void Close(float duration)
        {
#if DoTweenInstalled
            if (_origin == Origin.Right) _fillToThisPoint.rectTransform.DOAnchorPosX(-_borderHolder.sizeDelta.x / 2, duration);
            else _fillToThisPoint.rectTransform.DOAnchorPosX(_borderHolder.sizeDelta.x / 2, duration);
#endif
        }

        [Button]
        public void Test()
        {
            Do();
        }
    }
}