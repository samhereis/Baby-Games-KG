using System;
using Helpers;
using Loggers;
using Spine.Unity;

namespace CarTuning
{
    public class CarPart_WithAnimation : CarPart_Base
    {
        public SkeletonAnimation skeletonAnimation;
        public bool _doBlick = true;

        public async void DoBlick()
        {
            try
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

                    vkl = !vkl;

                    await AsyncHelper.DelayFloat(0.25f);
                }
            } catch (Exception e) { CustomLogger.instance.LogException(e); }
        }

        private void Awake()
        {
            skeletonAnimation = GetComponentInChildren<SkeletonAnimation>(true);
            gameObject.SetActive(false);
        }
    }
}