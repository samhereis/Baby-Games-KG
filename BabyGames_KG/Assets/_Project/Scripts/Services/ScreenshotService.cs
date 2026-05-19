using DataClasses;
using Helpers;
using Modes.Coloring;
using Services;
using System;
using _Project.Scripts.Data;
using UnityEngine;

namespace Saratan.Coloring
{
    [Serializable]
    public class ScreenshotService
    {
        public Action onStartScreenshoting_Icon;
        public Action<Texture2D> onEndScreenshoting_Icon;

        public Rect rect;
        private Camera _snapCamera;
        private Gameplay_GameState_Coloring_Model _model;

        public ScreenshotService(Gameplay_GameState_Coloring_Model newModel)
        {
            _model = newModel;

            DiService.Inject(this);

            _snapCamera = GameObject.Find("SnapCamera").GetComponent<Camera>();
            _snapCamera.gameObject.SetActive(false);

            rect = new Rect(0, 0, Screen.width - 300, Screen.height);
        }

        public Texture2D GetSnapshot(string textureName, LayerMask layerMask)
        {
            _snapCamera.cullingMask = layerMask;
            RenderTexture rt = new RenderTexture((int)rect.width, (int)rect.height, 32);
            ClearablesHolder.instance.clearableTextures.SafeAdd(rt);

            _snapCamera.targetTexture = rt;
            RenderTexture.active = rt;
            _snapCamera.Render();

            var shot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
            ClearablesHolder.instance.clearableTextures.SafeAdd(shot);

            shot.name = textureName;
            shot.wrapMode = TextureWrapMode.Clamp;

            shot.ReadPixels(rect, 0, 0);
            shot.Apply();

            RenderTexture.active = null;
            _snapCamera.targetTexture = null;
            rt.Release();

            //shot = TextureHelper.AddBorderWithRoundedCorners(shot, _model.gameSettings.snapshot_BorderWith, _model.gameSettings.snapshot_Color, _model.gameSettings.snapshot_CornerRadius, ClearablesHolder.instance.clearableTextures);

            return shot;
        }

        public Texture2D GetIcon(string textureName, LayerMask layerMask)
        {
            onStartScreenshoting_Icon?.Invoke();

            _snapCamera.cullingMask = layerMask;
            RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 32);
            ClearablesHolder.instance.clearableTextures.SafeAdd(rt);

            _snapCamera.targetTexture = rt;
            RenderTexture.active = rt;
            _snapCamera.Render();

            var shot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);
            ClearablesHolder.instance.clearableTextures.SafeAdd(shot);

            shot.name = textureName;
            shot.wrapMode = TextureWrapMode.Clamp;

            shot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            shot.Apply();

            RenderTexture.active = null;
            _snapCamera.targetTexture = null;
            rt.Release();

            onEndScreenshoting_Icon?.Invoke(shot);

            return shot;
        }
    }
}
