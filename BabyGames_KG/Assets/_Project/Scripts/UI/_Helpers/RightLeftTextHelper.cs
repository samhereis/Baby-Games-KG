using Helpers;
using Services;
using Sirenix.OdinInspector;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace UI
{
    [DisallowMultipleComponent]
    public class RightLeftTextHelper : MonoBehaviour, ISelfValidator
    {
        [SerializeField] private TextMeshProUGUI _text;
        private static LazyUpdator_Service _lazyUpdator_Service = new LazyUpdator_Service();

        public void Validate(SelfValidationResult result)
        {
            if (_text == null) { _text = GetComponent<TextMeshProUGUI>(); }
        }

        private async void OnEnable()
        {
            if (_text == null) { _text = GetComponent<TextMeshProUGUI>(); }
            if (_text.horizontalAlignment == HorizontalAlignmentOptions.Center) { return; }

            _lazyUpdator_Service.AddToQueue(UpdateAlignment);
            await UpdateAlignment();
        }

        private void OnDisable()
        {
            _lazyUpdator_Service.RemoveFromQueue(UpdateAlignment);
        }

        private async Task UpdateAlignment()
        {
            if (LocalizationSettings.SelectedLocale.name.Contains("Arabic") || LocalizationSettings.SelectedLocale.name.Contains("Hebrew"))
            {
                _text.horizontalAlignment = HorizontalAlignmentOptions.Right;
            }
            else
            {
                _text.horizontalAlignment = HorizontalAlignmentOptions.Left;
            }

            await AsyncHelper.NextFrame();
        }
    }
}