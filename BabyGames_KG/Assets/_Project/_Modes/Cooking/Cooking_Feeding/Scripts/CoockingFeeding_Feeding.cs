using DG.Tweening;
using FX;
using Gameplay;
using Helpers;
using Spine.Unity;
using System.Threading.Tasks;
using DataClasses;
using Services;
using Sounds;
using UnityEngine;
using Zenject;

namespace Coocking
{
    public class CoockingFeeding_Feeding : CoockingFeeding_StateBase
    {
        public Transform holder;

        public SkeletonAnimation dog;

        public Dropable_Basic food;
        public Transform miska;
        public Transform miskaTransform;
        public PlacesHolder miskaPlaceForDog;
        public Sound _eatingSound;
        public SoundQueue _yummySound;

        [SerializeField] private HintHand_Drag _hintHand_Drag;

        [Inject] private ISoundPlayer _soundPlayer;

        public override async Task Enter()
        {
            await base.Enter();
            DiService.Inject(this);

            holder.DOMoveX(0, 1);

            miska.transform.DOMove(miskaTransform.position, 1);
            miska.transform.DOScale(miskaTransform.localScale, 1);

            food.targetPosition = miskaPlaceForDog;
            food.doPunchAnimation = false;
            food.boxCollider.enabled = true;
            food.onFinish += OnMiskaDropped;

            _hintHand_Drag = GetComponent<HintHand_Drag>();
            _hintHand_Drag?.SetIsActive(true);

            await _controller.ShowCurtain(false);
        }

        private async void OnMiskaDropped(Dropable_Basic basic)
        {
            food.onFinish -= OnMiskaDropped;
            food.transform.DOScale(0, 0.25f);

            dog.AnimationState.ClearTracks();
            dog.AnimationState.SetAnimation(0, "comeToDish", false);
            dog.AnimationState.AddAnimation(1, "eating", false, 0);
            _soundPlayer.TryPlay(_eatingSound);

            await AsyncHelper.DelayFloat(2f);

            _soundPlayer.TryPlay(_yummySound);
            _model.onFinish?.Invoke();
        }
    }
}