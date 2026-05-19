using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Modes.Coloring
{
    [CreateAssetMenu(fileName = "Instruments", menuName = "ScriptableObjects/Instruments")]
    public class InstrumentSO : ScriptableObject, ISelfValidator
    {
        public ToolType toolType;

        public List<ColorInfo> colorInfo = new();

        public void Validate(SelfValidationResult result)
        {
            for (int i = 0; i < colorInfo.Count; i++)
            {
                colorInfo[i].index = i;
            }
        }

#if UNITY_EDITOR
        public List<Sprite> sprites = new();
        public float globalAlpha = 1;
        public Color globalColor = Color.white;
        public Vector2Int colorCenter;

        [Button]
        private void SetGlobalAlpha()
        {
            foreach (var colorInfo in colorInfo)
            {
                colorInfo.color.a = globalAlpha;
            }
        }

        [Button]
        private void SetGlobalColor()
        {
            foreach (var colorInfo in colorInfo)
            {
                colorInfo.color = globalColor;
            }
        }

        [Button]
        private void AutoFull_Brush()
        {
            colorInfo.Clear();

            for (int i = 0; i < sprites.Count; i++)
            {
                ColorInfo newItem = new ColorInfo();

                if (sprites[i].name == "_color") { continue; }

                newItem.index = i;
                newItem.instrumentIcon = sprites.Find(x => x.name == $"brush_{newItem.index}");
                newItem.paletteIcon = sprites.Find(x => x.name == $"_color");

                colorInfo.Add(newItem);
            }
        }

        [Button]
        private void AutoFull_Fill()
        {
            colorInfo.Clear();

            for (int i = 0; i < sprites.Count; i++)
            {
                ColorInfo newItem = new ColorInfo();

                if (sprites[i].name == "_color") { continue; }

                newItem.index = i;
                newItem.paletteIcon = sprites.Find(x => x.name == $"_color");
                newItem.instrumentIcon = sprites.Find(x => x.name == $"paint_{newItem.index}");

                colorInfo.Add(newItem);
            }
        }

        [Button]
        private void AutoFull_Marker()
        {
            colorInfo.Clear();

            for (int i = 0; i < sprites.Count; i++)
            {
                ColorInfo newItem = new ColorInfo();

                if (sprites[i].name == "_color") { continue; }

                newItem.index = i;
                newItem.instrumentIcon = sprites.Find(x => x.name == $"marker_{newItem.index}");
                newItem.paletteIcon = sprites.Find(x => x.name == $"_color");

                colorInfo.Add(newItem);
            }
        }

        [Button]
        private void AutoFull_Pattern()
        {
            colorInfo.Clear();

            for (int i = 0; i < (sprites.Count / 2); i++)
            {
                ColorInfo newItem = new ColorInfo();

                newItem.index = i;
                newItem.pattern = sprites.Find(x => x.name == $"tex{newItem.index + 1}");
                newItem.paletteIcon = sprites.Find(x => x.name == $"tex_for pallete{newItem.index + 1}");

                colorInfo.Add(newItem);
            }
        }

        [Button]
        private void AutoFull_Shine()
        {
            //foreach (var item in colorInfo)
            //{
            //    if (item.pattern == null) { continue; }
            //
            //    string assetPath = AssetDatabase.GetAssetPath(item.pattern);
            //    AssetDatabase.RenameAsset(assetPath, $"Glitter_Texture_{item.index - 1}");
            //    AssetDatabase.SaveAssets();
            //    AssetDatabase.Refresh();
            //}
            //
            //return;

            colorInfo.Clear();

            for (int i = 0; i < (sprites.Count / 3); i++)
            {
                ColorInfo newItem = new ColorInfo();

                newItem.index = i;
                newItem.pattern = sprites.Find(x => x.name == $"Glitter_Texture_{newItem.index + 1}");
                newItem.paletteIcon = newItem.pattern; // sprites.Find(x => x.name == $"glitter_icon_{newItem.index}");
                newItem.instrumentIcon = sprites.Find(x => x.name == $"glitter_instrument_{newItem.index}");

                colorInfo.Add(newItem);
            }
        }

        [Button]
        private void AutoFull_Spray()
        {
            colorInfo.Clear();

            for (int i = 0; i < sprites.Count - 1; i++)
            {
                ColorInfo newItem = new ColorInfo();

                newItem.index = i;
                newItem.paletteIcon = sprites.Find(x => x.name == $"_color");
                newItem.instrumentIcon = sprites.Find(x => x.name == $"spray_{i + 1}");

                colorInfo.Add(newItem);
            }
        }

        [Button]
        private void AutoFull_Stiker()
        {
            colorInfo.Clear();

            for (int i = 0; i < sprites.Count - 1; i++)
            {
                ColorInfo newItem = new ColorInfo();

                newItem.index = i;
                newItem.pattern = sprites.Find(x => x.name == $"baby sticker_finish{i + 1}");

                colorInfo.Add(newItem);
            }
        }

        [Button]
        private void SetPaletteIconsColor_PaletteIcon()
        {
            foreach (var colorInfo in colorInfo)
            {
                colorInfo.SetPaletteIconsColor(colorInfo.paletteIcon.texture, colorCenter.x, colorCenter.y);
            }
        }

        [Button]
        private void SetPaletteIconsColor_InstrumentIcon()
        {
            foreach (var colorInfo in colorInfo)
            {
                colorInfo.SetPaletteIconsColor(colorInfo.instrumentIcon.texture, colorCenter.x, colorCenter.y);
            }
        }

        [Button]
        private void SetPaletteIconsColor_Pattern()
        {
            foreach (var colorInfo in colorInfo)
            {
                colorInfo.SetPaletteIconsColor(colorInfo.pattern.texture, colorCenter.x, colorCenter.y);
            }
        }
#endif
    }
}