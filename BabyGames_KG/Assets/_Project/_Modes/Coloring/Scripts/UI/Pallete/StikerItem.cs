namespace Modes.Coloring
{
    public class StikerItem : PalletetemBase
    {
        public override void Initialize(Pallete pallete, ColorInfo colorInfo, bool select)
        {
            base.Initialize(pallete, colorInfo, select);
            _colorImage.sprite = colorInfo.pattern;
        }
    }
}