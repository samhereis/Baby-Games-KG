using _Project.Scripts.Sound;
using CoockingSalade;
using DG.Tweening;
using Helpers;
using Sirenix.OdinInspector;
using Sounds;
using System.Threading.Tasks;
using UnityEngine;

namespace Coocking
{
    public class CoockingPizza_Final : CoockingSalade_StateBase
    {
        public Transform pizza;
        public Transform oldPlate;
        public Transform plate;
        public Transform duhovka;
        public float showTime = 1;
        public SoundQueue _soYummySound;

        [Button]
        public override async Task Enter()
        {
            await base.Enter();

            await controller.ShowCurtain(true);
            await controller.ChangeBackground(backgroundIndex);
            await AsyncHelper.DelayFloat(1f);

            oldPlate?.gameObject.SetActive(false);
            duhovka?.gameObject.SetActive(false);
            plate?.gameObject.SetActive(true);
            pizza.transform.SetParent(plate, true);
            await pizza.transform.DOMoveY(-25, 0).AsyncWaitForCompletion();

            await controller.ShowCurtain(false);

            await pizza.transform.DOMoveY(0, 1).AsyncWaitForCompletion();
            await AsyncHelper.DelayFloat(showTime);

            Win();
            await Sound_FX.PlayAsync_Static(_soYummySound);
        }

        private void Win()
        {
            _model.onFinish?.Invoke();
        }
    }
}