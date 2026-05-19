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
    public class Panel_UI : Panel_Base
    {
        [Header("Panel")]
        public RectTransform holder;
        public Vector3 hidePosition;
        public Vector3 showPosition;

        [Header("Panel")]
        public float appearTime => 1f;
        public float disappearTime => 1f;

        [Header("Scale")]
        public float popDuration = 0.5f;
        public float popInterval = 0.1f;
        public Ease popEase = Ease.OutBack;

        [Header("Move")]
        public Vector3 itemsInitialPosition = Vector3.zero;

        [Inject] private SoundConfig_SO _soundConfig;
        [Inject] private ISoundPlayer _soundPlayer;

        [SerializeField, Fg_De] private List<PanelItem_UI> _currentPanelItems = new();

        private void Awake()
        {
            DiService.Inject(this);
        }

        [Button]
        public override async Task Appear()
        {
            await holder.DOAnchorPos(showPosition, appearTime).AsyncWaitForCompletion();
        }

        [Button]
        public override async Task Disppear()
        {
            await holder.DOAnchorPos(hidePosition, disappearTime).AsyncWaitForCompletion();
        }

        public void PrepareForAnimation(PanelItem_UI panelItem)
        {
            if (mode == PanelMode.Scale)
            {
                panelItem.holder.localScale = Vector3.zero;
            }
            else
            {
                panelItem.holder.anchoredPosition = itemsInitialPosition;
            }
        }

        public void PrepareForAnimation(List<PanelItem_UI> panelItmes)
        {
            _currentPanelItems = panelItmes;
            panelItmes.ForEach(i => { PrepareForAnimation(i); });
        }

        public async Task AnimateItems(List<PanelItem_UI> panelItems)
        {
            _currentPanelItems = panelItems;

            try
            {
                foreach (var item in panelItems)
                {
                    if (item == null || item.holder == null) continue;

                    if (mode == PanelMode.Scale)
                    {
                        _soundPlayer?.TryPlay(_soundConfig?.pannelSounds.Find(x => x.key == Panel_Sounds.Scale).value);
                        item.holder.DOScale(item.holderScale, popDuration).SetEase(popEase);
                        await AsyncHelper.DelayFloat(popInterval);
                    }
                    else
                    {
                        _soundPlayer?.TryPlay(_soundConfig?.pannelSounds.Find(x => x.key == Panel_Sounds.Move).value);
                        item.holder.DOAnchorPos(Vector3.zero, popDuration).SetEase(popEase);
                        await AsyncHelper.DelayFloat(popInterval);
                    }
                }
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }
        }

        public async Task HideItems()
        {
            try
            {
                foreach (var item in _currentPanelItems)
                {
                    if (item == null || item.holder == null) continue;

                    if (mode == PanelMode.Scale)
                    {
                        item.holder.DOScale(0, popDuration).SetEase(popEase);
                        await AsyncHelper.DelayFloat(0.1f);
                    }
                    else
                    {
                        item.holder.DOAnchorPos(itemsInitialPosition, popDuration).SetEase(popEase);
                        await AsyncHelper.DelayFloat(0.1f);
                    }
                }
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }
        }

        [Button]
        public void PrepareForAnimation()
        {
            PrepareForAnimation(_currentPanelItems);
        }

        [Button]
        public async void AnimateItems()
        {
            await AnimateItems(_currentPanelItems);
        }
    }
}
