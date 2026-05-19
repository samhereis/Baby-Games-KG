using DG.Tweening;
using Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace CarTuning
{
    public class CarTuning_SyreneLightState : CarTuning_StateBase
    {
        [SerializeField] private List<CarPart_WithAnimation> _wheels = new();

        public override async Task Enable()
        {
            _wheels = _model.currentCar.value.syreneLight_Position.GetComponentsInChildren<CarPart_WithAnimation>(true).ToList();

            await base.Enable();
        }

        public override async Task Enter()
        {
            await base.Enter();

            _model.onInitializeState?.Invoke(_wheels.Select(x => x.GetComponent<CarPart_Base>()).ToList());

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
            foreach (var item in _wheels)
            {
                item.gameObject.SetActive(item.gameObject == currentCar.gameObject);
                item.transform.DOScale((item.gameObject == currentCar.gameObject).ToInt(), 0.25f).SetEase(Ease.OutBack);
            }
            
            _model.onStateCompleted?.Invoke();
            onPartSet?.Invoke(currentCar);
        }

        private void NextStep()
        {
            _nextState = _nextStateOnWin;
        }
    }
}