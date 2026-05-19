using Helpers;
using Services;
using Sirenix.OdinInspector;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace UI.Helpers
{
    [ExecuteAlways]
    [ExecuteInEditMode]
    public class ScaleByPercentage : MonoBehaviour, ISelfValidator
    {
        private static LazyUpdator_Service _lazyUpdator = new LazyUpdator_Service();

        [SerializeField] private RectTransform _parent;

        [Required]
        [SerializeField] private RectTransform _rectTransform;

        [SerializeField] private ScaleSettings _scaleSettings = new ScaleSettings();
        [SerializeField] private IgnoreSettings _ignoreSettings = new IgnoreSettings();

        public void Validate(SelfValidationResult result)
        {
            Validate();
        }

        private void Validate()
        {
            if (_rectTransform == null) { _rectTransform = GetComponent<RectTransform>(); }
            if (transform.parent != null && _parent == null) { _parent = transform.parent.GetComponent<RectTransform>(); }
            DoAutoScale();
        }

        private void Start()
        {
            DoAutoScale();
        }

        private void Update()
        {
            DoAutoScale();
        }

        [Button]
        [ContextMenu(nameof(DoAutoScale))]
        private void DoAutoScale()
        {
            if (_rectTransform != null)
            {
                if (_parent != null)
                {
                    if (_ignoreSettings.ignoreTop == false) _rectTransform.SetTop(NumberHelper.GetNumberFromPercentage(_parent.rect.height, _scaleSettings.top));
                    if (_ignoreSettings.ignoreBottom == false) _rectTransform.SetBottom(NumberHelper.GetNumberFromPercentage(_parent.rect.height, _scaleSettings.bottom));

                    if (_ignoreSettings.ignoreRight == false) _rectTransform.SetRight(NumberHelper.GetNumberFromPercentage(_parent.rect.width, _scaleSettings.right));
                    if (_ignoreSettings.ignoreLeft == false) _rectTransform.SetLeft(NumberHelper.GetNumberFromPercentage(_parent.rect.width, _scaleSettings.left));
                }
                else
                {
                    if (_ignoreSettings.ignoreTop == false) _rectTransform.SetTop(NumberHelper.GetNumberFromPercentage(Screen.height, _scaleSettings.top));
                    if (_ignoreSettings.ignoreBottom == false) _rectTransform.SetBottom(NumberHelper.GetNumberFromPercentage(Screen.height, _scaleSettings.bottom));

                    if (_ignoreSettings.ignoreRight == false) _rectTransform.SetRight(NumberHelper.GetNumberFromPercentage(Screen.width, _scaleSettings.right));
                    if (_ignoreSettings.ignoreLeft == false) _rectTransform.SetLeft(NumberHelper.GetNumberFromPercentage(Screen.width, _scaleSettings.left));
                }
            }
        }

        private async Task DoAutoScaleAsync()
        {
            DoAutoScale();
            await AsyncHelper.NextFrame();
        }

        [Serializable]
        public class ScaleSettings
        {
            [field: SerializeField] public float top { get; private set; } = 0;
            [field: SerializeField] public float bottom { get; private set; } = 0;
            [field: SerializeField] public float left { get; private set; } = 0;
            [field: SerializeField] public float right { get; private set; } = 0;
        }

        [Serializable]
        public class IgnoreSettings
        {
            [field: SerializeField] public bool ignoreTop { get; private set; } = false;
            [field: SerializeField] public bool ignoreBottom { get; private set; } = false;
            [field: SerializeField] public bool ignoreLeft { get; private set; } = false;
            [field: SerializeField] public bool ignoreRight { get; private set; } = false;
        }
    }
}