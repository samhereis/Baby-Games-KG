using DataClasses;
using Helpers;
using Identifiers;
using Loggers;
using PaintCore;
using Services;
using Sirenix.OdinInspector;
using System;
using System.Threading.Tasks;
using _Project.Scripts.Data;
using UnityEngine;
using Zenject;

namespace Modes.Coloring
{
    public class BackgroundPage_Identifier : IdentifierBase
    {
        [field: SerializeField] public Paintable_Identifier_Background _paintableIdentifier { get; private set; }
        [field: SerializeField]  public Glitter_Identifier _glitterIdentifier { get; private set; }

        [Inject] private GameController _gameController;

        [field: SerializeField] public Texture2D texture { get; private set; }

        public Texture2D GetTexture()
        {
            Texture2D texture = null;

            if (TryGet<MeshRenderer>(out var meshRenderer))
            {
                if (meshRenderer.sharedMaterial != null)
                {
                    if (meshRenderer.sharedMaterial.mainTexture != null)
                    {
                        texture = meshRenderer.sharedMaterial.mainTexture.MakeTexture2D(ClearablesHolder.instance.clearableTextures);
                    }
                }
            }

            return texture;
        }

        [Button]
        public async void InitBackgroundPage(Texture2D backgroundDrawing)
        {
            _paintableIdentifier = Get<Paintable_Identifier_Background>();
            _glitterIdentifier = Get<Glitter_Identifier>();

            Get<MeshRenderer>().sortingLayerName = "Spine";

            try
            {
                DiService.Inject(this);

                if (backgroundDrawing == null)
                {
                    backgroundDrawing = new Texture2D(Screen.width, Screen.height);
                    ClearablesHolder.instance.clearableTextures.SafeAdd(backgroundDrawing);

                    Color[] colors = backgroundDrawing.GetPixels();
                    Color color = Color.white;

                    float x = _gameController.model.gameSettings.backgroundColorX;
                    float y = _gameController.model.gameSettings.backgroundColorY;
                    float z = _gameController.model.gameSettings.backgroundColorZ;
                    float a = _gameController.model.gameSettings.backgroundColorA;
                    color = new Color(x, y, z, a);

                    color.a = _gameController.model.gameSettings.nonGlitterAlpha;

                    await Task.Run(() =>
                    {
                        for (int i = 0; i < colors.Length; i++)
                        {
                            colors[i] = color;
                        }
                    });

                    backgroundDrawing.SetPixels(colors);
                    backgroundDrawing.Apply();
                }

                SetTexture(backgroundDrawing);
                Scale();

                _paintableIdentifier?.Init(_gameController.model.currentSpine, null);
                _glitterIdentifier?.Init(_gameController, _gameController.model.content.glitterMaterial_Drawable_Background);
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex, "Could not setbackground drawing");
            }
        }

        private void SetTexture(Texture2D backgroundDrawing)
        {
            Get<CwPaintableTexture>().SetTexture(backgroundDrawing);
        }

        [Button]
        private void Scale()
        {
            float width = Screen.width;
            if (width < 2000) { width = 2000; }

            float height = Screen.height;
            if (height < 1200) { height = 1200; }

            Vector3 scale = new Vector3(width * 0.01f, height * 0.01f, 1);

            if (TryGet<PolygonCollider2D>(out var polygonCollider)) { polygonCollider.transform.localScale = scale; }
            if (TryGet<MeshRenderer>(out var meshRenderer)) { meshRenderer.transform.localScale = scale; }
        }
    }
}