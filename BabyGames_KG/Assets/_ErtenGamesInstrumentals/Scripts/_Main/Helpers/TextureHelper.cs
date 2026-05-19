using Helpers.ImageCroppers;
using Loggers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Helpers
{
    public static class TextureHelper
    {
        public static Texture2D MakeTexture2D(this Texture texture, List<Texture> juntList)
        {
            if (texture == null)
            {
                return null;
            }

            if (texture is Texture2D)
            {
                return texture as Texture2D;
            }

            return MakeTexture2D(texture, new Rect(0, 0, texture.width, texture.height), juntList);
        }

        public static Texture2D MakeTexture2D(this Texture texture, Rect source, List<Texture> juntList)
        {
            if (texture == null)
            {
                return null;
            }

            RenderTexture active = RenderTexture.active;
            RenderTexture renderTexture = (RenderTexture.active = RenderTexture.GetTemporary(texture.width, texture.height));
            juntList.Add(renderTexture);

            bool sRGBWrite = GL.sRGBWrite;
            GL.sRGBWrite = false;
            GL.Clear(clearDepth: false, clearColor: true, new Color(1f, 1f, 1f, 1f));
            Graphics.Blit(texture, renderTexture);

            Texture2D copy = new Texture2D(texture.width, texture.height, texture.graphicsFormat, TextureCreationFlags.None);
            juntList.Add(renderTexture);

            copy.wrapMode = texture.wrapMode;
            copy.filterMode = texture.filterMode;
            copy.anisoLevel = texture.anisoLevel;

            copy.ReadPixels(source, 0, 0);
            copy.Apply();
            GL.sRGBWrite = sRGBWrite;
            RenderTexture.active = active;
            RenderTexture.ReleaseTemporary(renderTexture);

            copy.name = texture.name;

            return copy;
        }

        public static async Task<Texture2D> ScaleTextureToFitAndCenter(List<Texture> juntList, Texture2D originalTexture, int xPadding = 0, int yPadding = 0)
        {
            if (originalTexture == null)
            {
                return null;
            }

            var texture = await ImageCropper_Implementation2.CropAlphas_Square(originalTexture, xPadding, yPadding);
            juntList?.SafeAdd(texture);

            var centered = await ImageCropper_Implementation2.ScaleTextureToCenter(texture);
            juntList?.SafeAdd(centered);

            return centered;
        }


        public static Texture2D AddBorderWithRoundedCorners(
            Texture2D originalTexture,
            int borderWidth,
            Color borderColor,
            int outerCornerRadius,
            List<Texture> juntList = null)
        {
            int innerCornerRadius = Mathf.Max(0, outerCornerRadius - borderWidth);
            return AddBorderWithRoundedCorners(originalTexture, borderWidth, borderColor, outerCornerRadius, innerCornerRadius, juntList);
        }

        public static Texture2D AddBorderWithRoundedCorners(
            Texture2D originalTexture,
            int borderWidth,
            Color borderColor,
            int outerCornerRadius,
            int innerCornerRadius,
            List<Texture> juntList = null)
        {
            int srcW = originalTexture.width;
            int srcH = originalTexture.height;

            int newW = srcW + 2 * borderWidth;
            int newH = srcH + 2 * borderWidth;

            var tex = new Texture2D(newW, newH, originalTexture.format, originalTexture.mipmapCount > 1);
            juntList?.SafeAdd(tex);

            tex.wrapMode = originalTexture.wrapMode;
            tex.filterMode = originalTexture.filterMode;
            tex.anisoLevel = originalTexture.anisoLevel;

            var pixels = new Color[newW * newH];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

            var src = originalTexture.GetPixels();
            for (int y = 0; y < srcH; y++)
            {
                int dstRow = (y + borderWidth) * newW;
                int srcRow = y * srcW;
                for (int x = 0; x < srcW; x++)
                    pixels[dstRow + (x + borderWidth)] = src[srcRow + x];
            }

            int oL = 0, oB = 0, oR = newW - 1, oT = newH - 1;
            int iL = borderWidth, iB = borderWidth, iR = newW - 1 - borderWidth, iT = newH - 1 - borderWidth;

            int oR2 = outerCornerRadius * outerCornerRadius;
            int iR2 = innerCornerRadius * innerCornerRadius;

            bool InsideRoundedRect(int x, int y, int L, int B, int R, int T, int rad, int r2)
            {
                if (rad <= 0) return (x >= L && x <= R && y >= B && y <= T);

                int cx = Mathf.Clamp(x, L + rad, R - rad);
                int cy = Mathf.Clamp(y, B + rad, T - rad);
                int dx = x - cx;
                int dy = y - cy;
                return (dx * dx + dy * dy) <= r2;
            }

            for (int y = 0; y < newH; y++)
            {
                int row = y * newW;
                for (int x = 0; x < newW; x++)
                {
                    bool inOuter = InsideRoundedRect(x, y, oL, oB, oR, oT, outerCornerRadius, oR2);
                    if (!inOuter) continue;

                    bool inInner = InsideRoundedRect(x, y, iL, iB, iR, iT, innerCornerRadius, iR2);
                    if (inInner) continue; // carve the inner rounded rect out

                    pixels[row + x] = borderColor;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        private static bool IsWithinRoundedCorner(int x, int y, int width, int height, int borderWidth, int cornerRadius)
        {
            int cornerX = 0, cornerY = 0;

            if (x < borderWidth + cornerRadius)
                cornerX = borderWidth + cornerRadius - x;
            else if (x >= width - borderWidth - cornerRadius)
                cornerX = x - (width - borderWidth - cornerRadius);

            if (y < borderWidth + cornerRadius)
                cornerY = borderWidth + cornerRadius - y;
            else if (y >= height - borderWidth - cornerRadius)
                cornerY = y - (height - borderWidth - cornerRadius);

            return cornerX * cornerX + cornerY * cornerY >= cornerRadius * cornerRadius;
        }

        public static async Task SetColor(this Texture2D texture, Color color)
        {
            Color[] pixels = texture.GetPixels();

            await Task.Run(() =>
            {
                for (int i = 0; i < pixels.Length; i++)
                {
                    if (pixels[i].a < 0.05)
                    {
                        continue;
                    }

                    pixels[i] = color;
                }
            });

            texture.SetPixels(pixels);
            texture.Apply();
        }

        public static async Task CopyTexture(this Texture2D copyFrom, Texture2D copyTo)
        {
            try
            {
                Color[] copyFromColors = copyFrom.GetPixels();
                Color[] copyToColors = copyTo.GetPixels();

                await Task.Run(() =>
                {
                    for (int i = 0; i < copyToColors.Length; i++)
                    {
                        if (copyToColors[i].a < 0.5)
                        {
                            continue;
                        }

                        copyToColors[i] = copyFromColors[i];
                    }
                });

                copyTo.SetPixels(copyToColors);
                copyTo.Apply();
            }
            catch (Exception e)
            {
                CustomLogger.instance?.LogError("Error copying texture: ", e.Message, null, LogTypes.General);
            }
        }

        public static async Task<string> SaveTextureAsPNG(Texture2D texture, string folderPath, string fileName)
        {
            if (texture == null || string.IsNullOrEmpty(folderPath))
            {
                Debug.LogError("Invalid texture or folder path.");
                return null;
            }

#if UNITY_EDITOR
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            fileName = fileName + ".png";
            string filePath = Path.Combine(folderPath, fileName);

            byte[] bytes = texture.EncodeToPNG();
            await File.WriteAllBytesAsync(filePath, bytes);

            AssetDatabase.Refresh();

            return filePath;
#else
            Debug.LogError("Saving textures is only supported in the Unity Editor.");
            return null;
#endif
        }

        public static Texture2D GetReadableCopy_NewFormat(this Texture2D texture)
        {
            Texture2D copy = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, texture.mipmapCount > 1);
            copy.wrapMode = texture.wrapMode;
            copy.filterMode = texture.filterMode;
            copy.anisoLevel = texture.anisoLevel;

            Graphics.CopyTexture(texture, copy);

            return copy;
        }

        public static Texture2D GetReadableCopy(this Texture2D texture)
        {
            Texture2D copy = new Texture2D(texture.width, texture.height, texture.format, texture.mipmapCount > 1);
            copy.wrapMode = texture.wrapMode;
            copy.filterMode = texture.filterMode;
            copy.anisoLevel = texture.anisoLevel;

            Graphics.CopyTexture(texture, copy);

            return copy;
        }

        public static async Task ClampAlphas(this Texture2D texture, float minAlpha = 0.25f)
        {
            Color[] pixels = texture.GetPixels();

            await Task.Run(() =>
            {
                for (int i = 0; i < pixels.Length; i++)
                {
                    if (pixels[i].a < minAlpha)
                    {
                        pixels[i].a = 0;
                    }
                    else
                    {
                        pixels[i].a = 1;
                    }
                }
            });

            texture.SetPixels(pixels);
            texture.Apply();
        }

        public static Texture2D ResizeIfNeeded(Texture2D source, int maxSize, List<Texture> clearables)
        {
            clearables.SafeAdd(source);

            float scale = Mathf.Min((float)maxSize / source.width, (float)maxSize / source.height);
            int newWidth = Mathf.Max(1, Mathf.RoundToInt(source.width * scale));
            int newHeight = Mathf.Max(1, Mathf.RoundToInt(source.height * scale));

            RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(source, rt);

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;

            Texture2D resized = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);
            resized.name = source.name;
            resized.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
            resized.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);

            clearables.SafeAdd(resized);

            return resized;
        }
    }
}