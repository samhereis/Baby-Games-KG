using Assets._Project._Modes.Carwash.Scripts.State;
using Modes.Puzzle;

namespace CarTuning
{
    public class CarTuning_Controller : StateMachineBase
    {
        private CarTuning_GameState_Model _model;

        public void Construct(CarTuning_GameState_Model model)
        {
            _model = model;

            foreach (var item in _allStates)
            {
                if (item is CarTuning_StateBase carTuning_StateBase)
                {
                    carTuning_StateBase.Construct(_model);

                    carTuning_StateBase.onPartSet -= PlayAudio;
                    carTuning_StateBase.onPartSet += PlayAudio;
                }
            }
        }

        public void Initialize()
        {
            ChangeState(_startState);
        }

        private void PlayAudio(CarPart_Base carPart_Base)
        {
            _Project.Scripts.Sound.Sound_FX.audioPlayer?.TryPlay(_model.activity_Identifier.setSound);
        }
    }
}