using DataClasses;
using Helpers;
using Services;
using Sounds;
using Spine.Unity;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Modes.Puzzle
{
    public class Puzzle_AnimationState : StateMachine_StateBase
    {
        [SerializeField] private SkeletonAnimation _skeletonAnimation;
        [SerializeField] private Puzzle _puzzle;
        [SerializeField] private Sound _finalSound;

        [Inject] private ISoundPlayer _soundPlayer;

        public async override Task PreInittialize()
        {
            await base.PreInittialize();

            _skeletonAnimation.AnimationState.ClearTracks();

            DiService.Inject(this);
        }

        public override async Task Enter()
        {
            await base.Enter();

            _puzzle.mainImage.ForEach(x =>
            {
                x.enabled = false;
            });

            _puzzle.PlayFinishAudio();

            _skeletonAnimation.AnimationState.SetAnimation(0, "action", false);
            await AsyncHelper.DelayFloat(4f);
            OnCompleted();
        }

        private void OnCompleted()
        {
            _soundPlayer.TryPlay(_finalSound);
            _puzzle.Complete();
        }
    }
}