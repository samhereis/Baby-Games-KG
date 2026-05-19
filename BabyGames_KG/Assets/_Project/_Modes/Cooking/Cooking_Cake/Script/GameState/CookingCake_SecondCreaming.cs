using DataClasses;
using DG.Tweening;
using FX;
using Gameplay;
using Helpers;
using Identifiers;
using Services;
using Sirenix.OdinInspector;
using Sounds;
using Spine.Unity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Coocking
{
    public class CookingCake_SecondCreaming : CoockingBurger_StateBase
    {
        public Transform[] toHide;

        public Transform creamsHolder;
        public SkeletonAnimation cakeAnimation;
        public CookingCake_FirstCreaming firstCreaming;

        [Space]
        public Transform oldPanel;
        public Panel_World panel_World;

        public List<DroppableGeneral_SimpleController> creams;
        public List<Dropable_General> items;

        [Space]
        [SerializeField] private HintHand_Drag _hintHand_Drag;

        [Space]
        [SpineSkin] public List<string> creamLowerSkins;
        [SpineSkin] public List<string> creamUpperSkins;

        [Space]
        [SerializeField] private Sound _creamSound;
        [Inject] private ISoundPlayer _soundPlayer;

        public int creamIndex { get; private set; }

        public override async Task Enter()
        {
            await base.Enter();

            items = creams.Select(x => x._dropable).ToList();

            foreach (var item in toHide)
            {
                item.DOMoveX(-25, 1);
            }

            oldPanel?.gameObject?.SetActive(false);
            panel_World.PrepareForAnimation(items.Select(x => x.GetComponent<PanelItem>()).ToList());
            await panel_World.Appear();
            panel_World.AnimateItems();

            foreach (var item in items)
            {
                item.onDropStart += OnCreamAboutToDrop;
                item.onDropEnd += OnCreamDroped;
            }

            DiService.Inject(this);
        }

        public override async Task Exit()
        {
            await base.Exit();
            creamsHolder.DOMoveX(25f, 1f);
        }

        private void OnCreamAboutToDrop(Dropable_General obj)
        {
            _model.requestCompleteButtonHide?.Invoke();

            _hintHand_Drag.SetIsActive(false);
            foreach (var item in items) { item._boxCollider.enabled = false; }

            cakeAnimation.AnimationName = "idle";
        }

        [Button]
        private async void OnCreamDroped(Dropable_General obj)
        {
            if (_isDone == true) { return; }

            creamIndex = items.IndexOf(obj);
            Build();

            var copy = Instantiate(obj, obj.transform.position + new Vector3(0, 0.25f, 0), obj.transform.rotation, obj.transform.parent);
            await obj.PlaceBackAsync();
            foreach (var item in items) { item._boxCollider.enabled = false; }

            await copy.transform.DOScale(1, 0.5f).AsyncWaitForCompletion();
            copy._boxCollider.enabled = false;

            cakeAnimation.loop = false;
            cakeAnimation.AnimationName = "action";
            copy.GetComponentInChildren<SkeletonAnimation>().loop = false;
            copy.GetComponentInChildren<SkeletonAnimation>().AnimationName = "action";
            _soundPlayer.TryPlay(_creamSound);

            await AsyncHelper.DelayFloat(4);

            copy.transform.DOScale(0, 0.25f);

            _model.requestCompleteButtonShow?.Invoke();
            _model.onCompleteButtonPressed -= Next;
            _model.onCompleteButtonPressed += Next;

            foreach (var item in items)
            {
                item._boxCollider.enabled = true;
            }
            _hintHand_Drag.SetIsActive(true);
        }

        [Button]
        public void Build()
        {
            AddSkin(creamUpperSkins[creamIndex]);
            AddSkin(creamLowerSkins[creamIndex]);

            cakeAnimation.skeleton.SetSkin(firstCreaming.combinedSkin);
            cakeAnimation.skeleton.SetSlotsToSetupPose();
        }

        private void AddSkin(string skinName)
        {
            if (string.IsNullOrEmpty(skinName) == false)
            {
                var skin = cakeAnimation.skeleton.Data.FindSkin(skinName);
                firstCreaming.combinedSkin.AddSkin(skin);
            }
        }

        private async void Next()
        {
            Build();

            _isDone = true;
            DiService.Get<StateEnd_FX>()?.DoFX();
            await panel_World.HideItems();
            _nextState = _nextStateOnWin;

            foreach (var item in items)
            {
                item.onDropStart -= OnCreamAboutToDrop;
                item.onDropEnd -= OnCreamDroped;

                item._boxCollider.enabled = false;
            }
        }
    }
}