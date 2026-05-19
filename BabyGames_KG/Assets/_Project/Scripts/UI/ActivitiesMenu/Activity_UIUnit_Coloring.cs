using DataClasses;
using DG.Tweening;
using GameState;
using Helpers;
using Services;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Identifiers.UI
{
    public class Activity_UIUnit_Coloring : Activity_UIUnit_Standart
    {
        [Inject] private GameSaveService _saveService;

        private static List<Activity> _deletedOnIsNotCashed = new List<Activity>();

        public override async void Initialize(MainMenu_GameState_Model model, Activity activity)
        {
            DiService.Inject(this);

            activityData = activity;
            _model = model;

            if (_model.activity_Identifier_Provider.IsCashed(activityData))
            {
                if (await _model.activity_Identifier_Provider.HasUpdate(activityData))
                {
                    await _model.contentDeliveryService.ClearCacheAsync(activityData.GetName(), activityData.version, activityData.GetUrl());
                    _model.gameSaveService.DeleteDrawings(activity.acitvityCategory, activity.GetName());
                    _model.gameSaveService.DeleteBackgroundDrawings(activity.acitvityCategory, activity.GetName());
                    _model.gameSaveService.DeleteIcon(activity.acitvityCategory, activity.GetName());
                }
            }
            else
            {
                if (_deletedOnIsNotCashed.Contains(activity) == false)
                {
                    await AsyncHelper.NextFrame();
                    _model.gameSaveService.DeleteDrawings(activity.acitvityCategory, activity.GetName());
                    _model.gameSaveService.DeleteBackgroundDrawings(activity.acitvityCategory, activity.GetName());
                    _model.gameSaveService.DeleteIcon(activity.acitvityCategory, activity.GetName());

                    _deletedOnIsNotCashed.SafeAdd(activity);
                }
            }

            SetIcon();
        }

        protected async override void SetIcon()
        {
            var icon = await _saveService.GetIcon(activityData.acitvityCategory, activityData.activityName);

            if (icon != null)
            {
                _iconImage.sprite = icon;

                await AsyncHelper.DelayFloat(Random.Range(0.0f, 0.25f));
                transform.DOScale(1, Random.Range(0.15f, 0.75f)).SetEase(Ease.OutBack);
            }
            else
            {
                base.SetIcon();
            }

            _isInitialized = true;
        }
    }
}