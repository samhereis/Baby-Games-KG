using Modes.Puzzle;

namespace ColorfulTrain
{
    public class ColorfulTrain_StateBase : StateMachine_StateBase
    {
        protected ColorfulTrain_GameState_Model _model;

        public void Construct(ColorfulTrain_GameState_Model model)
        {
            _model = model;
        }
    }
}