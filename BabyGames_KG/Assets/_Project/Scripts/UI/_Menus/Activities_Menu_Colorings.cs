using DataClasses;
using DG.Tweening;
using GameState;
using Identifiers.UI;
using Services;
using Sirenix.OdinInspector;
using SO;
using Sounds;
using System.Threading.Tasks;
using UI.Menus;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Saratan.Coloring.Gallery
{
    public class Activities_Menu_Colorings : Activities_Menu
    {
        [ShowInInspector] protected static Vector2? _lastContentHolderPosition;

        [SerializeField] private Sound _dragSound;

        [Inject] private ISoundPlayer _soundPlayer;

        private float _sqrMagnitude = 0f;
        private bool _canPlay = true;
        private bool _firstPress = false;

        public override async Task Initialize(MainMenu_GameState_Model model, ActivityCategory_SO activityCategory)
        {
            await base.Initialize(model, activityCategory);
            DiService.Inject(this);
        }

        protected override void OnActivityChosen(Activity_UIUnit_Base activity_UIUnit_Base)
        {
            _lastContentHolderPosition = _contentHolder.anchoredPosition;
            base.OnActivityChosen(activity_UIUnit_Base);
        }

        private void Update()
        {
            if (Pointer.current.press.isPressed)
            {
                _sqrMagnitude = Pointer.current.delta.ReadValue().sqrMagnitude;
                if (_sqrMagnitude > 200)
                {
                    if (_canPlay == true)
                    {
                        _soundPlayer?.TryPlay(_dragSound);
                        _canPlay = false;
                    }
                }
                else
                {
                    _canPlay = true;
                }

                _firstPress = true;
            }

            if (Pointer.current.press.wasReleasedThisFrame)
            {
                _canPlay = true;
            }

            if (_lastContentHolderPosition != null && _firstPress == false)
            {
                _contentHolder.DOKill();
                _contentHolder.anchoredPosition = _lastContentHolderPosition.Value;
            }
        }
    }
}