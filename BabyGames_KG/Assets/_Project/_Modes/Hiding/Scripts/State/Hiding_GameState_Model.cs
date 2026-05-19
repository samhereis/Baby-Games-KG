using _Project._Modes.Hiding.Scripts.SO;
using DataClasses;
using FX;
using Hiding;
using Identifiers;
using Interfaces.Providers;
using Interfaces.Services;
using Services;
using SO;
using Spine.Unity;
using System;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace _Project._Modes.Hiding.Scripts.State
{
    public class Hiding_GameState_Model
    {
        public Action goToMainMenuRequest;
        public Action goToNextActivityRequest;
        public Action replayRequest;
        public Action onFinish;

        public HidingActivity_Identifier hidingActivity_Identifier;
        public HidingController hidingController;

        [Inject] public Hiding_Data hiding_Data;

        [Inject] public IContentDeliveryService contentDeliveryService;
        [Inject] public ListOfAllMenus_SO listOfAllMenus;

        [Inject] public IActivity_Identifier_Provider activity_Identifier_Provider;
        [Inject] public IActivities_SO_Provider activities_SO_Provider;

        public HidingWave currentWave;
        public int currentWaveIndex;
        public SkeletonGraphic click;
        public HintHand_Drag hint_drag;

        public Activity nextActivity { get; private set; }
        public Activity activity;
        public ActivityCategory_SO activityCategory_SO;

        public Hiding_GameState_Model(Activity activity)
        {
            this.activity = activity;
        }

        public async Task Initialize()
        {
            DiService.Inject(this);

            this.nextActivity = await GetNextActivity();
            await LoadAcitivtyIdentifier();

            hint_drag = hidingController.hintHand_Drag;
        }

        public async Task LoadAcitivtyIdentifier()
        {
            var gameObject = await contentDeliveryService.GetAsset<GameObject>(activity.GetUrl(), activity.GetName(), activity.version);
            var activityIdentifier = MonoBehaviour.Instantiate(gameObject);
            hidingActivity_Identifier = activityIdentifier.GetComponent<HidingActivity_Identifier>();
            hidingController = hidingActivity_Identifier.Get<HidingController>();
        }

        private async Task<Activity> GetNextActivity()
        {
            var activities = await activities_SO_Provider.GetActivities_SO();
            foreach (var activityCategory in activities.activityCategories)
            {
                if (activityCategory.activityCategoryName == activity.acitvityCategory)
                {
                    foreach (var foundActivity in activityCategory.activities)
                    {
                        if (foundActivity.type == ActivityType.Hiding)
                        {
                            continue;
                        }

                        if (foundActivity.activityName == activity.activityName)
                        {
                            continue;
                        }

                        return foundActivity;
                    }
                }
            }

            return null;
        }
    }
}