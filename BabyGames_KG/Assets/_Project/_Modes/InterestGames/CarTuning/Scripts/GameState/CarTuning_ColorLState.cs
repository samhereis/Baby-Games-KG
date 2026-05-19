using System;
using _Project.Scripts.Sound;
using Helpers;
using Sirenix.OdinInspector;
using System.Threading.Tasks;
using Loggers;

namespace CarTuning
{
    public class CarTuning_ColorLState : CarTuning_StateBase
    {
        public override async Task Enter()
        {
            _model.onCompleteClicked -= NextStep;
            _model.onCompleteClicked += NextStep;

            _model.onColorState?.Invoke();

            await base.Enter();
        }

        [Button]
        private async void NextStep()
        {
            try
            {
                foreach (var item in _model.currentCar.value.GetComponentsInChildren<CarPart_WithAnimation>())
                {
                    if (item.gameObject.activeSelf == false)
                    {
                        continue;
                    }

                    item.DoBlick();
                }

                _model.currentCar.value.drawable.enabled = false;
                

                _model.soundPlayer?.TryPlay(_model.activity_Identifier.finishSound_2);
                await AsyncHelper.DelayFloat(1);
                Sound_FX.audioPlayer?.TryPlay(_model.activity_Identifier.finishSound);

                await AsyncHelper.DelayFloat(3f);
            } catch (Exception e)
            {
                CustomLogger.instance.LogException(e);
            }

            _model.onFinish?.Invoke();
        }
    }
}