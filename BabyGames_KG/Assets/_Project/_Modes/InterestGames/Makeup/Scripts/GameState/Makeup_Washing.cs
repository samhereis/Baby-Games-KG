using DataClasses;
using DG.Tweening;
using FX;
using Services;
using Sounds;
using Spine;
using Spine.Unity;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace InterestGames
{
    public class Makeup_Washing : Makeup_StateBase
    {
        public Makeup_Washing_Soap soap;
        public Makeup_Washing_Water water;
        public Makeup_Washing_Platok platok;

        public Skin combinedSkin;
        public SkeletonAnimation girl;
        public Sound finalAnimation;

        public HintHand_DrawSimple hintHand_DrawSimple;

        [Inject] private ISoundPlayer _soundPlayer;

        public override async Task Enter()
        {
            await base.Enter();

            soap.boxCollider.enabled = true;
            water.boxCollider.enabled = false;
            platok.boxCollider.enabled = false;

            soap.GetComponent<SpriteRenderer>().DOColor(Color.white, 1f);
            water.GetComponent<SpriteRenderer>().DOColor(Color.gray, 1f);
            platok.GetComponent<SpriteRenderer>().DOColor(Color.gray, 1f);

            soap.Initialize();

            soap.onFinish += OnSoapDone;
            water.onFinish += OnWaterDone;
            platok.onFinish += OnPlatokDone;

            hintHand_DrawSimple.sources.Clear();
            hintHand_DrawSimple.targets.Clear();
            hintHand_DrawSimple.sources.Add(soap.transform);
            hintHand_DrawSimple.targets.Add(hintHand_DrawSimple.transform);

            hintHand_DrawSimple.SetIsActive(true);

            DiService.Inject(this);
        }

        public override async Task Exit()
        {
            await base.Exit();

            soap.onFinish -= OnSoapDone;
            water.onFinish -= OnWaterDone;
            platok.onFinish -= OnPlatokDone;
        }

        public override void Tick()
        {
            base.Tick();

            if (Keyboard.current.spaceKey.wasReleasedThisFrame)
            {
                _nextState = _nextStateOnWin;
            }
        }

        private void OnSoapDone()
        {
            soap.boxCollider.enabled = false;
            water.boxCollider.enabled = true;
            platok.boxCollider.enabled = false;

            soap.GetComponent<SpriteRenderer>().DOColor(Color.gray, 1f);
            water.GetComponent<SpriteRenderer>().DOColor(Color.white, 1f);
            platok.GetComponent<SpriteRenderer>().DOColor(Color.gray, 1f);

            hintHand_DrawSimple.sources.Clear();
            hintHand_DrawSimple.targets.Clear();
            hintHand_DrawSimple.sources.Add(water.transform);
            hintHand_DrawSimple.targets.Add(hintHand_DrawSimple.transform);

            water.Initialize();
        }

        private void OnWaterDone()
        {
            soap.boxCollider.enabled = false;
            water.boxCollider.enabled = false;
            platok.boxCollider.enabled = true;

            soap.GetComponent<SpriteRenderer>().DOColor(Color.gray, 1f);
            water.GetComponent<SpriteRenderer>().DOColor(Color.gray, 1f);
            platok.GetComponent<SpriteRenderer>().DOColor(Color.white, 1f);

            hintHand_DrawSimple.sources.Clear();
            hintHand_DrawSimple.targets.Clear();
            hintHand_DrawSimple.sources.Add(platok.transform);
            hintHand_DrawSimple.targets.Add(hintHand_DrawSimple.transform);

            platok.Initialize();
        }

        private void OnPlatokDone()
        {
            soap.boxCollider.enabled = false;
            water.boxCollider.enabled = false;
            platok.boxCollider.enabled = false;

            hintHand_DrawSimple.sources.Clear();
            hintHand_DrawSimple.targets.Clear();
            hintHand_DrawSimple.SetIsActive(false);

            girl.AnimationState.ClearTracks();
            girl.AnimationState.AddAnimation(1, "svet_bant_hair_1", false, 1);

            _nextState = _nextStateOnWin;

            _soundPlayer?.TryPlay(finalAnimation);
        }
    }
}