using UnityEngine;

namespace Helpers.ImageCroppers
{
    public static class ImageCropper_Implementation1
    {
        public static Rect FindNonTransparentBoundsIncludingOffset(Texture2D texture)
        {
            Color32[] pixels = texture.GetPixels32();
            int width = texture.width;
            int height = texture.height;

            // Initialize bounds with extreme values
            int minX = width;
            int minY = height;
            int maxX = -1;
            int maxY = -1;

            // Iterate through pixels to find non-transparent bounds
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color32 pixel = pixels[y * width + x];
                    if (pixel.a != 0) // Non-transparent pixel
                    {
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                    }
                }
            }

            // Return bounds as a Rect
            if (minX <= maxX && minY <= maxY)
            {
                return new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
            }
            else
            {
                // No non-transparent pixels found, return an empty rect
                return new Rect(0, 0, 0, 0);
            }
        }

        public static Vector2 CalculateCenteringOffset(Rect bounds, int textureWidth, int textureHeight)
        {
            float offsetX = (textureWidth - bounds.width) / 2 - bounds.xMin;
            float offsetY = (textureHeight - bounds.height) / 2 - bounds.yMin;

            return new Vector2(offsetX, offsetY);
        }

        public static Vector2 CalculateScaleToFit(Rect bounds, int desiredWidth, int desiredHeight)
        {
            float scaleX = desiredWidth / bounds.width;
            float scaleY = desiredHeight / bounds.height;

            // Choose the minimum scale to ensure the entire non-transparent area fits within the desired size
            float scale = Mathf.Min(scaleX, scaleY);

            return new Vector2(scale, scale);
        }


        public static Texture2D ScaleTextureToFitAndCenter(Texture2D originalTexture)
        {
            Rect nonTransparentBounds = FindNonTransparentBoundsIncludingOffset(originalTexture);

            if (nonTransparentBounds.width == 0 || nonTransparentBounds.height == 0)
            {
                Debug.LogWarning("Texture has no non-transparent pixels.");
                return null;
            }

            Vector2 centerOffset = CalculateCenteringOffset(nonTransparentBounds, originalTexture.width, originalTexture.height);

            // Adjust bounds with offset
            nonTransparentBounds.x += Mathf.FloorToInt(centerOffset.x);
            nonTransparentBounds.y += Mathf.FloorToInt(centerOffset.y);

            // Calculate scale to fit desired size
            Vector2 scale = CalculateScaleToFit(nonTransparentBounds, originalTexture.width, originalTexture.height);

            int newWidth = Mathf.CeilToInt(scale.x * nonTransparentBounds.width);
            int newHeight = Mathf.CeilToInt(scale.y * nonTransparentBounds.height);

            Color32[] scaledPixels = new Color32[newWidth * newHeight];

            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    int originalX = Mathf.FloorToInt(nonTransparentBounds.x + x / scale.x);
                    int originalY = Mathf.FloorToInt(nonTransparentBounds.y + y / scale.y);

                    Color32 pixel = originalTexture.GetPixel(originalX, originalY);
                    scaledPixels[y * newWidth + x] = pixel;
                }
            }

            Texture2D scaledTexture = new Texture2D(newWidth, newHeight, originalTexture.format, originalTexture.mipmapCount > 1);
            scaledTexture.wrapMode = originalTexture.wrapMode;
            scaledTexture.filterMode = originalTexture.filterMode;
            scaledTexture.anisoLevel = originalTexture.anisoLevel;

            scaledTexture.SetPixels32(scaledPixels);
            scaledTexture.Apply();

            return scaledTexture;
        }

    }
}
