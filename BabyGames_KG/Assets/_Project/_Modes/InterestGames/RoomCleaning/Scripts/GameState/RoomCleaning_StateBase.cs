using DG.Tweening;
using FX;
using Modes.Puzzle;
using Services;
using System.Threading.Tasks;
using UnityEngine;

namespace InterestGames
{
    public class RoomCleaning_StateBase : StateMachine_StateBase
    {
        [SerializeField] protected SpriteRenderer _curtain;
        [SerializeField] protected bool _isDone;
        [SerializeField] protected float _delayForWin = 1;

        protected GameplayGameState_RoomCleaning_Model _model;

        public void Construct(GameplayGameState_RoomCleaning_Model model)
        {
            _model = model;
        }

        public override async Task Enter()
        {
            await base.Enter();
        }

        public override async Task Exit()
        {
            _nextState = null;
            await base.Exit();
        }

        protected async Task FadeCurtain(float value)
        {
            _curtain.gameObject.SetActive(true);
            _curtain.DOKill();
            await _curtain.DOFade(value, 0.25f).AsyncWaitForCompletion();
        }
    }
}