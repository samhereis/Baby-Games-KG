using DG.Tweening;
using Helpers;
using Loggers;
using Services;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Modes.Coloring
{
    public class Pallete : MonoBehaviour
    {
        public event Action onUnlock;

        [FoldoutGroup("Debug"), ShowInInspector] private static ToolType _currentToolType = ToolType.Marker;
        [FoldoutGroup("Debug"), ShowInInspector] private static Dictionary<ToolType, int> _palleteIndexes = new Dictionary<ToolType, int>();

        [Space]
        [SerializeField] private List<InstrumentSO> _instruments = new();
        [SerializeField] private List<PalletePanel> _palletePanels;
        [SerializeField] private List<PalleteInstrumentSelector> _instrumentsButtons = new();

        [Space]
        [SerializeField] private Button _eraserButton;
        [SerializeField] private InstrumentSO _erasernsItrument;

        [Space]
        [SerializeField] private Button _buttonBg;

        [Space]
        [SerializeField] private float _panelsCloseSpeed;
        [SerializeField] private Ease _closeAnimationEase;

        [Space]
        [SerializeField] private float _panelsOpenSpeed;
        [SerializeField] private Ease _openAnimationEase;
        [SerializeField] private float _openDelayAfterClosingAnother;

        [Inject] private GameController _gameController;

        [FoldoutGroup("Debug"), SerializeField] private List<ToolBase> _tools = new();

        [FoldoutGroup("Debug"), ShowInInspector] private Dictionary<ToolType, InstrumentSO> _instrumentsDictionary = new();
        [FoldoutGroup("Debug"), ShowInInspector] private Dictionary<ToolType, PalletePanel> _pannelDictionary = new();
        [FoldoutGroup("Debug"), ShowInInspector] private Dictionary<ToolType, PalleteInstrumentSelector> _instrumentButtonsDictionary = new();

        [FoldoutGroup("Debug"), SerializeField] private PalletePanel _currentPanel;
        [FoldoutGroup("Debug"), SerializeField] private ToolBase _currentTool;

        private bool _isEverOpened = false;

        private void Awake()
        {
            _buttonBg.gameObject.SetActive(false);

            foreach (var item in _palletePanels)
            {
                item.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            _palleteIndexes = new();
            _instrumentsDictionary = new();
            _pannelDictionary = new();
            _instrumentButtonsDictionary = new();
        }

        public async Task Initialize()
        {
            DiService.Inject(this);

            foreach (var instrument in _instruments)
            {
                _instrumentsDictionary.Add(instrument.toolType, instrument);
            }

            foreach (var panel in _palletePanels)
            {
                _pannelDictionary.Add(panel.toolType, panel);
            }

            foreach (var toolButton in _instrumentsButtons)
            {
                toolButton.Initialize(_instruments.Find(x => x.toolType == toolButton.TypeTool));
                _instrumentButtonsDictionary.Add(toolButton.TypeTool, toolButton);
            }

            foreach (var palleteInstrumentData in _gameController.model.gameSettings.palleteData.palleteDefaults)
            {
                _palleteIndexes.Add(palleteInstrumentData.toolType, palleteInstrumentData.index);
            }

            _tools = FindObjectsByType<ToolBase>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();
            _currentPanel = _pannelDictionary[_currentToolType];
            _buttonBg.onClick.AddListener(() => { HidePanel(duration: _panelsCloseSpeed); });

            await SetColorsInstruments(ToolType.Marker);
            await SetColorsInstruments(ToolType.Brush);
            await SetShines(ToolType.Shine);
            await SetColorsInstruments(ToolType.Fill);
            await SetPatterns(ToolType.Pattern);
            await SetStikers(ToolType.Stiker);

            UpdateInstruments(_currentToolType);
            SetInstrumentButtons(_currentToolType);
            SetCurrentColor();
            UpdateLockButons();

            ActivateTool(_currentToolType, false, true);
            _isEverOpened = false;

            _eraserButton.onClick.AddListener(ActivateEraser);
        }

        private void ActivateEraser()
        {
            ActivateTool(ToolType._Eraser, false);
        }

        public void OnColorSelected(PalletetemBase selectedPalleteItem)
        {
            SetCurrentColor(selectedPalleteItem.colorInfo, _currentToolType);
            _palleteIndexes[_currentToolType] = _pannelDictionary[_currentToolType].palletetems.IndexOf(selectedPalleteItem);
            HideAllPanels(_panelsCloseSpeed);
        }

        public void OpenPanel()
        {
            ActivateTool(_currentToolType);
        }

        public async void ActivateTool(ToolType toolType, bool openPanel = true, bool isAuto = false)
        {
            if (_isEverOpened == false)
            {
                try
                {
                    foreach (var item in _palletePanels)
                    {
                        Vector3 position = _instrumentButtonsDictionary[item.toolType].GetComponent<RectTransform>().anchoredPosition3D;
                        item.GetComponent<RectTransform>().anchoredPosition3D = position;
                        item.transform.localScale = Vector3.zero;
                    }
                }
                catch (Exception ex)
                {
                    CustomLogger.instance?.LogException(ex);
                }
                _isEverOpened = true;
            }

            HideAllPanels(_panelsCloseSpeed);

            UpdateInstruments(toolType);
            SetInstrumentButtons(toolType);

            if (toolType == ToolType._Eraser)
            {
                (_eraserButton.targetGraphic as Image).sprite = _gameController.model.gameSettings.ui_eraser.onSprite;
                _eraserButton?.targetGraphic?.DOFade(1, 0.25f);
                _eraserButton?.targetGraphic?.rectTransform.DOAnchorPos3DY(_gameController.model.gameSettings.ui_eraser.onOnYPosition, 0.25f).SetEase(Ease.InOutBack); ;

                SetCurrentColor(_erasernsItrument.colorInfo[0], toolType);
                return;
            }
            else
            {
                (_eraserButton.targetGraphic as Image).sprite = _gameController.model.gameSettings.ui_eraser.offSprite;
                _eraserButton?.targetGraphic?.DOFade(_gameController.model.gameSettings.ui_eraser.offAlpha, 0.25f);
                _eraserButton?.targetGraphic?.rectTransform.DOAnchorPos3DY(_gameController.model.gameSettings.ui_eraser.onOffYPosition, 0.25f).SetEase(Ease.InOutBack);

                if (openPanel == false) { return; }

                bool anyPanelWasOpen = _currentPanel?.canvasGroup?.interactable == true;
                if (anyPanelWasOpen && _currentPanel?.toolType == toolType) { return; }

                _currentToolType = toolType;
                _currentPanel = _pannelDictionary[toolType];

                _buttonBg.gameObject.SetActive(true);

                if (anyPanelWasOpen) { await AsyncHelper.DelayFloat(_openDelayAfterClosingAnother); }

                CanvasGroup canvasGroup = _currentPanel.canvasGroup;
                canvasGroup.gameObject.SetActive(true);
                canvasGroup.interactable = true;

                canvasGroup.transform.DOKill();
                canvasGroup.transform.DOScale(1, _panelsOpenSpeed).SetEase(_openAnimationEase);
                canvasGroup.transform.DOLocalMove(Vector3.zero, _panelsOpenSpeed).SetEase(_openAnimationEase);
            }
        }

        public void HidePanel(PalletePanel palletePanel = null, float duration = 0)
        {
            if (palletePanel == null) { palletePanel = _currentPanel; }

            _buttonBg.gameObject.SetActive(false);

            palletePanel.canvasGroup.interactable = false;
            palletePanel.transform.transform.DOKill();
            palletePanel.transform.DOScale(0, duration).SetEase(_closeAnimationEase);

            Vector3 position = _instrumentButtonsDictionary[palletePanel.toolType].transform.position;
            palletePanel.transform.DOMove(position, duration).SetEase(_closeAnimationEase);
        }

        [Button]
        public void HideAllPanels(float duration)
        {
            _buttonBg.gameObject.SetActive(false);

            foreach (var toolPanel in _palletePanels)
            {
                if (duration == 0) { toolPanel.gameObject.SetActive(false); }
                HidePanel(toolPanel, duration);
            }
        }

        public void SetCurrentColor()
        {
            ColorInfo colorInfo = null;

            foreach (var i in _palleteIndexes)
            {
                colorInfo = _instrumentsDictionary[i.Key].colorInfo[i.Value];
                SetCurrentColor(colorInfo, i.Key);
            }

            colorInfo = _instrumentsDictionary[_currentToolType].colorInfo[_palleteIndexes[_currentToolType]];
            SetCurrentColor(colorInfo, _currentToolType);
        }

        public void SetCurrentColor(ColorInfo colorinfo, ToolType toolType)
        {
            _currentTool = _tools.Find(x => x.type == toolType);
            _currentTool?.Init(colorinfo);

            if (toolType == ToolType._Eraser) { return; }

            _instrumentButtonsDictionary[toolType]?.OnColorSelected(colorinfo);
            foreach (PalletetemBase palleteItem in _pannelDictionary[toolType]?.palletetems)
            {
                if (palleteItem.colorInfo == colorinfo) { palleteItem.EnableSelectedUI(); }
                else { palleteItem.DisableSelectedUI(); }
            }
        }

        public void UpdateInstruments(ToolType selectedToolType)
        {
            foreach (var item in _tools)
            {
                if (selectedToolType == item.type) { item.gameObject.SetActive(true); }
                else { item.gameObject.SetActive(false); }
            }
        }

        public void SetInstrumentButtons(ToolType selectedToolType)
        {
            foreach (var button in _instrumentsButtons)
            {
                button.UpdateSelected(selectedToolType);
            }
        }

        private void UpdateLockButons()
        {
            foreach (var item in _palletePanels)
            {
                item.SetAvailability(true, () => { onUnlock?.Invoke(); });
            }
        }

        private async Task SetColorsInstruments(ToolType toolType)
        {
            int childCount = _instrumentsDictionary[toolType].colorInfo.Count;
            PalletePanel palletePanel = _pannelDictionary[toolType];

            PalleteInstrumentSelector instrumentItem = _instrumentButtonsDictionary[toolType];
            InstrumentSO instrument = _instrumentsDictionary[toolType];
            await palletePanel.Initialize(instrument);

            var colorItems = palletePanel.palletetems;

            for (int i = 0; i < childCount; i++)
            {
                bool isActivate = i == _palleteIndexes[toolType];
                ColorInfo colorInfo = instrument.colorInfo[i];

                ColorItem colorItem = colorItems[i].GetComponent<ColorItem>();
                colorItem.Initialize(this, colorInfo, isActivate);
            }
        }

        private async Task SetShines(ToolType toolType)
        {
            int childCount = _instrumentsDictionary[toolType].colorInfo.Count;
            PalletePanel palletePanel = _pannelDictionary[toolType];
            PalleteInstrumentSelector instrumentItem = _instrumentButtonsDictionary[toolType];
            InstrumentSO instrument = _instrumentsDictionary[toolType];
            await palletePanel.Initialize(instrument);

            var colorItems = palletePanel.palletetems;

            for (int i = 0; i < childCount; i++)
            {
                bool isActivate = i == _palleteIndexes[toolType];

                var colorInfo = instrument.colorInfo[i];
                colorItems[i].Initialize(this, colorInfo, isActivate);
            }
        }

        private async Task SetPatterns(ToolType toolType)
        {
            int childCount = _instrumentsDictionary[toolType].colorInfo.Count;
            PalletePanel palletePanel = _pannelDictionary[toolType];
            PalleteInstrumentSelector instrumentItem = _instrumentButtonsDictionary[toolType];
            InstrumentSO instrument = _instrumentsDictionary[toolType];
            await palletePanel.Initialize(instrument);

            var colorItems = palletePanel.palletetems;

            for (int i = 0; i < childCount; i++)
            {
                bool isActivate = i == _palleteIndexes[toolType];

                var colorInfo = instrument.colorInfo[i];
                colorItems[i].Initialize(this, colorInfo, isActivate);
            }
        }

        private async Task SetStikers(ToolType toolType)
        {
            int childCount = _instrumentsDictionary[toolType].colorInfo.Count;
            PalletePanel palletePanel = _pannelDictionary[toolType];
            PalleteInstrumentSelector instrumentItem = _instrumentButtonsDictionary[toolType];
            InstrumentSO instrument = _instrumentsDictionary[toolType];
            await palletePanel.Initialize(instrument);

            var colorItems = palletePanel.palletetems;

            for (int i = 0; i < childCount; i++)
            {
                bool isActivate = i == _palleteIndexes[toolType];
                var colorInfo = _instrumentsDictionary[ToolType.Stiker].colorInfo[i];
                colorItems[i].Initialize(this, colorInfo, isActivate);
            }
        }
    }
}