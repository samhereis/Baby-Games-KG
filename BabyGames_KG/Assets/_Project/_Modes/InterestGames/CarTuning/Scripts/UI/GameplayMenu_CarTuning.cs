using Assets._Project._Modes.Carwash.Scripts.State;
using DG.Tweening;
using FX;
using Identifiers;
using System.Collections.Generic;
using System.Linq;
using _Project._Modes.InterestGames.CarTuning.Scripts.UI;
using UI.Menus;
using UnityEngine;
using UnityEngine.UI;

namespace CarTuning
{
    public class GameplayMenu_CarTuning : MenuBase
    {
        public CompleteButton checkMark;
        public Button backButton;

        [SerializeField] private Panel_UI _panel;
        [SerializeField] private CanvasGroup _content;
        [SerializeField] private CanvasGroup _color;
        [SerializeField] private List<CarTuning_PartUI_Color> _colors = new();

        [SerializeField] private CarTuning_PartUI_NoAnimation _carPartUI_NoAnimation_Prefab;
        [SerializeField] private CarTuning_PartUI_WithAnimation _carPartUI_WithAnimation;

        [Space]
        [SerializeField] private HintHand_Drag _hintHand_Drag;
        [SerializeField] private HintHand_DrawSimple _hintHand_Draw;

        protected CarTuning_GameState_Model _model;
        private List<CarPart_Base> _currentCarParts = new List<CarPart_Base>();

        protected override void Awake()
        {
            base.Awake();
            checkMark.SetAcitve(false);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _color.DOKill();
        }

        public virtual void Construct(CarTuning_GameState_Model model)
        {
            _model = model;

            _color.gameObject.SetActive(false);
            _color.alpha = 0;

            foreach (var item in _color.GetComponentsInChildren<CarTuning_PartUI_Color>(true))
            {
                item.Construct(_model);
            }

            _model.currentCar.AddListener(OnCarChanged);
            _model.currentCarPart.AddListener(OnCarPartChanged);
        }

        public async void Initialize<T>(List<T> carPart_Base) where T : CarPart_Base
        {
            _currentCarParts.Clear();
            _currentCarParts.AddRange(carPart_Base);

            checkMark.SetAcitve(false);

            if (_panel != null) { await _panel.HideItems(); }

            foreach (var item in _content.GetComponentsInChildren<CarTuning_PartUI_Base>(true))
            {
                Destroy(item.gameObject);
            }

            var spawned_Object = new List<Transform>();
            var spawned_Target = new List<Transform>();
            foreach (var item in carPart_Base)
            {
                if (item is CarPart_NoAnimation carPart_NoAnimation)
                {
                    var newInstance = Instantiate(_carPartUI_NoAnimation_Prefab, _content.transform);

                    newInstance.Construct(_model);
                    newInstance.Initialize(carPart_NoAnimation);

                    spawned_Object.Add(newInstance.transform);
                    spawned_Target.Add(newInstance._carPart.transform);

                    _panel?.PrepareForAnimation(newInstance.GetComponent<PanelItem_UI>());
                }
                else if (item is CarPart_WithAnimation carPart_WithAnimation)
                {
                    var newInstance = Instantiate(_carPartUI_WithAnimation, _content.transform);

                    newInstance.Construct(_model);
                    newInstance.Initialize(carPart_WithAnimation);

                    spawned_Object.Add(newInstance.transform);
                    spawned_Target.Add(newInstance._carPart.transform);

                    _panel?.PrepareForAnimation(newInstance.GetComponent<PanelItem_UI>());
                }
            }

            if (_panel != null)
            {
                await _panel.Appear();
                await _panel.AnimateItems(spawned_Object.Select(x => x.GetComponent<PanelItem_UI>()).ToList());
            }

            _hintHand_Drag?.objects.Clear();
            _hintHand_Drag?.targets.Clear();
            _hintHand_Drag?.objects.AddRange(spawned_Object);
            _hintHand_Drag?.targets.AddRange(spawned_Target);

            _hintHand_Drag.SetIsActive(true);
        }

        public async void ColorMode()
        {
            RectTransform contentRectTransform = _content.transform.parent.GetComponent<RectTransform>();
            float initialX = contentRectTransform.anchoredPosition.x;

            await contentRectTransform.DOAnchorPos3DX(700, 0.25f).AsyncWaitForCompletion();

            _content.gameObject.SetActive(false);
            _color.gameObject.SetActive(true);

            _color.DOFade(1, 1);

            contentRectTransform.DOAnchorPos3DX(initialX, 0.25f);

            _hintHand_Drag.SetIsActive(false);

            _hintHand_Draw.sources.Clear();
            _hintHand_Draw.targets.Clear();
            _hintHand_Draw.sources.AddRange(_colors.Select(x => x.transform));
            _hintHand_Draw.targets.Add(_model.currentCar.value.transform);

            _hintHand_Draw.gameObject.SetActive(true);
            _hintHand_Draw.SetIsActive(true);

            checkMark.onClick.AddListener(async () =>
            {
                await _panel.Disppear();
            });
        }

        private void OnCarChanged(CarTuning_Car_Identifier identifier)
        {

        }

        private void OnCarPartChanged(CarPart_Base carPart_Base)
        {
            if (carPart_Base is CarPart_NoAnimation carPart_NoAnimation)
            {
                if (carPart_NoAnimation.second != null)
                {
                    _hintHand_Drag?.targets.Clear();
                    _hintHand_Drag?.targets.AddRange(_currentCarParts.Select(x => x.second.transform));
                }
            }
        }
    }
}