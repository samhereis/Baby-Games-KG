using CoockingSalade;
using DG.Tweening;
using FX;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace Coocking
{
    public class CookingSalade_Pizza_Forming : CoockingSalade_StateBase
    {
        public CoockingPizza_Testo _testo;
        public Transform pizza;

        [SerializeField] private HintHand_DrawDots _hintHand_DDrawDots;

        public override async Task Enter()
        {
            await controller.ShowCurtain(true);
            controller.ChangeBackground(backgroundIndex);
            await base.Enter();

            pizza.DOMove(Vector3.zero, 0.25f);

            _testo.Initialize();
            _testo.isCompleted.AddListener(OnCompleted);

            await controller.ShowCurtain(false);

            _hintHand_DDrawDots = GetComponent<HintHand_DrawDots>();
            _hintHand_DDrawDots?.SetIsActive(true);
        }

        public override async Task Exit()
        {
            _testo.isCompleted.RemoveListener(OnCompleted);

            await base.Exit();
        }

        public override void ForceWin()
        {
            base.ForceWin();
            _testo.End();
            _nextState = _nextStateOnWin;
        }

        private void OnCompleted(bool obj)
        {
            _nextState = _nextStateOnWin;
        }
    }
}