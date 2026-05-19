using _Project._Modes.Hiding.Scripts.SO;
using Bubble;
using DataClasses;
using Identifiers;
using Interfaces.Providers;
using Interfaces.Services;
using Services;
using SO;
using System;
using System.Threading.Tasks;
using Zenject;

namespace _Project._Modes.Hiding.Scripts.State
{
    public class Bubble_GameState_Model
    {
        public Action goToMainMenuRequest;
        public Action goToNextActivityRequest;
        public Action replayRequest;
        public Action onFinish;

        [Inject] public Bubble_Data hiding_Data;

        [Inject] public IContentDeliveryService contentDeliveryService;
        [Inject] public ListOfAllMenus_SO listOfAllMenus;

        [Inject] public IActivity_Identifier_Provider activity_Identifier_Provider;
        [Inject] public IActivities_SO_Provider activities_SO_Provider;

        public BubbleActivity_Identifier activity_Identifier;
        public BubbleController bubbleController;

        public Activity nextActivity { get; private set; }
        public Activity activity { get; private set; }
        public ActivityCategory_SO activityCategory_SO { get; private set; }

        public Bubble_GameState_Model(Activity activity)
        {
            this.activity = activity;
        }

        public async Task Initialize()
        {
            DiService.Inject(this);
            this.nextActivity = await activity.GetNextActivity(activities_SO_Provider);
        }
    }
}