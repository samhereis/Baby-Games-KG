using DG.Tweening;
using Helpers;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace CarTuning
{
    public class CarTuning_CorpusState : CarTuning_StateBase
    {
        public override Task Enable()
        {
            return base.Enable();
        }

        public override async Task Enter()
        {
            await base.Enter();

            _model.onInitializeState?.Invoke(_model.activity_Identifier.cars.Select(x => x.GetComponent<CarPart_NoAnimation>() as CarPart_Base).ToList());

            _model.currentCarPart.RemoveListener(OnCarPartChanged);
            _model.onCompleteClicked -= NextStep;

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
            foreach (var item in _model.activity_Identifier.cars)
            {
                item.gameObject.SetActive(item.gameObject == currentCar.gameObject);
                item.transform.DOScale((item.gameObject == currentCar.gameObject).ToInt(), 0.25f).SetEase(Ease.OutBack);
            }
            
            _model.onStateCompleted?.Invoke();
            onPartSet?.Invoke(currentCar);
        }

        private void NextStep()
        {
            _model.currentCar.ChangeValue(_model.currentCarPart.value.GetComponent<CarTuning_Car_Identifier>());
            _nextState = _nextStateOnWin;
        }
    }
}