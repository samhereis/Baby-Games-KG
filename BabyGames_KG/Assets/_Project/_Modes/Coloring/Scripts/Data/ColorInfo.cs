using System;
using UnityEngine;

namespace Modes.Coloring
{
    [Serializable]
    public class ColorInfo
    {
        public Color color;
        public Sprite pattern;
        public Sprite paletteIcon;
        public Sprite instrumentIcon;
        public int index;

        public void SetPaletteIconsColor(Texture2D texture)
        {
            int centerX = texture.width / 2;
            int centerY = texture.height / 2;

            SetPaletteIconsColor(texture, centerX, centerX);
        }

        public void SetPaletteIconsColor(Texture2D texture, int centerX, int centerY)
        {
            color = texture.GetPixel(centerX, centerY);
        }
    }
}