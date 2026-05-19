using FX;
using Helpers;
using Services;
using Sirenix.OdinInspector;
using System.Linq;
using System.Threading.Tasks;

namespace ColorfulTrain
{
    public class ColorfulTrain_SpecialTransportState : ColorfulTrain_WheelGivingState
    {
        public override async Task Enter()
        {
            GetComponent<HintHand_Drag>().SetIsActive(true);

            await base.Enter();

            foreach (var item in places)
            {
                item.gameObject.SetActive(false);
            }

            ActivateNextCar(doneWheels.Count);

            SetHintData();
        }

        protected override async void OnWheelDropped(ColorfulTrain_Wheel wheel)
        {
            if (_dct.IsCancellationRequested) { return; }

            GetComponent<HintHand_Drag>().SetIsActive(false);
            doneWheels.SafeAdd(wheel);
            DiService.Get<StateEnd_FX>()?.DoFX();

            foreach (var item in wheellessCars)
            {
                if (item.wheel.isDropped.value == true && item.isDone.value == false)
                {
                    await item.MarkDone();
                }
            }

            if (_model.train.seats.TrueForAll(x => x.isDropped.value))
            {
                await AsyncHelper.DelayFloat(2f);

                if (_dct.IsCancellationRequested) { return; }
                _model.onFinish?.Invoke();
            }
            else
            {
                GetComponent<HintHand_Drag>().SetIsActive(false);

                ActivateNextCar(doneWheels.Count);

                await _model.train.Prepare();
                _model.controller.GoNext();
                await _model.train.Go();

                DeactivateExcept(doneWheels.Count);

                SetHintData();
            }
        }

        [Button]
        protected override void SetHintData()
        {
            var currentWheellessCar = places[doneWheels.Count].GetComponentInChildren<ColorfulTrain_WheellessCar>();

            GetComponent<HintHand_Drag>().objects = _model.train.seats.Where(x => x == currentWheellessCar.wheel).Select(x => x.spriteRenderer.transform).ToList();
            GetComponent<HintHand_Drag>().targets = _model.train.seats.Where(x => x == currentWheellessCar.wheel).Select(x => x.targetPlace).ToList();

            PlayerActions_DataHolder.ResetTime();
            GetComponent<HintHand_Drag>().SetIsActive(true);
        }

        protected void ActivateNextCar(int index)
        {
            places[index].gameObject.SetActive(true);
        }

        protected void DeactivateExcept(int index)
        {
            int index_temp = 0;
            foreach (var item in places)
            {
                places[index_temp].GetComponentInChildren<ColorfulTrain_WheellessCar>().wheel.canDrag = true;
                places[index_temp].gameObject.SetActive(index_temp == index);
                index_temp++;
            }
        }
    }
}