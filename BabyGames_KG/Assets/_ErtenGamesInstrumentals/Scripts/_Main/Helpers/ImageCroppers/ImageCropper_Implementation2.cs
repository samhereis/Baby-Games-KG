using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Helpers.ImageCroppers
{
    public static class ImageCropper_Implementation2
    {
        public static async Task<Texture2D> CropAlphas_Square(Texture2D texture, int xPadding = 0, int yPadding = 0)
        {
            if (texture == null)
            {
                return null;
            }

            int width = texture.width;
            int height = texture.height;

            int minX = width, minY = height, maxX = 0, maxY = 0;
            Color32[] originalTexture_Colors = texture.GetPixels32();

            await Task.Run(() =>
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (originalTexture_Colors[y * width + x].a != 0)
                        {
                            if (x < minX) minX = x;
                            if (x > maxX) maxX = x;
                            if (y < minY) minY = y;
                            if (y > maxY) maxY = y;
                        }
                    }
                }
            });

            minX = Math.Max(0, minX - xPadding);
            minY = Math.Max(0, minY - yPadding);
            maxX = Math.Min(width - 1, maxX + xPadding);
            maxY = Math.Min(height - 1, maxY + yPadding);

            int croppedWidth = Mathf.Abs(maxX - minX) + 1;
            int croppedHeight = Mathf.Abs(maxY - minY) + 1;

            Texture2D croppedTexture = new Texture2D(croppedWidth, croppedHeight, texture.format, texture.mipmapCount > 1);
            croppedTexture.wrapMode = texture.wrapMode;
            croppedTexture.filterMode = texture.filterMode;
            croppedTexture.anisoLevel = texture.anisoLevel;

            Color32[] croppedTexture_Colors = new Color32[croppedWidth * croppedHeight];

            await Task.Run(() =>
            {
                for (int x = 0; x < croppedWidth; x++)
                {
                    for (int y = 0; y < croppedHeight; y++)
                    {
                        int croppedIndex = y * croppedWidth + x;
                        int originalIndex = (minY + y) * width + (minX + x);

                        if (croppedIndex >= croppedTexture_Colors.Length || originalIndex >= originalTexture_Colors.Length)
                        {
                            continue;
                        }

                        croppedTexture_Colors[croppedIndex] = originalTexture_Colors[originalIndex];
                    }
                }
            });

            croppedTexture.SetPixels32(croppedTexture_Colors);
            croppedTexture.Apply();
            return croppedTexture;
        }

        public static async Task<Texture2D> CropAlphas_Rectangular(Texture2D texture, int xPadding = 0, int yPadding = 0)
        {
            if (texture == null)
            {
                return null;
            }

            int width = texture.width;
            int height = texture.height;

            int minX = width, minY = height, maxX = 0, maxY = 0;
            Color32[] originalColors = texture.GetPixels32();

            // Find the bounds of non-transparent pixels.
            await Task.Run(() =>
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (originalColors[y * width + x].a != 0)
                        {
                            if (x < minX) minX = x;
                            if (x > maxX) maxX = x;
                            if (y < minY) minY = y;
                            if (y > maxY) maxY = y;
                        }
                    }
                }
            });

            // Apply optional padding (ensuring boundaries stay within the original image)
            minX = Math.Max(0, minX - xPadding);
            minY = Math.Max(0, minY - yPadding);
            maxX = Math.Min(width - 1, maxX + xPadding);
            maxY = Math.Min(height - 1, maxY + yPadding);

            int croppedWidth = maxX - minX + 1;
            int croppedHeight = maxY - minY + 1;

            Texture2D croppedTexture = new Texture2D(croppedWidth, croppedHeight, texture.format, texture.mipmapCount > 1);
            croppedTexture.wrapMode = texture.wrapMode;
            croppedTexture.filterMode = texture.filterMode;
            croppedTexture.anisoLevel = texture.anisoLevel;

            Color32[] croppedColors = new Color32[croppedWidth * croppedHeight];

            // Copy the pixel data from the original texture to the new cropped texture.
            await Task.Run(() =>
            {
                for (int x = 0; x < croppedWidth; x++)
                {
                    for (int y = 0; y < croppedHeight; y++)
                    {
                        int croppedIndex = y * croppedWidth + x;
                        int originalIndex = (minY + y) * width + (minX + x);
                        croppedColors[croppedIndex] = originalColors[originalIndex];
                    }
                }
            });

            croppedTexture.SetPixels32(croppedColors);
            croppedTexture.Apply();
            return croppedTexture;
        }



        public static async Task<Texture2D> ScaleTextureToCenter(Texture2D texture)
        {
            int width = texture.width;
            int height = texture.height;

            int maxSize = Mathf.Max(width, height);

            Texture2D scaledTexture = new Texture2D(maxSize, maxSize, texture.format, texture.mipmapCount > 1);
            scaledTexture.wrapMode = texture.wrapMode;
            scaledTexture.filterMode = texture.filterMode;
            scaledTexture.anisoLevel = texture.anisoLevel;

            Color32[] originalTexture_Colors = texture.GetPixels32();
            Color32[] scaledTexture_Colors = scaledTexture.GetPixels32();

            await Task.Run(() =>
            {
                int offsetX = (maxSize - width) / 2;
                int offsetY = (maxSize - height) / 2;

                for (int x = 0; x < maxSize; x++)
                {
                    for (int y = 0; y < maxSize; y++)
                    {
                        if (x >= offsetX && x < offsetX + width && y >= offsetY && y < offsetY + height)
                        {
                            scaledTexture_Colors[y * maxSize + x] = originalTexture_Colors[(y - offsetY) * width + (x - offsetX)];
                            //scaledTexture.SetPixel(x, y, texture.GetPixel(x - offsetX, y - offsetY));
                        }
                        else
                        {
                            scaledTexture_Colors[y * maxSize + x] = Color.clear;
                            //scaledTexture.SetPixel(x, y, Color.clear);
                        }
                    }
                }
            });

            scaledTexture.SetPixels32(scaledTexture_Colors);
            scaledTexture.Apply();

            return scaledTexture;
        }
    }
}