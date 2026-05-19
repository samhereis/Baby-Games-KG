using Sounds;
using Spine.Unity;
using UnityEngine;

namespace CarTuning
{
    public class Princess_Dress_WithAnimation : Princess_DressBase
    {
        public SkeletonAnimation skeletonAnimation;
        public Sound sound = new();
        public Vector2 offset;
    }
}