using FX;
using Modes.Puzzle;
using Services;
using System.Threading.Tasks;
using UnityEngine;

namespace InterestGames
{
    public class Makeup_StateBase : StateMachine_StateBase
    {
        [SerializeField] protected bool _isDone;

        protected GameplayGameState_Makeup_Model _model;

        public void Construct(GameplayGameState_Makeup_Model model)
        {
            _model = model;
        }

        public override Task Enter()
        {
            if (_model != null)
            {
                if (_model.firstEnterDone) { DiService.Get<StateEnd_FX>()?.DoFX(); }
                else { _model.firstEnterDone = true; }
            }

            return base.Enter();
        }
    }
}