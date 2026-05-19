using CarTuning;
using DataClasses;
using DG.Tweening;
using Helpers;
using Services;
using Spine.Unity;
using System.Threading.Tasks;
using Zenject;

namespace InterestGames
{
    public class Princess_PuppyState : Princess_StateBase
    {
        protected Princess_Dress_WithAnimation currentPuppet;

        [Inject] private ISoundPlayer _soundPlayer;

        public override async Task Enter()
        {
            await base.Enter();
            DiService.Inject(this);

            _model.skinCombiner.spineObject.transform.DOMoveX(-4.5f, 0.25f);
        }

        protected override void OnItemSet(Princess_DressBase dressBase)
        {
            base.OnItemSet(dressBase);

            if (dressBase == null)
            {
                return;
            }

            currentPuppet = dressBase as Princess_Dress_WithAnimation;
        }

        protected override async void SetNextState()
        {
            currentPuppet.gameObject.SetActive(true);
            currentPuppet.transform.SetParent(transform.parent, true);

            _model.activityIdentifier.Get<SkeletonAnimation>().AnimationState.ClearTracks();
            _model.activityIdentifier.Get<SkeletonAnimation>().AnimationState.SetAnimation(0, currentPuppet.skinName, false);

            _soundPlayer.TryPlay(currentPuppet.sound);

            currentPuppet.skeletonAnimation.AnimationState.ClearTracks();
            currentPuppet.skeletonAnimation.AnimationState.SetAnimation(0, "dance", false);

            await AsyncHelper.DelayFloat(2f);
            _nextState = _nextStateOnWin;
        }
    }
}