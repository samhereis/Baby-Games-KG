using Spine;
using Spine.Unity;
using System.Collections.Generic;
using UnityEngine;

namespace InterestGames
{
    public class SkinCombiner : MonoBehaviour
    {
        [SpineSkin] public string skiBody = "Telo_pijama";
        [SpineSkin] public string skiTufli = "pijama_tufli";
        [SpineSkin] public string skinDiadema;
        [SpineSkin] public string skinJewilery;

        public SkeletonAnimation spineObject;
        public Skin combinedSkin;

        public List<string> skins_final;

        public void Build()
        {
            if (spineObject == null) { spineObject = GetComponentInChildren<SkeletonAnimation>(); }

            combinedSkin = new Skin("combinedSkin");

            AddSkin(skiBody);
            AddSkin(skiTufli);
            AddSkin(skinDiadema);
            AddSkin(skinJewilery);

            spineObject.skeleton.SetSkin(combinedSkin);
            spineObject.skeleton.SetSlotsToSetupPose();
        }

        private void AddSkin(string skinName)
        {
            if (string.IsNullOrEmpty(skinName) == false)
            {
                var skin = spineObject.skeleton.Data.FindSkin(skinName);
                combinedSkin.AddSkin(skin);
            }
        }
    }
}