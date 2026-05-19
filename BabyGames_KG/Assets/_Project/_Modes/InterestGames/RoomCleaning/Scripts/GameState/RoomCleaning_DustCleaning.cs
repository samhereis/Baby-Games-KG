using DataClasses.AssetReferences;
using DG.Tweening;
using FX;
using Helpers;
using InterestGames;
using Modes.Puzzle;
using Services;
using System.Threading.Tasks;
using UnityEngine;

namespace Coocking
{
    public class RoomCleaning_DustCleaning : RoomCleaning_StateBase
    {
        public float _percentageToCountAsDone = 75;

        public Drawable _drawable;
        public Drawable _drawable_left;
        public Drawable _drawable_middle;
        public Drawable _drawable_right;
        public Transform[] holder;
        public RoomCleaning_Platok platok;

        public ExternalAssetReference<Sprite> dirtSprite;
        public ExternalAssetReference<Sprite> dirtSprite_left;
        public ExternalAssetReference<Sprite> dirtSprite_middle;
        public ExternalAssetReference<Sprite> dirtSprite_right;

        public HintHand_DrawSimple hintHand;

        private bool ready_1;
        private bool ready_2;
        private bool ready_3;

        public override async Task Enter()
        {
            await FadeCurtain(1);
            await base.Enter();

            _drawable_left.gameObject.SetActive(true);
            _drawable_middle.gameObject.SetActive(true);
            _drawable_right.gameObject.SetActive(true);

            _drawable_left.Initialize(await dirtSprite_left.GetAssetAsync());
            _drawable_middle.Initialize(await dirtSprite_middle.GetAssetAsync());
            _drawable_right.Initialize(await dirtSprite_right.GetAssetAsync());

            foreach (var item in holder)
            {
                item.DOMoveX(0, 0.25f);
            }

            await FadeCurtain(0);

            hintHand.sources.Add(platok.transform);

            hintHand.SetIsActive(true);

            LazyUpdator_Service.instance?.AddToQueue(CheckForWin);
        }

        public override async Task Exit()
        {
            await base.Exit();

            LazyUpdator_Service.instance?.RemoveFromQueue(CheckForWin);
            foreach (var item in holder)
            {
                item.transform.DOMoveX(-25, 0);
            }

            hintHand.SetIsActive(false);
        }

        private async Task CheckForWin()
        {
            if (_isDone) return;

            MakeConfetti(_drawable_left, ready_1);
            MakeConfetti(_drawable_middle, ready_2);
            MakeConfetti(_drawable_right, ready_3);

            ready_1 = _drawable_left.percentageOfColoring > _percentageToCountAsDone;
            ready_2 = _drawable_middle.percentageOfColoring > _percentageToCountAsDone;
            ready_3 = _drawable_right.percentageOfColoring > _percentageToCountAsDone;

            if (ready_1 && ready_2 && ready_3)
            {
                LazyUpdator_Service.instance?.RemoveFromQueue(CheckForWin);
                NextState();
            }

            if (ready_1) { hintHand.targets.Remove(_drawable_left.transform); }
            if (ready_2) { hintHand.targets.Remove(_drawable_middle.transform); }
            if (ready_3) { hintHand.targets.Remove(_drawable_right.transform); }

            await AsyncHelper.DelayFloat(0.5f);
        }

        private async void NextState()
        {
            if (_isDone) return;
            _isDone = true;

            platok.transform.DOScale(0, 0.25f);

            await _drawable_left.Complete();
            await _drawable_middle.Complete();
            await _drawable_right.Complete();

            DiService.Get<StateEnd_FX>()?.DoFX();

            await AsyncHelper.DelayFloat(_delayForWin);
            _nextState = _nextStateOnWin;
        }

        private async void MakeConfetti(Drawable drawable, bool isReady)
        {
            if (isReady == false && drawable.percentageOfColoring > _percentageToCountAsDone)
            {
                var particle = await _model.content.mediumConfetti.GetRandom().InstantiateAsync();
                particle.transform.position = drawable.transform.position;
                particle.Play();
                Destroy(particle.gameObject, particle.main.duration);
            }
        }
    }
}