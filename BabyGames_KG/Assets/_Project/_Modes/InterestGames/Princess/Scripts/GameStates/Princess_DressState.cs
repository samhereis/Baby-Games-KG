using CarTuning;
using DG.Tweening;
using FX;
using Services;
using Spine.Unity;
using System.Threading.Tasks;

namespace InterestGames
{
    public class Princess_DressState : Princess_StateBase
    {
        public override async Task Enter()
        {
            await base.Enter();
            _model.skinCombiner.spineObject.transform.DOMoveX(-2, 0.25f);
        }

        protected override void OnItemSet(Princess_DressBase dressBase)
        {
            base.OnItemSet(dressBase);

            if (dressBase == null) { return; }

            dressBase.gameObject.SetActive(false);

            _model.skinCombiner.skiBody = dressBase.skinName;
            _model.skinCombiner.Build();

            _model.activityIdentifier.Get<SkeletonAnimation>().AnimationState.ClearTracks();
            _model.activityIdentifier.Get<SkeletonAnimation>().AnimationState.SetAnimation(0, "LightDress", false);
        }
    }
}