using DataClasses.AssetReferences;
using Helpers;
using Identifiers;
using System.Collections.Generic;
using UI.Menus;
using UnityEngine;
using UnityEngine.UI;

namespace ColorfulTrain
{
    public class GameplayMenu_ColorfulTrain : MenuBase
    {
        public Button backButton;
        public RectTransform _content;

        [SerializeField] private ExternalAssetReference_HasComponent<ColorfulTrain_Wheel_UI> _wheelUIUnit_Prefab = new();
        [SerializeField] private Panel_UI _panel_UI;

        private ColorfulTrain_GameState_Model _model;

        public void Construct(ColorfulTrain_GameState_Model model)
        {
            _model = model;
        }

        private async void OnStateChanged(ColorfulTrain_StateBase colorfulTrain_StateBase)
        {
            foreach (var item in _content.GetComponentsInChildren<RectTransform>())
            {
                if (item == _content) { continue; }
                Destroy(item.gameObject);
            }

            if (colorfulTrain_StateBase is ColorfulTrain_CollectingState colorfulTrain_CollectingState)
            {
                List<PanelItem_UI> temp = new();

                var seats = _model.train.seats.Shuffle_Copy();
                foreach (var item in seats)
                {
                    var instance = await _wheelUIUnit_Prefab.InstantiateAsync(_content);
                    instance.Construct(item);
                    item.currentUI_Copy = instance;

                    temp.Add(instance.GetComponent<PanelItem_UI>());
                }

                _panel_UI.PrepareForAnimation(temp);
                await _panel_UI.Appear();
                _panel_UI.AnimateItems();
            }
            else
            {
                await _panel_UI.Disppear();
            }
        }
    }
}