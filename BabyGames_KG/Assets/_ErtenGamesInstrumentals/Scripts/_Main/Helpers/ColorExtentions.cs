using UnityEngine;

namespace Helpers
{
    public static class ColorExtentions
    {
        public static string ToHEX(this Color color)
        {
            int r = Mathf.Clamp(Mathf.RoundToInt(color.r * 255), 0, 255);
            int g = Mathf.Clamp(Mathf.RoundToInt(color.g * 255), 0, 255);
            int b = Mathf.Clamp(Mathf.RoundToInt(color.b * 255), 0, 255);

            return $"#{r:X2}{g:X2}{b:X2}";
        }

        public static string Colorize(this string text, Color? color)
        {
#if UNITY_EDITOR
            if (color == null) { return text; }
            return $"<color={color.Value.ToHEX()}>{text}</color>";
#endif
            return text;
        }

        public static string Colorize(this string text, string color)
        {
            if (string.IsNullOrEmpty(color)) { return text; }
            return $"<color={color}>{text}</color>";
        }
    }
}