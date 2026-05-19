using FX;
using InterestGames;
using Modes.Puzzle;
using Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarTuning
{
    public class Princess_StateBase : StateMachine_StateBase
    {
        public Action<Princess_DressBase> onItemSet;

        public List<Princess_DressBase> wearings = new();

        protected Gameplay_GameState_InterestGameGirl_Model _model;

        public virtual void Construct(Gameplay_GameState_InterestGameGirl_Model model)
        {
            _model = model;
        }

        public override Task Enter()
        {
            if (_model.firstEnterDone && this is not Princess_WinState) { DiService.Get<StateEnd_FX>()?.DoFX(); }
            else { _model.firstEnterDone = true; }

            _model.currentState?.ChangeValue(this);

            _model.currentDress.AddListener(OnItemSet);
            _model.onNextStateRequested += SetNextState;

            OnItemSet(null);
            return base.Enter();
        }

        public override Task Exit()
        {
            _model.currentDress.RemoveListener(OnItemSet);
            _model.onNextStateRequested -= SetNextState;

            return base.Exit();
        }

        protected virtual void OnItemSet(Princess_DressBase dressBase)
        {
            foreach (var item in wearings)
            {
                item.SetVisible(_model.currentDress.value == item);
            }

            onItemSet?.Invoke(dressBase);
        }

        protected virtual void SetNextState()
        {
            _nextState = _nextStateOnWin;
        }
    }
}