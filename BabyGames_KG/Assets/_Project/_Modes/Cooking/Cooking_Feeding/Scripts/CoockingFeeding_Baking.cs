using DataClasses;
using DG.Tweening;
using FX;
using Helpers;
using Loggers;
using Services;
using Sirenix.OdinInspector;
using Sounds;
using Spine.Unity;
using System;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Coocking
{
    public class CoockingFeeding_Baking : CoockingFeeding_StateBase
    {
        public Transform[] toHide;

        public Transform duhovkaHolder;

        [Header("New Miska")]
        public SkeletonAnimation newMiskaAnimation;
        public Dropable_Basic newMiska;
        public Transform newMiskaGetin_End;
        public Vector3 newMiskaScale = new Vector3(0.75f, 0.75f, 0.75f);

        [Header("Baked food")]
        public Dropable_Basic food_baked;

        [Header("Miska dog")]
        public Transform miskaDog_Food;
        public Transform miskaDog;
        public Transform miskaDog_Transform;

        [Header("Duhovka")]
        public SkeletonAnimation duhovkaAnimation;
        public SkeletonPartsRenderer duhovkaDoor;
        public SkeletonPartsRenderer[] duhovkaCorpus;

        [Header("Sounds")]
        public SoundAdvanced duhovkaSound;

        [Space]
        public float openDuration = 1;
        public float closeDuration = 1;
        public float bakingDuration = 1;
        public float getInDuration = 0.25f;
        public float gInDelay = 0.1f;
        public float goOutDelay = 1;

        [Space]
        [SerializeField] private HintHand_Drag _hintHand_Drag;

        [Space]
        [SerializeField] private Sound _bakingSound;
        [Inject] private ISoundPlayer _soundPlayer;

        public override async Task Enable()
        {
            DiService.Inject(this);

            await base.Enable();

            await _controller.ShowCurtain(true);
            await _controller.ChangeBackground(backgroundIndex);

            foreach (var item in newMiska.GetComponentsInChildren<SkeletonPartsRenderer>())
            {
                item.MeshRenderer.sortingLayerName = "Front";
            }
            newMiska.transform.DOScale(newMiskaScale, 1);
            newMiska.onFinish += OnNewMiskaDropedIntoDuhovka;

            await OpenDove();

            await _controller.ShowCurtain(false);

            _hintHand_Drag = GetComponent<HintHand_Drag>();
            _hintHand_Drag?.SetIsActive(true);

            _hintHand_Drag.objects.Clear();
            _hintHand_Drag.targets.Clear();
            _hintHand_Drag.objects.Add(newMiska.transform);
            _hintHand_Drag.targets.Add(newMiskaGetin_End);
        }

        public override async Task Exit()
        {
            await base.Exit();
        }

        [Button]
        private async Task OpenDove()
        {
            try
            {
                foreach (var item in toHide)
                {
                    item.DOMoveX(-25, 1);
                }

                duhovkaHolder.DOMove(Vector3.zero, 0.1f);

                duhovkaAnimation.loop = false;
                duhovkaAnimation.timeScale = 1;
                duhovkaAnimation.AnimationName = "action";
                await AsyncHelper.DelayFloat(openDuration);
                duhovkaAnimation.timeScale = 0;
            } catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
                await _controller.ShowCurtain(false);
            }
        }

        private async void OnNewMiskaDropedIntoDuhovka(Dropable_Basic obj)
        {
            DiService.Get<StateEnd_FX>()?.DoFX();
            duhovkaDoor.MeshRenderer.sortingLayerName = "Front";

            newMiska.onFinish -= OnNewMiskaDropedIntoDuhovka;
            newMiska.GetComponent<BoxCollider>().enabled = false;

            newMiska.transform.DOMove(newMiskaGetin_End.position, 1f);
            await AsyncHelper.DelayFloat(gInDelay);

            _soundPlayer.TryPlay(duhovkaSound);
            duhovkaAnimation.timeScale = 1;
            newMiska.transform.SetParent(duhovkaHolder);

            await AsyncHelper.DelayFloat(gInDelay);
            await newMiska.transform.DOScale(newMiskaGetin_End.localScale, getInDuration).OnComplete(() =>
            {
                foreach (var item in newMiska.GetComponentsInChildren<SkeletonPartsRenderer>())
                {
                    item.MeshRenderer.sortingLayerName = "Default";
                }

                foreach (var item in duhovkaCorpus)
                {
                    item.MeshRenderer.sortingLayerName = "Front";
                }
            }).AsyncWaitForCompletion();

            await AsyncHelper.DelayFloat(closeDuration);
            duhovkaAnimation.timeScale = 0;

            newMiskaAnimation.AnimationState.ClearTracks();
            newMiskaAnimation.AnimationState.AddAnimation(0, "zapek", false, 0);
            _soundPlayer.TryPlay(_bakingSound);
            await AsyncHelper.DelayFloat(bakingDuration);

            duhovkaAnimation.timeScale = 1;
            await AsyncHelper.DelayFloat(goOutDelay);

            food_baked.doPunchAnimation = false;
            food_baked.boxCollider.enabled = true;
            food_baked.onFinish += onFoodSet;
            food_baked.isDropped.AddListener(OnFoodDroped);
            food_baked.GetComponent<SkeletonPartsRenderer>().MeshRenderer.sortingOrder = 100;

            miskaDog_Food.transform.localScale = Vector3.zero;

            miskaDog.DOMove(miskaDog_Transform.position, 1);
            miskaDog.DOScale(miskaDog_Transform.localScale, 1);

            foreach (var item in newMiska.GetComponentsInChildren<SkeletonPartsRenderer>())
            {
                item.MeshRenderer.sortingLayerName = "Front";
            }

            _hintHand_Drag.objects.Clear();
            _hintHand_Drag.targets.Clear();
            _hintHand_Drag.objects.Add(food_baked.transform);
            _hintHand_Drag.targets.Add(miskaDog.transform);
        }

        private void OnFoodDroped(bool obj)
        {
            food_baked.transform.DOScale(0, 0.25f);
            miskaDog_Food.transform.DOScale(1, 0.25f);
        }

        private async void onFoodSet(Dropable_Basic basic)
        {
            await AsyncHelper.DelayFloat(1);
            await _controller.ShowCurtain(true);

            duhovkaHolder.DOMoveX(-25, 1);

            food_baked.boxCollider.enabled = true;

            _nextState = _nextStateOnWin;

            _hintHand_Drag?.SetIsActive(false);
        }

        public override void ForceWin()
        {
            base.ForceWin();
            _nextState = _nextStateOnWin;
        }
    }
}