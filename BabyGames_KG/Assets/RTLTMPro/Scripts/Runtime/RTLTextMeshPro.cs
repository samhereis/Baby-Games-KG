using System.Globalization;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace RTLTMPro
{
    [ExecuteInEditMode]
    public class RTLTextMeshPro : TextMeshProUGUI
    {
        // ReSharper disable once InconsistentNaming
#if TMP_VERSION_2_1_0_OR_NEWER
        public override string text
#else
        public new string text
#endif
        {
            get { return base.text; }
            set
            {
                if (originalText == value)
                    return;

                originalText = value;

                UpdateText();
            }
        }

        public string OriginalText
        {
            get { return originalText; }
            set { originalText = value; }
        }

        public bool PreserveNumbers
        {
            get { return true; }
            set
            {
                if (preserveNumbers == value)
                    return;

                preserveNumbers = value;
                havePropertiesChanged = true;
            }
        }

        public bool Farsi
        {
            get { return farsi; }
            set
            {
                if (farsi == value)
                    return;

                farsi = value;
                havePropertiesChanged = true;
            }
        }

        public bool FixTags
        {
            get { return fixTags; }
            set
            {
                if (fixTags == value)
                    return;

                fixTags = value;
                havePropertiesChanged = true;
            }
        }

        public bool ForceFix
        {
            get { return forceFix; }
            set
            {
                if (forceFix == value)
                    return;

                forceFix = value;
                havePropertiesChanged = true;
            }
        }

        [SerializeField] protected bool preserveNumbers;

        [SerializeField] protected bool farsi = true;

        [SerializeField] [TextArea(3, 10)] protected string originalText;

        [SerializeField] protected bool fixTags = true;

        [SerializeField] protected bool forceFix;

        protected readonly FastStringBuilder finalText = new FastStringBuilder(RTLSupport.DefaultBufferSize);

#if UNITY_EDITOR
        protected override void Reset()
        {
            raycastTarget = false;
        }
#endif

        protected void Update()
        {
            if (havePropertiesChanged)
            {
                UpdateText();
            }
        }

        public void UpdateText()
        {
            if (originalText == null) { originalText = ""; }

            if (ForceFix == false && TextUtils.IsRTLInput(originalText) == false)
            {
                isRightToLeftText = false;
                base.text = ToSentenceCase(originalText);
            }
            else
            {
                isRightToLeftText = true;
                base.text = ToSentenceCase(GetFixedText(originalText));
            }

            havePropertiesChanged = true;
        }

        private string ToSentenceCase(string input)
        {
            if (string.IsNullOrEmpty(input)) { return input; }

            string lowerInput = input.ToLower(CultureInfo.CurrentCulture);

            return Regex.Replace(lowerInput, @"(^|\.\s+|\!\s+|\?\s+)([a-zа-яөүң])", m =>
                    m.Groups[1].Value + m.Groups[2].Value.ToUpper(CultureInfo.CurrentCulture),
                RegexOptions.IgnoreCase);
        }

        private string GetFixedText(string input)
        {
            if (string.IsNullOrEmpty(input)) { return input; }

            finalText.Clear();
            RTLSupport.FixRTL(input, finalText, farsi, fixTags, PreserveNumbers);
            finalText.Reverse();
            return finalText.ToString();
        }
    }
}