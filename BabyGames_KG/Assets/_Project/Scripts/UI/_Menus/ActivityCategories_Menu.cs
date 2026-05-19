using System;
using CustomAttributes;
using DanielLochner.Assets.SimpleScrollSnap;
using DG.Tweening;
using GameState;
using Helpers;
using Identifiers.UI;
using RTLTMPro;
using Sirenix.OdinInspector;
using SO;
using Sounds;
using System.Collections.Generic;
using System.Threading.Tasks;
using Services;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI.Menus
{
    public class ActivityCategories_Menu : MenuBase
    {
        [field: SerializeField] public Button forParentsButton;
        [field: SerializeField] public Button unlockButton;
        [field: SerializeField] public RectTransform subsInfoParent;
        [field: SerializeField] public RTLTextMeshPro subsInfo;

        [SerializeField][Re_Fg_Co] private RectTransform _contentHolder;
        [SerializeField][Re_Fg_Co] private SimpleScrollSnap _scrollSnap;

        [Space]
        [SerializeField][Fg_De] private List<ActivityCategory_UIUnit> _activityCategory_Instances = new();
        [SerializeField][Fg_De] private ActivityCategory_UIUnit _currentObjectInCenter;
        [SerializeField][Fg_De] private int _currentObjectInCenter_Index;
        [SerializeField][Fg_De] private List<Sound> _lastlyPlayedSounds = new();
        [SerializeField][Fg_De] private bool _firstPlayedIgnored = new();

        private MainMenu_GameState_Model _model;

        public async Task Initialize(MainMenu_GameState_Model model)
        {
            _model = model;

            _activityCategory_Instances = await _model.activityCategory_UIUnits_Provider.GetActivityCategory_UIUnits(_contentHolder);
            _scrollSnap.StartingPanel = _model.savedPannelNumber;
            _scrollSnap.Initialize();

            await AsyncHelper.NextFrame();
            GoToPannel();
            _currentObjectInCenter_Index = _scrollSnap.GetNearestPanel();
            _currentObjectInCenter = _scrollSnap.Panels[_currentObjectInCenter_Index].GetComponent<ActivityCategory_UIUnit>();
        }

        private void OnDisable()
        {
            LazyUpdator_Service.instance?.RemoveFromQueue(ShowSubscriptionInfo);
        }

        private void Update()
        {
            var nearestPanelIndex = _scrollSnap.GetNearestPanel();
            if (_currentObjectInCenter_Index != nearestPanelIndex)
            {
                OnScrolled(nearestPanelIndex);
            }
        }

        public async override void Enable(float? duration = null)
        {

            base.Enable(duration);

            foreach (var item in _activityCategory_Instances)
            {
                item.onChoose -= OnActivityCategoryChosen;
                item.onChoose += OnActivityCategoryChosen;
            }

            await AsyncHelper.NextFrame();
            GoToPannel();

            unlockButton.onClick.RemoveListener(OpenUnlockButton);
            unlockButton.onClick.AddListener(OpenUnlockButton);
            
            LazyUpdator_Service.instance?.AddToQueue(ShowSubscriptionInfo);
        }

        public override void Disable(float? duration = null)
        {
            base.Disable(duration);

            foreach (var item in _activityCategory_Instances)
            {
                item.onChoose -= OnActivityCategoryChosen;
            }

            subsInfoParent.DOKill();
            
            LazyUpdator_Service.instance?.RemoveFromQueue(ShowSubscriptionInfo);
        }

        private async void OnScrolled(int nearestPanelIndex)
        {
            _currentObjectInCenter_Index = nearestPanelIndex;
            _currentObjectInCenter = _scrollSnap.Panels[_currentObjectInCenter_Index].GetComponent<ActivityCategory_UIUnit>();
            var sound = await _currentObjectInCenter.categorySound.GetSound();

            if(_firstPlayedIgnored == false) 
            { 
                _firstPlayedIgnored = true;
                return;
            }
            
            if (_lastlyPlayedSounds.Contains(_currentObjectInCenter.categorySound) == false)
            {
                _model.soundPlayer.TryPlay(_currentObjectInCenter.categorySound);
                _lastlyPlayedSounds.Add(_currentObjectInCenter.categorySound);

                await AsyncHelper.DelayFloat(sound.length);
                _lastlyPlayedSounds.RemoveAll(x => x == _currentObjectInCenter.categorySound);
            }
        }

        private async void OnActivityCategoryChosen(ActivityCategory_UIUnit category_UIUnit)
        {
            _model.savedPannelNumber = _activityCategory_Instances.IndexOf(_activityCategory_Instances.Find(x => x == category_UIUnit));

            if (category_UIUnit == _currentObjectInCenter)
            {
                await category_UIUnit.PlayChoseAnimation();
                _model.onActivityCategoryOpenRequested?.Invoke(category_UIUnit.activityCategory);
            }
            else
            {
                GoToPannel();
            }
        }

        private void OpenUnlockButton()
        {
            _model.onPurchaseRequested?.Invoke();
        }

        [Button]
        private void GoToPannel()
        {
            _scrollSnap.SelectedPanel = _model.savedPannelNumber;
            _scrollSnap.CenteredPanel = _model.savedPannelNumber;
            _scrollSnap.StartingPanel = _model.savedPannelNumber;

            _scrollSnap.GoToPanel(_model.savedPannelNumber);
            _contentHolder.DOAnchorPos3DY(_model.savedPannelPosition, 0.1f);
        }

        private async Task ShowSubscriptionInfo()
        {
            try
            {
                subsInfoParent.DOMoveY(-250, 0);
                
                if (_model.subscriptionController.IsSubscribed() == false)
                {
                    unlockButton.gameObject.SetActive(true);
                    subsInfo.text = "Not Subscribed";
                }
                else
                {
                    unlockButton.gameObject.SetActive(false);
                    subsInfo.text = _model.subscriptionController.GetFullSubscriptionInfo();
                }
                
                await AsyncHelper.DelayFloat(1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}