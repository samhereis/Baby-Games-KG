using DataClasses;
using Helpers;
using Identifiers;
using Services;
using Sounds;
using Spine.Unity;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace WhoLivesWhere
{
    public class WLW_TraktorIdentifier : IdentifierBase
    {
        public List<WLW_TraktorSeat> seats = new();

        public SoundQueue letGoSound;
        public Sound goSound;

        [Inject] public ISoundPlayer soundPlayer;

        private void Awake()
        {
            DiService.Inject(this);
        }

        private void OnDestroy()
        {
            soundPlayer.Stop(goSound);
        }

        public async Task Prepare()
        {
            if (letGoSound != null && await letGoSound.GetSound() is AudioClip audio)
            {
                soundPlayer.TryPlay(letGoSound);
                await AsyncHelper.DelayFloat(audio.length);
            }
        }

        public void Go()
        {
            soundPlayer.TryPlay(goSound);

            Get<SkeletonAnimation>().AnimationState.ClearTracks();
            Get<SkeletonAnimation>().AnimationState.SetAnimation(0, "action", false);
        }
    }
}