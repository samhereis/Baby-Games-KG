using Bubble;
using DataClasses;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;

namespace Identifiers
{
    public class BubbleActivity_Identifier : _ActivityBase_Identifier
    {
        [SerializeField] private SkeletonAnimation _background;

        public override void UpdateData(Activity activity)
        {
            base.UpdateData(activity);

            type = DataClasses.ActivityType.Bubble;
        }

        [Button]
        public void Initialize()
        {
            _background.AnimationState.ClearTracks();
            _background.AnimationState.SetAnimation(0, "action", true);

            Get<BubbleController>().Construct(FindAnyObjectByType<GameplayMenu_Bubble>());
            Get<BubbleController>().Initialize();
        }
    }
}