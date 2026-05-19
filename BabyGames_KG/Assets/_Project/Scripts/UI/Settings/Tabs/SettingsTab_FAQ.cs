using DataClasses.AssetReferences;
using Sirenix.OdinInspector;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Coloring.ForParents
{
    public class SettingsTab_FAQ : SettingsTab_Base
    {
        [SerializeField] private Button _buttonMailToo;
        [SerializeField] private TMP_Text _text;

        [SerializeField] private RectTransform _content;

        [SerializeField] private ExternalAssetsReference_HasComponent<ItemFAQ> _faqs_IOS;
        [SerializeField] private ExternalAssetsReference_HasComponent<ItemFAQ> _faqs_Android;

        public string email = "feedback@lalafun.net";

        private string _textMessage = "";

        private void OnDestroy()
        {
            _buttonMailToo.onClick.RemoveListener(OpenFeedbackPage);
        }

        private void Awake()
        {
            _textMessage = $"Colorings build {Application.version} Feedback";
            _buttonMailToo.onClick.AddListener(OpenFeedbackPage);
        }

        private void OnEnable()
        {
            _text.text = _text.text.Replace("$", email);
            FillFaqs(Application.platform);
        }

        public void OpenFeedbackPage()
        {
            Application.OpenURL("mailto:feedback@lalafun.net?subject=" + _textMessage);
        }

        [Button]
        private async void FillFaqs(RuntimePlatform platform)
        {
            foreach (var item in _content.GetComponentsInChildren<ItemFAQ>(true))
            {
                Destroy(item.gameObject);
            }

            if (platform == RuntimePlatform.Android)
            {
                foreach (var item in await _faqs_Android.GetAssetsAsync())
                {
                    Instantiate(item, parent: _content);
                }
            }
            else if (platform == RuntimePlatform.IPhonePlayer)
            {
                foreach (var item in await _faqs_IOS.GetAssetsAsync())
                {
                    Instantiate(item, parent: _content);
                }
            }
            else
            {
                foreach (var item in await _faqs_Android.GetAssetsAsync())
                {
                    Instantiate(item, parent: _content);
                }
            }
        }
    }
}