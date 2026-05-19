using PaintCore;
using System.Threading.Tasks;
using UnityEngine;

namespace Helpers
{
    public static class PaintIn3DHelper
    {
        public static void SetTexture(this CwPaintableTexture paintableTexture, Texture texture)
        {
            paintableTexture.Texture = texture;
            paintableTexture.Deactivate();
            paintableTexture.Texture = texture;
            paintableTexture.GetComponent<Renderer>().sharedMaterial.mainTexture = texture;
            paintableTexture.Activate();
        }

        public async static Task<Color> GetDominantColor(this CwPaintableTexture paintableTexture, Texture2D texture, float nonGliggerAlpha = 0.98f)
        {
            Color averageColor = Color.white;

            if (texture == null) { return averageColor; }

            if (texture == null) { return averageColor; }
            Color[] pixels = texture.GetPixels();

            int countAboveHalfTransparent = 0;

            await Task.Run(() =>
            {
                float totalR = 0;
                float totalG = 0;
                float totalB = 0;

                foreach (Color pixel in pixels)
                {
                    if (pixel.a < 0.5) { continue; }

                    totalR += pixel.r;
                    totalG += pixel.g;
                    totalB += pixel.b;
                    countAboveHalfTransparent++;
                }

                if (countAboveHalfTransparent > 0)
                {
                    float avgR = (totalR / countAboveHalfTransparent);
                    float avgG = (totalG / countAboveHalfTransparent);
                    float avgB = (totalB / countAboveHalfTransparent);

                    averageColor = new Color(avgR, avgG, avgB, nonGliggerAlpha);
                }
            });

            return averageColor;
        }
    }
}