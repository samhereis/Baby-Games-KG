using Sirenix.OdinInspector;
using UI.Menus;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Windows
{
    public class LoadingMenu : MenuBase
    {
        [Header("UI Elements")]
        [Required]
        [SerializeField] protected Slider _progressSlider;

        public void SetProgress(float progress)
        {
            _progressSlider.value = progress;
        }
    }
}