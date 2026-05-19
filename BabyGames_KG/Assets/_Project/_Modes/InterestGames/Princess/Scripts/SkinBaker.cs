using Spine;
using Spine.Unity;
using System.Collections.Generic;
using UnityEngine;

namespace InterestGames
{
    public class SkinBaker : MonoBehaviour
    {
        [SpineSkin] public List<string> skins_final;
        [SerializeField] public SkeletonAnimation spineObject;
        [SerializeField] public SkeletonGraphic skeletonGraphic;
        public Skin combinedSkin;
        public bool autoBake;

        private void Awake()
        {
            if (autoBake)
            {
                Build();
            }
        }

        public void Build()
        {
            if (spineObject == null) { spineObject = GetComponentInChildren<SkeletonAnimation>(); }
            if (skeletonGraphic == null) { skeletonGraphic = GetComponentInChildren<SkeletonGraphic>(); }

            if (spineObject != null)
            {
                combinedSkin = new Skin("combinedSkin");

                foreach (var item in skins_final)
                {
                    AddSkin_World(item);
                }

                spineObject.skeleton.SetSkin(combinedSkin);
                spineObject.skeleton.SetSlotsToSetupPose();
            }

            if (skeletonGraphic != null)
            {
                combinedSkin = new Skin("combinedSkin");

                foreach (var item in skins_final)
                {
                    AddSkin_Grapic(item);
                }

                skeletonGraphic.Skeleton.SetSkin(combinedSkin);
                skeletonGraphic.Skeleton.SetSlotsToSetupPose();
            }
        }

        private void AddSkin_World(string skinName)
        {
            if (string.IsNullOrEmpty(skinName) == false)
            {
                var skin = spineObject.skeleton.Data.FindSkin(skinName);
                combinedSkin.AddSkin(skin);
            }
        }

        private void AddSkin_Grapic(string skinName)
        {
            if (string.IsNullOrEmpty(skinName) == false)
            {
                var skin = skeletonGraphic.Skeleton.Data.FindSkin(skinName);
                combinedSkin.AddSkin(skin);
            }
        }
    }
}