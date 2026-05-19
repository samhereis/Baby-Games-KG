using CoockingSalade;
using Modes.Puzzle;
using UnityEngine;

namespace Coocking
{
    public class CoockingBurger_StateBase : StateMachine_StateBase
    {
        [SerializeField] protected bool _isDone;
        [SerializeField] protected int _backgroundIndex;

        protected Coocking_GameState_Model _model;
        protected Coocking_Controller _controller;

        public void Construct(Coocking_GameState_Model model)
        {
            _model = model;
        }

        public void Construct(Coocking_GameState_Model model, Coocking_Controller controller)
        {
            _model = model;
            _controller = controller;
        }
    }
}