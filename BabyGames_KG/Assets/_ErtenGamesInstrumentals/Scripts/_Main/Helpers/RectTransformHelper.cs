using UnityEngine;

namespace Helpers
{
    public static class RectTransformExtensions
    {
        public static Rect[] Row(this Rect rect, float[] wights)
        {
            Rect[] result = new Rect[wights.Length];

            float sum = 0;
            for (int i = 0; i < wights.Length; i++) sum += wights[i] < 0 ? (wights[i] = 1) : wights[i];

            for (int i = 0; i < wights.Length; i++)
            {
                float ratio = wights[i] / sum;
                var newRect = new Rect();
                newRect.height = rect.height;
                newRect.width = rect.width * ratio;
                newRect.y = rect.y;
                float lastX = 0;

                if (i > 0) lastX = result[i - 1].xMax;

                newRect.x = lastX;
                result[i] = newRect;
            }

            return result;
        }

        public static void SetLeft(this RectTransform rt, float left)
        {
            if (rt.offsetMin.x != left) rt.offsetMin = new Vector2(left, rt.offsetMin.y);
        }

        public static void SetBottom(this RectTransform rt, float bottom)
        {
            if (rt.offsetMin.y != bottom) rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
        }

        public static void SetRight(this RectTransform rt, float right)
        {
            if (rt.offsetMax.x != -right) rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
        }

        public static void SetTop(this RectTransform rt, float top)
        {
            if (rt.offsetMax.y != -top) rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
        }
    }
}