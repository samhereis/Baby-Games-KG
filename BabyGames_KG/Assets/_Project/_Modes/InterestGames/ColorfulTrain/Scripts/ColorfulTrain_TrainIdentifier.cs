using DataClasses;
using DG.Tweening;
using Helpers;
using Identifiers;
using Loggers;
using Services;
using Sounds;
using Spine.Unity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CustomAttributes;
using UnityEngine;
using Zenject;

namespace ColorfulTrain
{
    public class ColorfulTrain_TrainIdentifier : IdentifierBase
    {
        public SoundQueue letGoSound;
        public Sound start;
        public Sound go;
        public Sound end;

        [SerializeField] private AudioSource _soundPlayer;

        [Fg_De] public List<ColorfulTrain_Wheel> seats = new();

        [Inject] private ISoundPlayer _soundPlayer_Secondary;

        private void OnEnable()
        {
            DiService.Inject(this);

            _soundPlayer_Secondary?.TryPlay(end);
            _soundPlayer.DOFade(0, 0.5f);
        }

        public async Task Prepare()
        {
            if (letGoSound != null && await letGoSound.GetSound() is AudioClip audio)
            {
                _soundPlayer_Secondary.TryPlay(letGoSound);
                await AsyncHelper.DelayFloat(audio.length);
            }
        }

        public async Task Go()
        {
            _soundPlayer_Secondary?.TryPlay(start);

            _soundPlayer.clip = await go.GetSound();
            _soundPlayer.DOFade(1, 0.5f);
            _soundPlayer.Play();

            foreach (var colorfulTrainWheellessCar in seats)
            {
                colorfulTrainWheellessCar.canDrag = false;
                colorfulTrainWheellessCar.Restore();
            }

            try
            {
                Get<SkeletonAnimation>().AnimationState.ClearTracks();
                Get<SkeletonAnimation>().AnimationState.SetAnimation(0, "action", false);

                var duration = Get<SkeletonAnimation>().AnimationState.Tracks.Items[0].Animation.Duration;
                await AsyncHelper.DelayFloat(duration);
            } catch (Exception ex)
            {
                CustomLogger.instance.LogException(ex, "Error setting train animation");
            }

            foreach (var colorfulTrainWheellessCar in seats)
            {
                colorfulTrainWheellessCar.canDrag = true;
                colorfulTrainWheellessCar.Restore();
            }

            _soundPlayer_Secondary?.TryPlay(end);
            _soundPlayer.DOFade(0, 0.5f);
        }
    }
}