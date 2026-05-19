using CustomAttributes;
using DG.Tweening;
using Febucci.UI;
using Febucci.UI.Effects;
using Helpers;
using RTLTMPro;
using Services;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace UI
{
    [Serializable]
    public class CurvedTextSetter_Data
    {
        [FoldoutGroup("$language")] public string language;
        [FoldoutGroup("$language")] public float amplitude;
        [FoldoutGroup("$language")][Range(0, 1)] public float wave;
        [FoldoutGroup("$language")] public float yPosition = 0;
        [FoldoutGroup("$language")][Range(0, 1)] public float scale = 1;
    }

    [DisallowMultipleComponent]
    public class CurvedTextSetter : MonoBehaviour, ISelfValidator
    {
        public RTLTextMeshPro text;
        public TextAnimator_TMP textAnimator;

        [Space]
        public string curveTag;

        [Space]
        public List<CurvedTextSetter_Data> settingsByLanguage = new();

        [Fg_De, SerializeField] private string _currentLanguage;
        [Fg_De, SerializeField] private string _startText;
        [Fg_De, SerializeField] private string _endText;
        [Fg_De, SerializeField] private WaveBehavior _curveBehavior;

        private static LazyUpdator_Service _lazyUpdator = new();

        public void Validate(SelfValidationResult result)
        {
            if (text == null) { text = GetComponentInChildren<RTLTextMeshPro>(true); }
            if (textAnimator == null) { textAnimator = GetComponentInChildren<TextAnimator_TMP>(true); }
        }

        private void Awake()
        {
            _startText = $"<{curveTag}>";
            _endText = $"</{curveTag}>";
        }

        private void OnEnable()
        {
            _lazyUpdator.AddToQueue(SetCurve);
        }

        private void OnDisable()
        {
            _lazyUpdator.RemoveFromQueue(SetCurve);
        }

        [Button]
        private async Task SetCurve()
        {
            if (text.OriginalText.StartsWith(_startText) == false)
            {
                text.OriginalText = $"{_startText}{text.text}";
                text.text = $"{_startText}{text.text}";
                text.SetText($"{_startText}{text.text}");
            }

            if (text.OriginalText.EndsWith(_endText) == false)
            {
                text.OriginalText = $"{text.text}{_endText}";
                text.text = $"{text.text}{_endText}";
                text.SetText($"{text.text}{_endText}");
            }

            _currentLanguage = LocalizationSettings.SelectedLocale.name;
            CurvedTextSetter_Data data = settingsByLanguage.Find(x => x.language == _currentLanguage);
            if (data != null)
            {
                var t = textAnimator.DatabaseBehaviors.Data.First(x => x.TagID == curveTag);
                if (t == null) { return; }
                _curveBehavior = t as WaveBehavior;

                if (_curveBehavior != null)
                {
                    _curveBehavior.baseAmplitude = data.amplitude;
                    _curveBehavior.SetModifier(new ModifierInfo("a", data.amplitude));

                    _curveBehavior.baseWaveSize = data.wave;
                    _curveBehavior.SetModifier(new ModifierInfo("w", data.amplitude));
                }

                text.rectTransform.DOAnchorPosY(data.yPosition, 0.25f);
                text.rectTransform.DOScale(data.scale, 0.25f);
            }

            await AsyncHelper.DelayFloat(0.25f);
        }
    }
}