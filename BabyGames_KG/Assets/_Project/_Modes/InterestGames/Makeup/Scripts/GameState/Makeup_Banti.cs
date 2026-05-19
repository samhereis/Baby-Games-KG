using Coocking;
using DataClasses;
using DG.Tweening;
using Helpers;
using Loggers;
using Services;
using Spine.Unity;
using System;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace InterestGames
{
    public class Makeup_Banti : Makeup_MakeupItemsBase
    {
        public Makeup_Hair hair;
        public SkeletonAnimation girl;

        [Space]
        public Sounds.Sound animationSound;
        public float animationSoundDelay;
        public float winDelay;

        [Inject] private ISoundPlayer _soundPlayer;

        public override Task Enable()
        {
            DiService.Inject(this);
            return base.Enable();
        }

        protected override void OnDrop(Dropable_Basic basic)
        {
            var index = droppables.IndexOf(basic) + (controller.hairIndex * 3);
            controller.banti[9] = controller.banti[index];
            basic.DoReset();
            TryWin();
        }

        protected override void TryWin()
        {
            controller.Build();

            _model.completeButtonShowRequested?.Invoke();

            _model.onCompleteButtonClicked -= OnCompleteButtonClicked;
            _model.onCompleteButtonClicked += OnCompleteButtonClicked;
        }

        protected override async void OnCompleteButtonClicked()
        {
            base.OnCompleteButtonClicked();

            girl.AnimationState.ClearTracks();
            girl.AnimationState.SetAnimation(0, "fin_dvij", false);

            try
            {
                if (animationSound != null && _soundPlayer != null)
                {
                    await AsyncHelper.DelayFloat(animationSoundDelay);
                    _soundPlayer.TryPlay(animationSound);
                }

                await AsyncHelper.DelayFloat(winDelay);
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }

            _model.hasWon?.ChangeValue(true);

            foreach (var item in droppables)
            {
                item.boxCollider.enabled = false;
                item.transform.DOScale(0, 0.25f);
            }
        }
    }
}