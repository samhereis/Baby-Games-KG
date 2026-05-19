using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ToggleButton : MonoBehaviour
    {
        public Toggle toggle;

        [Space]
        public Image backgroundImage;
        public RectTransform switchImage;

        [Space]
        public Vector3 _positionOff;
        public Vector3 _positionOn;

        private void Awake()
        {
            toggle.onValueChanged.AddListener(ToggleValueChanged);
            ToggleValueChanged(toggle.isOn);
        }

        [Button]
        private void ToggleValueChanged(bool isOn)
        {
            Vector3 pos = isOn ? _positionOn : _positionOff;

            if (Application.isPlaying)
            {
                backgroundImage.DOKill();
                switchImage.DOKill();

                switchImage.DOAnchorPos3D(pos, 0.25f).SetEase(Ease.InOutBack);
            }
            else
            {
                switchImage.anchoredPosition3D = pos;
            }
        }
    }
}