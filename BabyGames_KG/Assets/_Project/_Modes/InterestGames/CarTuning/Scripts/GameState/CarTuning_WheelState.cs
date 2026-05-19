using DG.Tweening;
using Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace CarTuning
{
    public class CarTuning_WheelState : CarTuning_StateBase
    {
        [SerializeField] private List<CarPart_NoAnimation> _wheels_Front = new();
        [SerializeField] private List<CarPart_Base> _wheels_Back = new();

        public override async Task Enable()
        {
            _wheels_Front = _model.currentCar.value.frontWheel_Position.GetComponentsInChildren<CarPart_NoAnimation>(true).ToList();
            _wheels_Back = _wheels_Front.Select(x => x.second).ToList();

            await base.Enable();
        }

        public override async Task Enter()
        {
            await base.Enter();

            _model.onInitializeState?.Invoke(_wheels_Front.Select(x => x.GetComponent<CarPart_Base>()).ToList());

            _model.currentCarPart.AddListener(OnCarPartChanged);
            _model.onCompleteClicked += NextStep;
        }

        public override Task Exit()
        {
            _model.currentCarPart.RemoveListener(OnCarPartChanged);
            _model.onCompleteClicked -= NextStep;

            return base.Exit();
        }

        private void OnCarPartChanged(CarPart_Base currentCar)
        {
            if (_wheels_Front.Contains(currentCar))
            {
                foreach (var item in _wheels_Front)
                {
                    item.gameObject.SetActive(item.gameObject == currentCar.gameObject);
                    item.transform.DOScale((item.gameObject == currentCar.gameObject).ToInt(), 0.25f).SetEase(Ease.OutBack);
                }
            }

            if (_wheels_Back.Contains(currentCar))
            {
                foreach (var item in _wheels_Back)
                {
                    item.gameObject.SetActive(item.gameObject == currentCar.gameObject);
                    item.transform.DOScale((item.gameObject == currentCar.gameObject).ToInt(), 0.25f).SetEase(Ease.OutBack);
                }
            }

            if (_wheels_Front.Exists(x => x.gameObject.activeInHierarchy) && _wheels_Back.Exists(x => x.gameObject.activeInHierarchy))
            {
                _model.onStateCompleted?.Invoke();
            }
            onPartSet?.Invoke(currentCar);
        }

        private void NextStep()
        {
            _nextState = _nextStateOnWin;
        }
    }
}