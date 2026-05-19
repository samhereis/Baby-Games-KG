using DataClasses;
using DG.Tweening;
using FX;
using Gameplay;
using Helpers;
using Loggers;
using Services;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Sounds;
using Spine.Unity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Coocking
{
    public class CookingCake_Pour : CoockingBurger_StateBase
    {
        public Transform[] toHide;
        public Transform miskaHolder;

        [Header("Old Miska")]
        public Transform oldMiskaTransform;

        public DroppableGeneral_SimpleController oldMiskaDroppable;
        public Dropable_General oldMiska;
        public SkeletonAnimation oldMiskaAnimation;
        public Transform oldMiskaTarget;
        public List<KeyedObject<SkeletonPartsRenderer, string>> skeletonPartsSortingLayers = new();
        public List<KeyedObject<SkeletonPartsRenderer, int>> skeletonPartsSortingOrders = new();

        [Header("Old Miska")]
        public SkeletonAnimation newMiskaAnimation;

        [SerializeField] private HintHand_Drag _hintHand_Drag;

        [Space]
        [SerializeField] private Sound _nalivSound;
        [Inject] private ISoundPlayer _soundPlayer;

        public override async Task Disable()
        {
            await base.Disable();
            oldMiska.GetComponent<BoxCollider>().enabled = false;
        }

        public override async Task Enter()
        {
            await base.Enter();

            oldMiska = oldMiskaDroppable._dropable;

            try
            {
                foreach (var item in toHide)
                {
                    item.DOMoveX(-25, 1);
                }

                oldMiskaTransform.GetComponentsInChildren<SkeletonPartsRenderer>(true).ForEach(x => { x.MeshRenderer.sortingLayerName = "Front"; });

                oldMiskaTransform.SetParent(miskaHolder, true);

                miskaHolder.DOMoveX(0, 1);

                oldMiskaTransform.DOLocalMove(oldMiskaTarget.localPosition, 1);
                oldMiskaTransform.DOScale(oldMiskaTarget.localScale, 1).OnComplete(() =>
                {
                    oldMiska.GetComponent<BoxCollider>().enabled = true;
                });

                oldMiska.initialPosition = oldMiskaTarget.localPosition;

                oldMiska.hasDropped.AddListener(OnOldMiskaDropped);

                _hintHand_Drag = GetComponent<HintHand_Drag>();
                _hintHand_Drag.SetIsActive(true);

                DiService.Inject(this);

                SetOldMiskaAnimation("idle");
                miskaHolder.GetComponentInChildren<Collider>()?.SetEnabled(false);
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
                await _controller.ShowCurtain(false);
            }
        }

        protected async void SetOldMiskaAnimation(string animationName)
        {
            try
            {
                for (int i = 0; i < 3; i++)
                {
                    oldMiskaAnimation.timeScale = 1;
                    oldMiskaAnimation.AnimationState.ClearTracks();
                    oldMiskaAnimation.AnimationState.AddAnimation(0, animationName, false, 0);
                    await AsyncHelper.NextFrame();
                }
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }
        }

        public override async Task Exit()
        {
            await base.Exit();

            miskaHolder.GetComponentInChildren<Collider>().enabled = true;
        }

        [Button]
        private void SetSkeletonPartsSortings()
        {
            try
            {
                foreach (var item in skeletonPartsSortingLayers)
                {
                    item.key.MeshRenderer.sortingLayerName = item.value;
                }

                foreach (var item in skeletonPartsSortingOrders)
                {
                    item.key.MeshRenderer.sortingOrder = item.value;
                }
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }
        }

        private async void OnOldMiskaDropped(bool obj)
        {
            oldMiska.hasDropped.RemoveListener(OnOldMiskaDropped);

            oldMiska.GetComponent<BoxCollider>().enabled = false;

            oldMiskaAnimation.loop = false;
            newMiskaAnimation.loop = false;

            SetOldMiskaAnimation("action");

            newMiskaAnimation.AnimationState.ClearTracks();
            newMiskaAnimation.AnimationState.AddAnimation(0, "naliv", false, 0);
            newMiskaAnimation.AnimationState.AddAnimation(1, "idle", false, 0);

            _soundPlayer.TryPlay(_nalivSound);

            await AsyncHelper.DelayFloat(1.5f);
            oldMiskaTransform.DOLocalMove(oldMiska.initialPosition, 1);

            _nextState = _nextStateOnWin;
        }

        public override void ForceWin()
        {
            base.ForceWin();
            _nextState = _nextStateOnWin;
        }
    }
}