using Sirenix.OdinInspector;
using SO;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Interaction
{
    [RequireComponent(typeof(Button))]
    public class ButtonFX : MonoBehaviour, ISelfValidator
    {
        [SerializeField] private Sound_String_SO _clickSound;
        [SerializeField] private Sound_String_SO _hoverSound;
        [SerializeField] private Button _button;

        private bool _hasDownAnimationEnded = false;

        public void Validate(SelfValidationResult result)
        {
            if (_button == null) { _button = GetComponent<Button>(); }
        }

        private void Awake()
        {
            if (_button == null) { _button = GetComponent<Button>(); }
        }

        private void OnEnable()
        {
            _button?.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            _button?.onClick.RemoveListener(OnClick);
        }

        public void OnClick()
        {

        }
    }
}