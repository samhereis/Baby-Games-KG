using _Project.Scripts.SO.Configs;
using CustomAttributes;
using DataClasses;
using DG.Tweening;
using Helpers;
using Loggers;
using Services;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Identifiers
{
    public class Panel_World : Panel_Base, ISelfValidator
    {
        public Vector3 hidePosition;
        public Vector3 showPosition;

        public float appearDuration = 0.5f;
        public float disAppearDuration = 0.25f;

        public bool reverse = false;
        public bool playSound = true;

        public Transform holder;

        [Header("Scale")]
        public float popDuration = 0.5f;
        public float popInterval = 0.5f;
        public Ease popEase = Ease.OutBack;

        [Header("Move")]
        public Vector3 itemsInitialPosition = Vector3.zero;
        public Vector3 panelPreAnimatePosition = Vector3.zero;

        [Fg_De] public List<PanelItem> currentPanelItems = new();

        [Inject] private SoundConfig_SO _soundConfig;
        [Inject] private ISoundPlayer _soundPlayer;

        public void Validate(SelfValidationResult result)
        {
            if (holder == null) { holder = transform; }
            if (showPosition == Vector3.zero) { showPosition.x = 7.25f; }
            if (hidePosition == Vector3.zero) { hidePosition.x = 25f; }
        }

        private void Awake()
        {
            holder.position = hidePosition;
            Validate(null);

            DiService.Inject(this);
        }

        [Button]
        public override async Task Appear()
        {
            await holder.DOLocalMove(showPosition, appearDuration).AsyncWaitForCompletion();
        }

        [Button]
        public override async Task Disppear()
        {
            await holder.DOLocalMove(hidePosition, disAppearDuration).AsyncWaitForCompletion();
        }

        public void PrepareForAnimation(PanelItem panelItem)
        {
            if (panelItem == null || panelItem.holder == null) return;

            panelItem.transform.SetParent(transform, true);

            var position = panelItem.transform.localPosition;
            position.x = 0;
            panelItem.transform.localPosition = position;

            if (mode == PanelMode.Scale)
            {
                panelItem.holder.localScale = Vector3.zero;
            }
            else
            {
                panelItem.holder.position = itemsInitialPosition;
            }
        }

        public void PrepareForAnimation(List<PanelItem> panelItmes)
        {
            currentPanelItems = panelItmes;
            panelItmes.ForEach(i => { PrepareForAnimation(i); });
        }

        public async Task AnimateItems_Async(List<PanelItem> panelItems)
        {
            currentPanelItems = panelItems;
            if (reverse) { currentPanelItems.Reverse(); }

            try
            {
                if (mode == PanelMode.Move)
                {
                    await transform.DOMove(panelPreAnimatePosition, disAppearDuration).SetEase(popEase).AsyncWaitForCompletion();
                }

                foreach (var item in panelItems)
                {
                    if (item == null || item.holder == null) { continue; }
                    item.gameObject.SetActive(true);

                    if (mode == PanelMode.Scale)
                    {
                        if (playSound) { _soundPlayer?.TryPlay(_soundConfig?.pannelSounds.Find(x => x.key == Panel_Sounds.Scale).value); }
                        item.holder.DOMove(item.initialPosition, 0);
                        item.holder.DOScale(item.holderScale, popDuration).SetEase(popEase);
                        await AsyncHelper.DelayFloat(popInterval);
                    }
                    else
                    {
                        if (playSound) { _soundPlayer?.TryPlay(_soundConfig?.pannelSounds.Find(x => x.key == Panel_Sounds.Move).value); }
                        item.holder.DOMove(item.initialPosition + panelPreAnimatePosition, popDuration);
                        await AsyncHelper.DelayFloat(popInterval);
                    }
                }

                if (mode == PanelMode.Move)
                {
                    await AsyncHelper.DelayFloat(popDuration);
                    await transform.DOMove(Vector3.zero, appearDuration).SetEase(popEase).AsyncWaitForCompletion();
                }
            } catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }
        }

        [Button]
        public async Task HideItems()
        {
            try
            {
                if (mode == PanelMode.Move)
                {
                    currentPanelItems.Reverse();
                    await transform.DOMove(panelPreAnimatePosition, disAppearDuration).SetEase(popEase).AsyncWaitForCompletion();
                }

                foreach (var item in currentPanelItems)
                {
                    if (item == null || item.holder == null) continue;

                    if (mode == PanelMode.Scale)
                    {
                        item.holder.DOScale(0, popDuration);
                        await AsyncHelper.DelayFloat(0.1f);
                    }
                    else
                    {
                        item.holder.DOMove(itemsInitialPosition, popDuration);
                        await AsyncHelper.DelayFloat(0.1f);
                    }
                }
            } catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }
        }

        [Button]
        public void PrepareForAnimation()
        {
            PrepareForAnimation(currentPanelItems);
        }

        [Button]
        public async void AnimateItems()
        {
            await AnimateItems_Async(currentPanelItems);
        }

        [Button]
        public async Task AnimateItems_Async()
        {
            await AnimateItems_Async(currentPanelItems);
        }
    }
}