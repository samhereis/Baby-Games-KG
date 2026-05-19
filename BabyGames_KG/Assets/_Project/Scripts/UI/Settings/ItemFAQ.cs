using CustomAttributes;
using DataClasses.Consts;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace Coloring.ForParents
{
    public class ItemFAQ : MonoBehaviour
    {
        [SerializeField] private LocalizeStringEvent _questionText;
        [SerializeField] private List<RectTransform> _answerText = new();
        [SerializeField] private Image _imageArrow;

        [Space]
        [SerializeField] private RectTransform _content;
        [SerializeField] private Image _background;
        [SerializeField] private string _prefix = "";
        [SerializeField] private float _threshold = -4;

        [Space]
        [SerializeField] private Color _closedColor = Color.white;
        [SerializeField] private Color _opendColor = Color.blue;

        [SerializeField, Fg_De] private bool _isOpenInfo;
        [SerializeField, Fg_De] private bool _isTextVisible;

        private void Start()
        {
            _content = transform.parent.GetComponent<RectTransform>();
            _answerText.ForEach(x => x.gameObject.SetActive(false));
        }

        private void Update()
        {
            _isTextVisible = _answerText[0]?.position.y > _threshold;
        }

        public void ToggleOpenClose()
        {
            _isOpenInfo = !_isOpenInfo;
            _answerText.ForEach(x => x.gameObject.SetActive(_isOpenInfo));

            if (_isOpenInfo)
            {
                _imageArrow.transform.DORotate(Vector3.zero, 0f);
                _background.DOColor(_opendColor, 0.5f);
            }
            else
            {
                _imageArrow.transform.DORotate(new Vector3(0, 0, 180f), 0f);
                _background.DOColor(_closedColor, 0.5f);
            }

            if (_content != null && _isOpenInfo == true && _isTextVisible == false)
            {
                _content?.DOAnchorPos3DY(_content.anchoredPosition3D.y + 200, 0.25f);
            }
        }

        public void TryOpenLink(string link)
        {
            if (link.StartsWith("https://") == false)
            {
                link = "https://" + link;
            }

            Application.OpenURL(link);
        }

        public void TryOpenSupport(string mailTo)
        {
            //Application.OpenURL("mailto:feedback@lalafun.net?subject=" + _textMessage);

            string subject = $"Colorings build {Application.version} support request";
            Application.OpenURL($"mailto:{mailTo}?subject={subject}");
        }

        [Button]
        private void SetAutoReference()
        {
            _questionText.StringReference = new LocalizedString
            {
                TableReference = Constants_Localization.Default,
                TableEntryReference = $"for_parents_FAQ_{LeaveOnlyNumbers(gameObject.name)}_question{_prefix}"
            };

            _answerText.ForEach(x =>
            {
                if (x.TryGetComponent<LocalizeStringEvent>(out var localizeStringEvent))
                {
                    localizeStringEvent.StringReference = new LocalizedString
                    {
                        TableReference = Constants_Localization.Default,
                        TableEntryReference = $"for_parents_FAQ_{LeaveOnlyNumbers(gameObject.name)}_answer{_prefix}"
                    };
                }
            });
        }

        private string LeaveOnlyNumbers(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            string cleaned = new string(s.Where(char.IsDigit).ToArray());
            return cleaned;
        }
    }
}
