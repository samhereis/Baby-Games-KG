using UnityEngine;

namespace UnityUtils
{
    public class ScreenUtils
    {
        private static Vector2 ScreenSizeInPixels() { return new Vector2(Screen.width, Screen.height); }
        private static float ScreenHeightInPixels() { return Screen.height; }

        public static Vector2 ScreenSizeUI(float canvasHeight = 1080f) { return ScreenSizeInPixels() * (canvasHeight / ScreenHeightInPixels()); }
        public static float ScreenHeightUI(float multiplier = 1.0f, float canvasHeight = 1080f) { return ScreenSizeUI(canvasHeight).y * multiplier; }
    }
}