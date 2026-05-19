using DG.Tweening;
using Modes.Puzzle;
using UnityEngine;

namespace InterestGames
{
    public class Interests_RoomCleaning_Controller : StateMachineBase
    {
        protected GameplayGameState_RoomCleaning_Model _model;

        public bool autoInitialize;

        private void Awake()
        {
#if UNITY_EDITOR
            if (autoInitialize)
            {
                Initialize(null);
            }
#endif
        }

        public virtual void Initialize(GameplayGameState_RoomCleaning_Model model)
        {
            _model = model;
            _model?._skinCombiner?.Build();

            foreach (var item in _allStates)
            {
                if (item is RoomCleaning_StateBase princess_StateBase)
                {
                    princess_StateBase.Construct(_model);
                }
            }

            ChangeState(_startState);
        }
    }
}