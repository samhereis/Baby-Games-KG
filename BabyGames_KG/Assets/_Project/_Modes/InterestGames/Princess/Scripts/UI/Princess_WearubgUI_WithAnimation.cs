using DG.Tweening;
using Spine.Unity;
using UnityEngine;

namespace CarTuning
{
    public class Princess_WearubgUI_WithAnimation : Princess_WearubgUI_Base
    {
        [SerializeField] private SkeletonGraphic _skeletonGraphic;

        public void Initialize(Princess_Dress_WithAnimation carPart)
        {
            _skeletonGraphic.skeletonDataAsset = carPart.skeletonAnimation.skeletonDataAsset;

            _skeletonGraphic.Initialize(true);
            _skeletonGraphic.LateUpdate();

            _skeletonGraphic.rectTransform.DOAnchorPos(_skeletonGraphic.rectTransform.anchoredPosition + carPart.offset, 0);

            _wearing = carPart;
        }
    }
}