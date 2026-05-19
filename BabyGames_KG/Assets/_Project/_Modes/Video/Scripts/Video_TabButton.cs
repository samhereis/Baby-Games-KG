using DG.Tweening;
using Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Video
{
    public class Video_TabButton : MonoBehaviour
    {
        public Button button;
        public TextMeshProUGUI text;
        public Image background;

        public Color activeColor;
        public Color inactiveColor;
        public Color inactiveColor_BackImage;

        public void SetActiveStatus(bool active)
        {
            text.DOColor(active ? activeColor : inactiveColor, 0.5f);
            background.DOColor(active ? activeColor : inactiveColor_BackImage, 0.5f);
        }
    }
}