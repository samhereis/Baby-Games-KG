using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Modes.Coloring
{
    public class PalleteInstrumentSelector : MonoBehaviour
    {
        public ToolType TypeTool => _type;
        [SerializeField] private ToolType _type = ToolType.Brush;

        [SerializeField] private InstrumentSO _instrument;
        [SerializeField] private Pallete _pallite;
        [SerializeField] private Image _image;

        [SerializeField] private float _targetPosX = -50;
        [SerializeField] private float _defoltPosX = -10;
        private bool _isSelected = false;

        public void Initialize(InstrumentSO instrumentSO)
        {
            _instrument = instrumentSO;
        }

        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(OnClickButtonInstrument);
        }

        private void OnDestroy()
        {
            GetComponent<Button>().onClick.RemoveListener(OnClickButtonInstrument);
        }

        public void OnColorSelected(ColorInfo colorInfo)
        {
            if (colorInfo.instrumentIcon != null) { _image.sprite = colorInfo.instrumentIcon; };
        }

        public void UpdateSelected(ToolType selectedToolType)
        {
            if (_type == selectedToolType) { EnableTool(); } else { DisableTool(); }
        }

        private void OnClickButtonInstrument()
        {
            _pallite.ActivateTool(_instrument.toolType);
        }

        public void MoveToPositionX(float posX)
        {
            _image.GetComponent<RectTransform>().DOAnchorPosX(posX, 0.5f);
        }

        public void EnableTool()
        {
            if (_isSelected) return;

            MoveToPositionX(_targetPosX);
            _isSelected = true;
        }

        public void DisableTool()
        {
            if (!_isSelected) return;

            MoveToPositionX(_defoltPosX);
            _isSelected = false;
        }
    }
}