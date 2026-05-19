using CarTuning;
using Spine.Unity;

namespace InterestGames
{
    public class Princess_ShoesState : Princess_StateBase
    {
        protected override void OnItemSet(Princess_DressBase dressBase)
        {
            base.OnItemSet(dressBase);

            if (dressBase == null) { return; }

            dressBase.gameObject.SetActive(false);

            _model.skinCombiner.skiTufli = dressBase.skinName;
            _model.skinCombiner.Build();

            _model.activityIdentifier.Get<SkeletonAnimation>().AnimationState.ClearTracks();
            _model.activityIdentifier.Get<SkeletonAnimation>().AnimationState.SetAnimation(0, "LightTufli", false);
        }
    }
}