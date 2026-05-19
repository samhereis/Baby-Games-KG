using _Project.Scripts.Sound;
using CarTuning;
using Modes.Puzzle;
using Sounds;

namespace InterestGames
{
    public class Princess_Controller : StateMachineBase
    {
        protected Gameplay_GameState_InterestGameGirl_Model _model;

        public bool autoInitialize;
        public Sound _shoosSound;

        private void Awake()
        {
#if UNITY_EDITOR
            if (autoInitialize)
            {
                Initialize(null);
            }
#endif
        }

        public virtual void Initialize(Gameplay_GameState_InterestGameGirl_Model model)
        {
            _model = model;
            _model?.skinCombiner?.Build();

            int index = 0;
            foreach (var item in _allStates)
            {
                if (item is Princess_StateBase princess_StateBase)
                {
                    princess_StateBase.Construct(_model);

                    if (index <= 4)
                    {
                        princess_StateBase.onItemSet -= PlayAudio;
                        princess_StateBase.onItemSet += PlayAudio;
                    }
                }

                index++;
            }

            ChangeState(_startState);
        }

        private void PlayAudio(Princess_DressBase dressBase)
        {
            if (dressBase == null) { return; }
            Sound_FX.audioPlayer?.TryPlay(_shoosSound);
        }
    }
}