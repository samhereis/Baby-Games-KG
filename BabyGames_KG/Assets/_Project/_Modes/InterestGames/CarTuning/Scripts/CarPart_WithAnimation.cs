using Helpers;
using Spine.Unity;

namespace CarTuning
{
    public class CarPart_WithAnimation : CarPart_Base
    {
        public SkeletonAnimation skeletonAnimation;
        public bool _doBlick = true;

        public async void DoBlick()
        {
            if (_doBlick == false)
            {
                skeletonAnimation.AnimationName = "idle_vkl";
                return;
            }

            bool vkl = false;
            for (int i = 0; i < 10; i++)
            {
                if (gameObject == null) { return; }

                skeletonAnimation.AnimationName = vkl ? "idle_vkl" : "idle_vykl";

                //car.skeletonAnimation.AnimationState.ClearTracks();
                //car.skeletonAnimation.AnimationState.SetAnimation(0, vkl ? "idle_vkl" : "idle_vykl", false);

                vkl = !vkl;

                await AsyncHelper.DelayFloat(0.25f);
            }
        }

        private void Awake()
        {
            skeletonAnimation = GetComponentInChildren<SkeletonAnimation>(true);
            gameObject.SetActive(false);
        }
    }
}