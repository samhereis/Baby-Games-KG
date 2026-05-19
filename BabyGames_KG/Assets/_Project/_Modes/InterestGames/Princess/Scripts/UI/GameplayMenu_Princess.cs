using CarTuning;
using DataClasses.AssetReferences;
using FX;
using Identifiers;
using System.Collections.Generic;
using System.Linq;
using UI.Menus;
using UnityEngine;
using UnityEngine.UI;

namespace Modes.Puzzle
{
    public class GameplayMenu_Princess : MenuBase
    {
        public CompleteButton checkMark;
        public Button backButton;

        public RectTransform content;

        [SerializeField] private ExternalAssetReference_HasComponent<Princess_WearubgUI_NoAnimation> _wearingUI_NoAnimation_Prefab = new();
        [SerializeField] private ExternalAssetReference_HasComponent<Princess_WearubgUI_WithAnimation> _wearingUI_WithAnimation_Prefab = new();
        [SerializeField] private Panel_UI _panelItem_UI;

        [Space]
        [SerializeField] private HintHand_Drag _hintHand_Drag;

        protected Gameplay_GameState_InterestGameGirl_Model _model;

        private Princess_StateBase _currentState;
        private List<Princess_WearubgUI_Base> _spawned_Object = new List<Princess_WearubgUI_Base>();

        public virtual void Construct(Gameplay_GameState_InterestGameGirl_Model model)
        {
            _model = model;

            _model.currentState.RemoveListener(OnStateChanged);
            _model.currentState.AddListener(OnStateChanged);

            _model.currentDress.RemoveListener(OnWearingChanged);
            _model.currentDress.AddListener(OnWearingChanged);

            checkMark.SetAcitve(false);
        }

        public override async void Enable(float? duration = null)
        {
            await _panelItem_UI.Disppear();

            base.Enable(duration);

            _model.currentState.RemoveListener(OnStateChanged);
            _model.currentState.AddListener(OnStateChanged);

            _model.currentDress.RemoveListener(OnWearingChanged);
            _model.currentDress.AddListener(OnWearingChanged);

            checkMark.onClick.AddListener(RequestNextState);
        }

        public async override void Disable(float? duration = null)
        {
            await _panelItem_UI.Disppear();

            base.Disable(duration);

            _model.currentState.RemoveListener(OnStateChanged);
            _model.currentDress.RemoveListener(OnWearingChanged);

            checkMark.onClick.RemoveListener(RequestNextState);
        }

        private async void OnStateChanged(Princess_StateBase currentState)
        {
            checkMark.SetAcitve(false);

            _currentState = currentState;

            await _panelItem_UI.HideItems();
            foreach (var item in content.GetComponentsInChildren<Princess_WearubgUI_Base>())
            {
                Destroy(item.gameObject);
            }
            _spawned_Object.Clear();

            var spawned_Object = new List<Transform>();
            var spawned_Target = new List<Transform>();
            foreach (var item in currentState.wearings)
            {
                if (item is Princess_Dress_NoAnimation princess_Dress_NoAnimation)
                {
                    var wearingUI = await _wearingUI_NoAnimation_Prefab.InstantiateAsync(content);
                    wearingUI.Construct(_model);
                    wearingUI.Initialize(princess_Dress_NoAnimation);

                    _spawned_Object.Add(wearingUI);
                    spawned_Object.Add(wearingUI.transform);
                    spawned_Target.Add(wearingUI._wearing.transform);
                }

                if (item is Princess_Dress_WithAnimation princess_Dress_WithAnimation)
                {
                    var wearingUI = await _wearingUI_WithAnimation_Prefab.InstantiateAsync(content);
                    wearingUI.Construct(_model);
                    wearingUI.Initialize(princess_Dress_WithAnimation);

                    _spawned_Object.Add(wearingUI);
                    spawned_Object.Add(wearingUI.transform);
                    spawned_Target.Add(wearingUI._wearing.transform);
                }
            }

            _panelItem_UI.PrepareForAnimation(spawned_Object.Select(x => x.GetComponent<PanelItem_UI>()).ToList());
            await _panelItem_UI.Appear();
            _panelItem_UI.AnimateItems();

            _hintHand_Drag?.objects.Clear();
            _hintHand_Drag?.targets.Clear();
            _hintHand_Drag?.objects.AddRange(spawned_Object);
            _hintHand_Drag?.targets.AddRange(spawned_Target);

            _hintHand_Drag?.SetIsActive(true);

            _baseSettings.canvasGroup.interactable = true;
        }

        private void OnWearingChanged(Princess_DressBase wearing)
        {
            checkMark.SetAcitve(true);
        }

        private void RequestNextState()
        {
            _hintHand_Drag.SetIsActive(false);
            _baseSettings.canvasGroup.interactable = false;

            foreach (var item in _currentState.wearings)
            {
                if (item is Princess_Dress_WithAnimation princess_Dress_WithAnimation)
                {
                    if (_model.currentDress.value != item)
                    {
                        item.gameObject.SetActive(false);
                    }
                }
            }

            foreach (var item in _spawned_Object)
            {
                item._wearing = null;
            }

            checkMark.SetAcitve(false);
            _model.onNextStateRequested?.Invoke();
        }
    }
}