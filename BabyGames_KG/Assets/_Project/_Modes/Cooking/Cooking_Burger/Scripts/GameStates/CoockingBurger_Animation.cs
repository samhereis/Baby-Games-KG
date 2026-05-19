using DataClasses;
using DG.Tweening;
using Helpers;
using Services;
using Sirenix.OdinInspector;
using Sounds;
using Spine;
using Spine.Unity;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Project.Scripts.Sound;
using UnityEngine;
using Zenject;

namespace Coocking
{
    public class CoockingBurger_Animation : CoockingBurger_StateBase
    {
        public SpriteRenderer curtain;
        public SpriteRenderer backgroundToFade;
        public Transform buildingBurger;
        public SkeletonAnimation burger;
        public Transform plate;
        public Transform podnos;

        [Space]
        public CoockingBurger_Bulki coockingBurger;
        public CoockingBurger_Sauce coockingSauce;

        [SpineSkin] public List<string> burgers = new List<string>();
        [SpineSkin] public List<string> sauces = new List<string>();
        public SkeletonAnimation spineObject;
        public Skin combinedSkin;

        public List<string> skins_final;

        [Space]
        [FoldoutGroup("Sound"), SerializeField] private Sound _burgerSound;
        [FoldoutGroup("Sound"), SerializeField] private SoundQueue_Advanced endSound;
        [Inject] private ISoundPlayer _soundPlayer;

        public override async Task Disable()
        {
            await base.Disable();

            transform.DOKill();
            transform.DOMoveX(25, 0);
        }

        public override async Task Enter()
        {
            DiService.Inject(this);

            backgroundToFade.DOFade(0, 0.25f);
            plate.DOMoveX(-25, 0.25f);
            podnos.DOMoveY(25, 1);
            await curtain.DOFade(1, 0.25f).AsyncWaitForCompletion();

            await AsyncHelper.DelayFloat(0.5f);
            await base.Enter();

            Build();

            buildingBurger.gameObject.SetActive(false);
            await curtain.DOFade(0, 0.25f).AsyncWaitForCompletion();

            transform.DOKill();
            await transform.DOMoveX(0, 1).AsyncWaitForCompletion();

            burger.loop = false;
            burger.AnimationName = "action";

            if (_soundPlayer != null) { _soundPlayer.TryPlay(_burgerSound); }
            await AsyncHelper.DelayFloat(2f);
            Sound_FX.PlayAsync_Static  (endSound);
            await AsyncHelper.DelayFloat(1f);
            Win();
        }

        private void Win()
        {
            _model.onFinish?.Invoke();
        }

        [Button]
        public void Build()
        {
            if (spineObject == null) { spineObject = GetComponentInChildren<SkeletonAnimation>(); }

            combinedSkin = new Skin("combinedSkin");

            AddSkin(burgers[coockingBurger.bulkaIndex]);
            AddSkin(sauces[coockingSauce.sauceIndex]);

            spineObject.skeleton.SetSkin(combinedSkin);
            spineObject.skeleton.SetSlotsToSetupPose();
        }

        private void AddSkin(string skinName)
        {
            if (string.IsNullOrEmpty(skinName) == false)
            {
                var skin = spineObject.skeleton.Data.FindSkin(skinName);
                combinedSkin.AddSkin(skin);
            }
        }
    }
}