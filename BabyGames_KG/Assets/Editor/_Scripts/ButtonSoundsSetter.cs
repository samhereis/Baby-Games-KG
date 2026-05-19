using Sirenix.OdinInspector;
using Sirenix.Utilities;
using SO.Lists;
using UI.Effects;
using UI.Menus;
using UnityEngine;

namespace EditorHelper
{
    [RequireComponent(typeof(MenuBase))]
    [DisallowMultipleComponent]
    public class ButtonSoundsSetter : MonoBehaviour, ISelfValidator
    {
#if UNITY_EDITOR
        [SerializeField] private MenuBase _menu;
        [SerializeField] private Sound_SO _sound_SO;
#endif

        public void Validate(SelfValidationResult result)
        {
#if UNITY_EDITOR
            if (_menu == null) { _menu = GetComponent<MenuBase>(); }

            _menu.buttons.ForEach(button =>
            {
                if (button.TryGetComponent<ButtonSound>(out var buttonSound) == false) { buttonSound = button.gameObject.AddComponent<ButtonSound>(); }
                buttonSound.SetSound(_sound_SO);
            });
#endif
        }
    }
}