using CoockingSalade;
using CustomAttributes;
using Modes.Puzzle;
using UnityEngine;

namespace Coocking
{
    public class CoockingIceCream_StateBase : StateMachine_StateBase
    {
        [SerializeField, Fg_De] protected bool _isDone;

        protected Coocking_GameState_Model _model;
        protected Coocking_Controller _controller;

        public void Construct(Coocking_GameState_Model model)
        {
            _model = model;
            _controller = _model.controller;
        }
    }
}