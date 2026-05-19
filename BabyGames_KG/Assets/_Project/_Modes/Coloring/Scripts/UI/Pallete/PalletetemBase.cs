using UnityEngine;
using UnityEngine.UI;

namespace Modes.Coloring
{
    public class PalletetemBase : MonoBehaviour
    {
        [field: SerializeField] public ColorInfo colorInfo { get; protected set; }

        [SerializeField] protected Image _colorImage;
        [SerializeField] protected Image _checkMarkImage;
        [SerializeField] protected Image _selectImage;

        protected Pallete _pallete;

        public virtual void Initialize(ColorInfo colorInfo)
        {
            _colorImage.sprite = colorInfo.paletteIcon;
            _colorImage.color = colorInfo.color;
        }

        public virtual void Initialize(Pallete pallete, ColorInfo newColorInfo, bool select)
        {
            _pallete = pallete;
            colorInfo = newColorInfo;

            Initialize(colorInfo);

            if (select) { EnableSelectedUI(); } else { DisableSelectedUI(); }
        }

        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(SelectColor);
        }

        private void OnDestroy()
        {
            GetComponent<Button>().onClick.RemoveListener(SelectColor);
        }

        public virtual void SelectColor()
        {
            _pallete.OnColorSelected(this);
        }

        public virtual void EnableSelectedUI()
        {
            _checkMarkImage.gameObject.SetActive(true);
            _selectImage.gameObject.SetActive(true);

            gameObject.transform.localScale = Vector3.one * 1.15f;
        }

        public virtual void DisableSelectedUI()
        {
            gameObject.transform.localScale = Vector3.one;
            _checkMarkImage.gameObject.SetActive(false);
            _selectImage.gameObject.SetActive(false);
        }
    }
}