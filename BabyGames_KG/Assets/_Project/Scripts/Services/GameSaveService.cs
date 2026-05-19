using DataClasses;
using Helpers;
using Loggers;
using System;
using System.IO;
using System.Threading.Tasks;
using _Project.Scripts.Data;
using UnityEngine;

public class GameSaveService
{
    private const string _ICON_KEY = "icon";
    private const string _DEFAULT_ATLAS_KEY = "default_atlas";
    private const string _DRAWN_ATLAS_KEY = "drawing";
    private const string _SCREENSHOT_KEY = "schreenshot";
    private const string _BACKGROUND_DRAWING_KEY = "background";

    private string _dataPath = Application.persistentDataPath;

    public GameSaveService()
    {
        _dataPath = Application.persistentDataPath;
    }

    public (string folderName, string fileName) GetIconPath(string category, string picture)
    {
        string folderName = $"{_dataPath}/{category}/{picture}";
        string fileName = $"{folderName}/{picture}-{_ICON_KEY}.png";

        return (folderName, fileName);
    }

    public (string folderName, string fileName) GetDefaultAtlastPath(string category, string picture, int index = 0)
    {
        string folderName = $"{_dataPath}/{category}/{picture}";
        string fileName = $"{folderName}/{picture}-{_DEFAULT_ATLAS_KEY}_{index}.png";

        if (Directory.Exists(folderName) == false)
        {
            Directory.CreateDirectory(folderName);
        }

        return (folderName, fileName);
    }

    public (string folderName, string fileName) GetDrawingPath(string category, string picture, int index = 0)
    {
        string folderName = $"{_dataPath}/{category}/{picture}";
        string fileName = $"{folderName}/{picture}-{_DRAWN_ATLAS_KEY}_{index}.png";

        if (Directory.Exists(folderName) == false)
        {
            Directory.CreateDirectory(folderName);
        }

        return (folderName, fileName);
    }

    public (string folderName, string fileName) GetScreenshotPath(string category, string picture)
    {
        string folderName = $"{_dataPath}/{category}/{picture}";
        string fileName = $"{folderName}/{picture}-{_SCREENSHOT_KEY}.png";

        if (Directory.Exists(folderName) == false)
        {
            Directory.CreateDirectory(folderName);
        }

        return (folderName, fileName);
    }

    public (string folderName, string fileName) GetBackgroundDrawingPath(string category, string picture)
    {
        string folderName = $"{_dataPath}/{category}/{picture}";
        string fileName = $"{folderName}/{picture}-{_BACKGROUND_DRAWING_KEY}.png";

        if (Directory.Exists(folderName) == false)
        {
            Directory.CreateDirectory(folderName);
        }

        return (folderName, fileName);
    }

    public static Texture2D LoadPNG(string filePath)
    {
        Texture2D tex = null;
        byte[] fileData;
        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2);
            ClearablesHolder.instance.clearableTextures.SafeAdd(tex);

