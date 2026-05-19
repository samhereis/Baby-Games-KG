namespace Modes.Coloring
{
    public class ColorItem : PalletetemBase
    {
        public override void Initialize(ColorInfo colorInfo)
        {
            _colorImage.color = colorInfo.color;
        }

        public override void Initialize(Pallete pallete, ColorInfo colorInfo, bool select)
        {
            _pallete = pallete;
            this.colorInfo = colorInfo;

            _colorImage.color = colorInfo.color;

            if (select) { EnableSelectedUI(); } else { DisableSelectedUI(); }
        }
    }
}