using Spine.Unity;
using UnityEngine;

namespace CarTuning
{
    public class CarTuning_PartUI_WithAnimation : CarTuning_PartUI_Base
    {
        [SerializeField] private SkeletonGraphic _skeletonGraphic;
        [SerializeField] private RectTransform _holder;

        public void Initialize(CarPart_WithAnimation carPart)
        {
            _skeletonGraphic.skeletonDataAsset = carPart.skeletonAnimation.skeletonDataAsset;

            _skeletonGraphic.Initialize(true);
            _skeletonGraphic.LateUpdate();

            _carPart = carPart;
            Initialize();
        }
    }
}