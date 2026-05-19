using _Project._Modes.Orchestra.Scripts;
using CustomAttributes;
using DataClasses.AssetReferences;
using DG.Tweening;
using FX;
using Helpers;
using Identifiers;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UI.Menus;
using UnityEngine;
using UnityEngine.UI;

namespace Modes.Sorting
{
    public class GameplayMenu_Orchestra : MenuBase
    {
        [SerializeField] [Re_Fg_Co] private Button _backButton;
        [SerializeField] private HintHand_Drag _hintHand;

        [SerializeField] private float _hintHandDelay = 5;

        [SerializeField] private ExternalAssetReference_HasComponent<Orchestra_Draggable> _draggablesPrefab;
        [SerializeField] private Panel_UI _panel_UI;

        private Gameplay_GameState_Orchestra_Model _model;

        protected override void Awake()
        {
            base.Awake();
        }

        public async void Initialize(Gameplay_GameState_Orchestra_Model model)
        {
            _model = model;

            foreach (var item in TryGetAll_List<Orchestra_Draggable>())
            {
                Destroy(item.gameObject);
            }

            _hintHand.objects.Clear();
            _hintHand.targets.Clear();

            List<PanelItem_UI> temp = new();
            foreach (var item in _model.currentOrchestraWaveData.waveUnits.Shuffle_Copy())
            {
                var draggable = await _draggablesPrefab.InstantiateAsync(_holder);
                draggable.Initialize(item, _model.activity_Identifier.Get<OrchestraController>());
                item.draggable = draggable;

                _hintHand.objects.Add(item.draggable.transform);
                _hintHand.targets.Add(item.dropZone.transform);

                temp.Add(draggable.Get<PanelItem_UI>());
            }
            _hintHand.SetIsActive(true);

            _model.onPanelVisibilityChanged -= MoveContentPannel;
            _model.onPanelVisibilityChanged += MoveContentPannel;

            _model.onWin -= OnWin;
            _model.onWin += OnWin;

            _model.onWaveCompleted -= OnWaveCompleted;
            _model.onWaveCompleted += OnWaveCompleted;

            _panel_UI.PrepareForAnimation(temp);
            MoveContentPannel(true);
        }

        public override void Enable(float? duration = null)
        {
            base.Enable(duration);

            _backButton.onClick.RemoveListener(OnBackButtonClicked);
            _backButton.onClick.AddListener(OnBackButtonClicked);
        }

        public override void Disable(float? duration = null)
        {
            _backButton.onClick.RemoveListener(OnBackButtonClicked);

            _model.onWaveCompleted -= OnWaveCompleted;

            MoveContentPannel(false);
            base.Disable(duration);
        }

        [Button]
        private async void MoveContentPannel(bool visible)
        {
            if (visible)
            {
                await _panel_UI.Appear();
                _panel_UI.AnimateItems();
            }
            else
            {
                await _panel_UI.Disppear();
            }
        }

        private void OnWaveCompleted(OrchestraWaveData waveUnit)
        {
            _hintHand.SetIsActive(false);
        }

        private void OnWin()
        {
            _backButton.transform.DOScale(0, 0.25f);
        }

        private void OnBackButtonClicked()
        {
            _model.goToMainMenuRequest?.Invoke();
        }
    }
}