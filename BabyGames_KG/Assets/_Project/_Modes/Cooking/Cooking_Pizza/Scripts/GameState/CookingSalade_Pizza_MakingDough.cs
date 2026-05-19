using _Project._Modes.Orchestra.Scripts.SO;
using _Project.Scripts.Sound;
using CoockingSalade;
using DG.Tweening;
using FX;
using Helpers;
using Loggers;
using Services;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Coocking
{
    public class CookingSalade_Pizza_MakingDough : CoockingSalade_StateBase
    {
        [SerializeField] private List<Dropable_World> _ingredients = new();
        [SerializeField] private List<Transform> _ingredients_ToHide = new();
        public CookingCake_Butter butter;

        [SerializeField] private CookingSalade_Mixer _mixer;

        [SerializeField] private HintHand_Drag _hintHand_Drag;

        [Inject] private Orchestra_Data _orchestra_Data;

        public Transform miska;
        public Transform miskaTarget;

        private bool _isDonePlacing = false;

        protected override void Awake()
        {
            base.Awake();
            _mixer.transform.localScale = Vector3.zero;

            DiService.Inject(this);
        }

        public override async Task Enter()
        {
            await controller.ChangeBackground(backgroundIndex);
            await controller.ShowCurtain(false);

            await base.Enter();

            foreach (var item in _ingredients)
            {
                item.isDropped.RemoveListener(OnAnItemDroping);
                item.isDropped.AddListener(OnAnItemDroping);
                item.onFinish -= OnAnItemDroped;
                item.onFinish += OnAnItemDroped;

                item.objectJuicer.StartJamming();
            }

            butter?.hasDropped.AddListener(OnButterDroped);
            butter.item.objectJuicer.StartJamming();

            _hintHand_Drag = GetComponent<HintHand_Drag>();
            _hintHand_Drag.objects.AddRange(_ingredients.Select(x => x.transform));
            _hintHand_Drag.targets.AddRange(_ingredients.Select(x => x.targetPosition));
            _hintHand_Drag?.SetIsActive(true);
        }

        public override async Task Exit()
        {
            await base.Exit();

            foreach (var item in _ingredients)
            {
                item.isDropped.RemoveListener(OnAnItemDroping);
                item.onFinish -= OnAnItemDroped;

                item.objectJuicer.StopJamming();
            }

            butter?.hasDropped.RemoveListener(OnButterDroped);
        }

        private async void OnButterDroped(bool obj)
        {
            _hintHand_Drag?.SetIsActive(false);
            butter.item.objectJuicer.initialScale = Vector3.zero;
            butter.item.objectJuicer.returnSpeed = 0.1f;
            butter.item.objectJuicer.StopJamming();

            if (butter != null)
            {
                butter.GetComponentInChildren<BoxCollider>(true).enabled = false;
            }

            foreach (var item in butter?.parts)
            {
                item.SetParent(transform);
            }

            butter?.transform.DOScale(0, 0.1f).SetEase(Ease.InBack);

            await AsyncHelper.DelayFloat(1f);
            TryWin();
        }

        private void OnAnItemDroping(bool obj)
        {
#if UNITY_EDITOR
            return;
#endif

            foreach (var item in _ingredients)
            {
                item.boxCollider.enabled = false;
            }
        }

        private async void OnAnItemDroped(Dropable_World obj)
        {
            _hintHand_Drag?.SetIsActive(false);
            obj.objectJuicer.initialScale = Vector3.zero;
            obj.objectJuicer.returnSpeed = 0.1f;
            obj.objectJuicer.StopJamming();

            if (obj.name.Contains("egg"))
            {
                await AsyncHelper.DelayFloat(0.5f);
                obj.transform.DOScale(0, 0);
            }
            else
            {
                obj.transform.DOScale(0, 0.1f).SetEase(Ease.InBack);
            }

            TryWin();

#if UNITY_EDITOR
            return;
#endif

            foreach (var item in _ingredients)
            {
                if (item.isDropped.value == false)
                {
                    item.boxCollider.enabled = true;
                }
            }
        }

        [Button]
        private async void TryWin()
        {
            if (_isDonePlacing) { return; }

            _isDonePlacing = _ingredients.TrueForAll(x => x.isDropped.value == true);
            if (_isDonePlacing == true && butter != null) { _isDonePlacing = butter?.hasDropped.value == true; }
            if (_isDonePlacing)
            {
                await AsyncHelper.DelayFloat(2f);

                foreach (var item in _ingredients)
                {
                    item.isDropped.RemoveListener(OnAnItemDroping);
                    item.onFinish -= OnAnItemDroped;
                }

                butter?.hasDropped.RemoveListener(OnButterDroped);

                _mixer.Show();

                try
                {
                    Sound_FX.Play_Static(Sound_Effect.Success);
                    var confetti = await _orchestra_Data.transition.InstantiateAsync();
                    confetti.transform.position = Vector3.zero;
                    confetti.Play();
                } catch (Exception ex)
                {
                    CustomLogger.instance?.LogException(ex);
                }

                miska.transform.DOMove(miskaTarget.transform.position, 1);
                miska.transform.DORotate(miskaTarget.transform.eulerAngles, 1);
                miska.transform.DOScale(miskaTarget.transform.localScale, 1);

                _mixer.isReady.AddListener(OnMixerDone);
            }
        }

        private async void OnMixerDone(bool obj)
        {
            _mixer.isReady.RemoveListener(OnMixerDone);

            await controller.ShowCurtain(false);

            await AsyncHelper.DelayFloat(1f);

            foreach (var item in _ingredients_ToHide)
            {
                item.DOScale(0, 0.25f);
            }

            DiService.Get<StateEnd_FX>()?.DoFX();
            _nextState = _nextStateOnWin;
        }

        public override void ForceWin()
        {
            base.ForceWin();
            _nextState = _nextStateOnWin;
        }
    }
}