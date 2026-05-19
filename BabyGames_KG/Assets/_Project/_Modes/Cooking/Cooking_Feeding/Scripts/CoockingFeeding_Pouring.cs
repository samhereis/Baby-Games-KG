using DG.Tweening;
using FX;
using Sirenix.OdinInspector;
using Spine.Unity;
using System.Threading.Tasks;
using DataClasses;
using Services;
using Sounds;
using UnityEngine;
using Zenject;

namespace Coocking
{
    public class CoockingFeeding_Pouring : CoockingFeeding_StateBase
    {
        public Dropable_World miskaDropable;
        public Transform miskaTransform;

        public SkeletonAnimation form;
        public Transform formTransform;
        [SerializeField] private Sound _pourSound;

        [SerializeField] private HintHand_Drag _hintHand_Drag;
        
        [Inject] private ISoundPlayer  _soundPlayer;

        public override async Task Enter()
        {
            await base.Enter();
            DiService.Inject(this);

            miskaDropable.onFinish += OnFinish;
            miskaDropable.onPostAction += OnStartedPoring;
            miskaDropable.boxCollider.enabled = true;

            SyncWithTransform();

            _hintHand_Drag = GetComponent<HintHand_Drag>();
            _hintHand_Drag?.SetIsActive(true);
        }

        public override async Task Exit()
        {
            await base.Exit();

            miskaDropable.onFinish -= OnFinish;
            miskaDropable.onPostAction -= OnStartedPoring;
        }

        [Button]
        private void SyncWithTransform()
        {
            miskaDropable.transform.DOMove(miskaTransform.position, 1);
            miskaDropable.transform.DOScale(miskaTransform.localScale, 1);

            form.transform.DOMove(formTransform.position, 1);
            form.transform.DOScale(formTransform.localScale, 1);
        }

        private void OnStartedPoring(Dropable_World world)
        {
            _hintHand_Drag?.SetIsActive(false);

            miskaDropable.GetComponent<SkeletonAnimation>().timeScale = 1;
            form.AnimationName = "naliv";
            _soundPlayer.TryPlay(_pourSound);
        }

        private void OnFinish(Dropable_World world)
        {
            _nextState = _nextStateOnWin;
            miskaDropable.transform.DOMoveX(-25, 1);
        }
    }
}