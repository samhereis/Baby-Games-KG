using Modes.Puzzle;
using System.Threading.Tasks;
using UnityEngine;

namespace CoockingSalade
{
    public class CoockingSalade_StateBase : StateMachine_StateBase
    {
        public bool isDone = false;
        public int backgroundIndex;

        public GameObject[] turnOn;
        public GameObject[] turnOff;

        public Coocking_Controller controller;

        protected Coocking_GameState_Model _model;

        public override Task Enable()
        {
            foreach (var turn in turnOff)
            {
                turn.gameObject.SetActive(false);
            }

            foreach (var turn in turnOn)
            {
                turn.gameObject.SetActive(true);
            }

            return base.Enable();
        }

        public void Construct(Coocking_GameState_Model model)
        {
            controller = model.controller;
            _model = model;
        }
    }
}