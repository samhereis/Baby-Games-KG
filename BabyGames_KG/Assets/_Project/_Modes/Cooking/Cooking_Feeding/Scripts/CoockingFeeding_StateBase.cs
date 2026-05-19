using CoockingSalade;
using CustomAttributes;
using Modes.Puzzle;
using System.Threading.Tasks;
using Helpers;
using UnityEngine;

namespace Coocking
{
    public class CoockingFeeding_StateBase : StateMachine_StateBase
    {
        public SpriteRenderer curtain;
        public int backgroundIndex;

        [SerializeField, Fg_De] protected bool _isDone;
        [SerializeField, Fg_De] protected Coocking_Controller _controller;

        protected Coocking_GameState_Model _model;

        public void Construct(Coocking_GameState_Model model)
        {
            _model = model;

            if (_controller == null)
            {
                _controller = FindAnyObjectByType<Coocking_Controller>();
            }
        }

        public override async Task Enable()
        {
            var shouldChangeBG = _controller.currentBackground != backgroundIndex;
            
            if (shouldChangeBG)
            {
                await _controller.ShowCurtain(true);
                await _controller.ChangeBackground(backgroundIndex);
            }
            await base.Enable();
            
            AsyncHelper.DoDelayed(async () =>
            {
                if (shouldChangeBG) { await _controller.ShowCurtain(false); }
            }, 0.5f);
        }
    }
}