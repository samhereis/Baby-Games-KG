using _Project.Scripts.Sound;
using Assets._Project._Modes.Carwash.Scripts.State;
using DG.Tweening;
using Helpers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Carwash
{
    public class CarSelecButton : MonoBehaviour, IPointerClickHandler
    {
        private Carwash_GameState_Model _model;

        public string animationName;
        public CarwashCar_Identifier car;
        public Transform icon;
        public Transform door;

        public string followBoneName = "bus2";
        public Vector3 cameraFollowOffset = Vector3.zero;

        public bool _isSelected;

        public void Construct(Carwash_GameState_Model model)
        {
            _model = model;
        }

        public async void OnPointerClick(PointerEventData eventData)
        {
            if (_model.isWaitingForCarSelection == false) { return; }

            if (_isSelected) return;
            _isSelected = true;

            Sound_FX.Play_Static(Sound_Effect.StartDrag);

            _model.SelectCar(this);

            await AsyncHelper.DelayFloat(3f);
            door.DOScale(0, 0.25f);
        }
    }
}
