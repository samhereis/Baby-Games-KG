using CoockingSalade;
using DG.Tweening;
using Helpers;
using Identifiers;
using Sirenix.OdinInspector;
using System.Threading.Tasks;
using UnityEngine;

namespace Coocking
{
    public class CoockingPizza_Duhovka : CoockingSalade_StateBase
    {
        public Transform duhovka;
        public Transform pizza;
        public Transform pizza_Podnos;
        public Transform pizza_Podnos_New;

        public SpriteRenderer pizzaRenderer;
        public Transform progress;
        public Color pizzaReadyColor;

        public Transform duhovkaTargetTransform;
        public Transform pizzaTargetTransform;

        public Panel_World panel_World;

        [Space]
        public CoockingSalade_StateBase _finalState;

        [Button]
        public override async Task Enter()
        {
            await base.Enable();

            await controller.ShowCurtain(true);
            panel_World.Disppear();
            progress.transform.DOScale(0, 0);
            await AsyncHelper.DelayFloat(0.25f);

            duhovka.transform.DOMove(duhovkaTargetTransform.position, 0);
            duhovka.transform.DORotate(duhovkaTargetTransform.eulerAngles, 0);
            duhovka.transform.DOScale(duhovkaTargetTransform.localScale, 0);

            pizza.DOMoveY(-15, 0);

            pizza_Podnos_New.gameObject.SetActive(true);
            pizza_Podnos.gameObject.SetActive(false);

            foreach (var item in pizza.GetComponentsInChildren<SpriteRenderer>(true))
            {
                item.sortingLayerName = "Front";
            }

            await AsyncHelper.DelayFloat(1f);
            await controller.ShowCurtain(false);

            pizza.transform.DOMove(pizzaTargetTransform.position, 1);
            pizza.transform.DORotate(pizzaTargetTransform.eulerAngles, 1);
            pizza.transform.DOScale(pizzaTargetTransform.localScale, 1);

            await MakeReady();

            Win();
        }

        private async Task MakeReady()
        {
            progress.transform.DOScale(1, 4);
            await pizzaRenderer.DOColor(pizzaReadyColor, 5).AsyncWaitForCompletion();
        }

        private async void Win()
        {
            if (_finalState != null)
            {
                duhovka.transform.SetParent(null);
                _nextState = _finalState;
            }
            else
            {
                _model.onFinish?.Invoke();
            }
        }
    }
}