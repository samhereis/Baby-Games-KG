using DataClasses;
using DG.Tweening;
using FX;
using Gameplay;
using Helpers;
using Loggers;
using Services;
using Sounds;
using Spine.Unity;
using System;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Coocking
{
    public class CookingCake_Baking : CoockingBurger_StateBase
    {
        public Transform[] toHide;

        public Transform duhovkaHolder;

        [Header("New Miska")]
        public SkeletonAnimation newMiskaAnimation;

        public DroppableGeneral_SimpleController newMiskaDropable;
        public Dropable_General newMiska;

        public Transform newMiskaGetin_End;

        [Header("Duhovka")]
        public SkeletonAnimation duhovkaAnimation;
        public SkeletonPartsRenderer duhovkaPlatform;

        [Header("Sounds")]
        public SoundAdvanced duhovkaSound;

        public float openDuration = 1;
        public float closeDuration = 1;
        public float bakingDuration = 1;

        [Space]
        public float duhovkaAnimationTimeScaleWhileGettingIn = 0.5f;
        public float getInDuration = 0.25f;
        public float gInDelay = 0.1f;

        [Space]
        public float duhovkaAnimationTimeScaleWhileGettingOut = 0.5f;
        public float getOutDuration = 0.25f;
        public float goOutDelay = 1;

        [Space]
        [SerializeField] private HintHand_Drag _hintHand_Drag;

        [Space]
        [SerializeField] private Sound _bakingSound;
        [Inject] private ISoundPlayer _soundPlayer;

        public override async Task Enter()
        {
            await base.Enter();

            newMiska = newMiskaDropable._dropable;

            await _controller.ShowCurtain(true);
            await _controller.ChangeBackground(_backgroundIndex);

            try
            {
                foreach (var item in toHide)
                {
                    item.DOMoveX(-25, 1);
                }

                duhovkaHolder.DOMoveX(-2.5f, 0);

                duhovkaAnimation.loop = false;
                duhovkaAnimation.AnimationName = "action";
                await AsyncHelper.DelayFloat(openDuration);
                duhovkaAnimation.timeScale = 0;

                newMiska.hasDropped.AddListener(OnNewMiskaDropedIntoDuhovka);

                _hintHand_Drag = GetComponent<HintHand_Drag>();
                _hintHand_Drag.SetIsActive(true);

                DiService.Inject(this);
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
                await _controller.ShowCurtain(false);
            }

            await _controller.ShowCurtain(false);
        }

        public override async Task Exit()
        {
            await base.Exit();
        }

        private async void OnNewMiskaDropedIntoDuhovka(bool obj)
        {
            DiService.Get<StateEnd_FX>()?.DoFX();

            _hintHand_Drag.SetIsActive(false);

            newMiska.hasDropped.RemoveListener(OnNewMiskaDropedIntoDuhovka);
            newMiska.GetComponent<BoxCollider>().enabled = false;

            {
                newMiska.transform.DOMove(newMiskaGetin_End.position, 1f);
                await AsyncHelper.DelayFloat(gInDelay);
            }

            {
                _soundPlayer.TryPlay(duhovkaSound);
                duhovkaAnimation.timeScale = duhovkaAnimationTimeScaleWhileGettingIn;
                newMiska.transform.SetParent(duhovkaHolder);

                await AsyncHelper.DelayFloat(gInDelay);
                newMiska.transform.DOScale(newMiskaGetin_End.localScale, getInDuration);

                await AsyncHelper.DelayFloat(getInDuration);
                duhovkaAnimation.timeScale = 1;
                duhovkaPlatform.MeshRenderer.sortingLayerName = "Front";

                await AsyncHelper.DelayFloat(closeDuration);
            }

            {
                duhovkaAnimation.timeScale = 0;
                newMiskaAnimation.AnimationState.ClearTracks();
                newMiskaAnimation.AnimationState.AddAnimation(0, "pechka", false, 0);
                _soundPlayer.TryPlay(_bakingSound);
                duhovkaAnimation.timeScale = 0;
                await AsyncHelper.DelayFloat(bakingDuration);
            }

            {
                duhovkaAnimation.timeScale = duhovkaAnimationTimeScaleWhileGettingOut;
                await AsyncHelper.DelayFloat(goOutDelay);

                newMiska.transform.DOScale(Vector3.one, getOutDuration);
                newMiska.transform.DOMoveY(newMiska.transform.position.y - 0.5f, getOutDuration);
                duhovkaPlatform.MeshRenderer.sortingLayerName = "Background";
            }

            await AsyncHelper.DelayFloat(1);
            await _controller.ShowCurtain(true);
            duhovkaHolder.DOMoveX(-25, 0);

            _nextState = _nextStateOnWin;
        }

        public override void ForceWin()
        {
            base.ForceWin();
            _nextState = _nextStateOnWin;
        }
    }
}