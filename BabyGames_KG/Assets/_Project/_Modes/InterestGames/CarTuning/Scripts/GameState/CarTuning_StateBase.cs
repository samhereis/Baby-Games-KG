using Assets._Project._Modes.Carwash.Scripts.State;
using Modes.Puzzle;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace CarTuning
{
    public class CarTuning_StateBase : StateMachine_StateBase
    {
        public Action<CarPart_Base> onPartSet;

        public GridLayoutGroup _holderGrid;
        public Vector2 _holderCellSize = new Vector2(250, 115);

        protected CarTuning_GameState_Model _model;

        public virtual void Construct(CarTuning_GameState_Model model)
        {
            _model = model;
        }

        public override async Task Enable()
        {
            await base.Enable();

            if (_holderGrid != null)
            {
                _holderGrid.cellSize = _holderCellSize;
            }
        }

        public override Task Enter()
        {
            if (_model.isFirstEntered == true)
            {
                _model.stateEnd_FX?.DoFX();
            }
            else
            {
                _model.isFirstEntered = true;
            }

            return base.Enter();
        }
    }
}