            string[] pages = filePath.Split('/');
            int page = pages.Length - 1;
            tex.name = pages[page].Split('.')[0];
            tex.LoadImage(fileData);
        }
        return tex;
    }

    public async void SetIcon(Texture2D src, string category, string picture)
    {
        try
        {
            string fileName = GetIconPath(category, picture).fileName;

            Texture2D scaledTexture = await TextureHelper.ScaleTextureToFitAndCenter(ClearablesHolder.instance.clearableTextures, src);
            scaledTexture = TextureHelper.ResizeIfNeeded(scaledTexture, 256, ClearablesHolder.instance.clearableTextures);

            byte[] iconBytes = scaledTexture.EncodeToPNG();
            File.WriteAllBytes(fileName, iconBytes);
        }
        catch (Exception exception)
        {
            CustomLogger.instance?.LogException(exception, "Error setting icon");
        }
    }

    public async Task<Sprite> GetIcon(string category, string picture)
    {
        Sprite sprite = null;

        try
        {
            string fileName = GetIconPath(category, picture).fileName;

            if (File.Exists(fileName) == false)
            {
                return null;
            }

            Texture2D icon = LoadPNG(fileName);
            ClearablesHolder.instance.clearableTextures.SafeAdd(icon);
            if (icon.width > 256 && icon.height > 256)
            {
                icon = TextureHelper.ResizeIfNeeded(icon, 256, ClearablesHolder.instance.clearableTextures);
                ClearablesHolder.instance.clearableTextures.SafeAdd(icon);
                SetIcon(icon, category, picture);
            }

            sprite = Sprite.Create(icon, new Rect(0, 0, icon.width, icon.height), new Vector2(0.5f, 0.5f), 100, 1, SpriteMeshType.FullRect);
            ClearablesHolder.instance.clearableSprites.SafeAdd(sprite);
        }
        catch (Exception exception)
        {
            CustomLogger.instance?.LogException(exception, "Error getting icon");
        }

        return sprite;
    }

    public void DeleteIcon(string category, string picture)
    {
        try
        {
            string fileName = GetIconPath(category, picture).fileName;
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }
        catch (Exception exception)
        {
            CustomLogger.instance?.LogException(exception, "Error deleting icon");
        }
    }

    public void SetDrawings(Texture2D src, string category, string picture, int index = 0)
    {
        try
        {
            string fileName = GetDrawingPath(category, picture, index).fileName;

            byte[] iconBytes = src.EncodeToPNG();
            File.WriteAllBytes(fileName, iconBytes);
        }
        catch (Exception exception)
        {
            CustomLogger.instance?.LogException(exception, "Error setting drawing");
        }
    }

    public Texture2D GetDrawing(string category, string picture, int index = 0)
    {
        Texture2D texture = null;

        try
        {
            string fileName = GetDrawingPath(category, picture, index).fileName;
            texture = LoadPNG(fileName);
        }
        catch (Exception exception)
        {
            CustomLogger.instance?.LogException(exception, "Error getting drawing");
        }

        ClearablesHolder.instance.clearableTextures.SafeAdd(texture);
        return texture;
    }

    public void DeleteDrawings(string category, string picture)
    {
        try
        {
            string folderName = GetDrawingPath(category, picture, 0).folderName;

            foreach (string file in Directory.GetFiles(folderName))
            {
                if (file.Contains(_DRAWN_ATLAS_KEY))
                {
                    File.Delete(file);
                }
            }
        }
        catch (Exception exception)
        {
            CustomLogger.instance?.LogException(exception, "Error deleting drawing");
        }
    }

    public Texture2D GetBackgroundDrawing(string category, string picture, int index = 0)
    {
        Texture2D texture = null;

        try
        {
            string fileName = GetBackgroundDrawingPath(category, picture).fileName;
            texture = LoadPNG(fileName);
        }
        catch (Exception exception)
        {
            CustomLogger.instance?.LogException(exception, "Error getting background");
        }

        ClearablesHolder.instance.clearableTextures.SafeAdd(texture);
        return texture;
    }

    public void SetBackgroundDrawing(Texture2D src, string category, string picture)
    {
        try
        {
            string fileName = GetBackgroundDrawingPath(category, picture).fileName;

            byte[] iconBytes = src.EncodeToPNG();
            File.WriteAllBytes(fileName, iconBytes);
        }
        catch (Exception exception)
        {
            CustomLogger.instance?.LogException(exception, "Error saving background");
        }
    }

    public void DeleteBackgroundDrawings(string category, string picture)
    {
        try
        {
            string folderName = GetBackgroundDrawingPath(category, picture).folderName;

            foreach (string file in Directory.GetFiles(folderName))
            {
                if (file.Contains(_BACKGROUND_DRAWING_KEY))
                {
                    File.Delete(file);
                }
            }
        }
        catch (Exception exception)
        {
            CustomLogger.instance?.LogException(exception, "Error deleting background drawing");
        }
    }

    public async Task SaveScreenshot(Texture2D src, string category, string picture)
    {
        await AsyncHelper.Skip();

        try
        {
            string fileName = GetScreenshotPath(category, picture).fileName;

            byte[] iconBytes = src.EncodeToPNG();
            File.WriteAllBytes(fileName, iconBytes);
        }
        catch (Exception exception)
        {
            CustomLogger.instance?.LogException(exception, "Error saving screenshot");
        }
    }
}