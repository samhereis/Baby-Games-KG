using DataClasses;
using Helpers;
using Services;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using _Project.Scripts.Data;
using UnityEngine;

namespace Modes.Coloring
{
    public class Overrode_MirrorTexture : MonoBehaviour
    {
        public enum MirrorMode { DominantColor, CopyTexture }

        [SerializeField] private MirrorMode _mirrorMode = MirrorMode.DominantColor;
        [SerializeField] private List<Paintable_Identifier_SlotSeparation> _copyTos = new();

        [Space]
        [SerializeField] private Color _color;

        private Paintable_Identifier_SlotSeparation _paintable_Identifier;

        public void SetCopyiables(List<Paintable_Identifier_SlotSeparation> copyTos, MirrorMode mirrorMode)
        {
            _copyTos = copyTos;
            _mirrorMode = mirrorMode;
        }

        public void Initialize()
        {
            DiService.Inject(this);

            _paintable_Identifier = GetComponent<Paintable_Identifier_SlotSeparation>();
        }

        public void Mirror()
        {
            if (_mirrorMode == MirrorMode.DominantColor)
            {
                foreach (var item in _copyTos)
                {
                    CopyDominantColor(item);
                }
            }

            if (_mirrorMode == MirrorMode.CopyTexture)
            {
                CopyTexture();
            }
        }

        [Button]
        private async void CopyDominantColor(Paintable_Identifier_SlotSeparation copyTo)
        {
            var _bigTexture = copyTo.paintableTexture.Texture.MakeTexture2D(ClearablesHolder.instance.clearableTextures);
            var _mySmallTexture = _paintable_Identifier.GetOutputTexture();
            var _otherSmallTexture = copyTo.GetOutputTexture();

            _color = await _paintable_Identifier.paintableTexture.GetDominantColor(_mySmallTexture);
            await _otherSmallTexture.SetColor(_color);

            int x = Mathf.FloorToInt(copyTo.atlasRegion.x);
            int y = Mathf.FloorToInt(copyTo.atlasRegion.page.height - copyTo.atlasRegion.y - copyTo.atlasRegion.height);
            int sizeX = copyTo.atlasRegion.page.width;
            int sizeY = copyTo.atlasRegion.page.height;

            try
            {
                Graphics.CopyTexture(_otherSmallTexture, 0, 0, 0, 0, _otherSmallTexture.width, _otherSmallTexture.height, _bigTexture, 0, 0, x, y);
                copyTo.paintableTexture.SetTexture(_bigTexture);

                Debug.Log("Texture copy operation completed successfully!");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during Graphics.CopyTexture: {ex.Message}");
            }

            Destroy(_mySmallTexture);
            Destroy(_otherSmallTexture);
        }

        [Button]
        private async void CopyTexture()
        {
            try
            {
                var _bigTexture = _paintable_Identifier.paintableTexture.Texture.MakeTexture2D(ClearablesHolder.instance.clearableTextures);
                var _mySmallTexture = _paintable_Identifier.GetOutputTexture();

                foreach (var item in _copyTos)
                {
                    var _otherSmallTexture = item.GetOutputTexture();
                    await _mySmallTexture.CopyTexture(_otherSmallTexture);

                    int x = Mathf.FloorToInt(item.atlasRegion.x);
                    int y = Mathf.FloorToInt(item.atlasRegion.page.height - item.atlasRegion.y - item.atlasRegion.height);
                    int sizeX = item.atlasRegion.page.width;
                    int sizeY = item.atlasRegion.page.height;

                    Graphics.CopyTexture(_otherSmallTexture, 0, 0, 0, 0, _otherSmallTexture.width, _otherSmallTexture.height, _bigTexture, 0, 0, x, y);
                    item.paintableTexture.SetTexture(_bigTexture);

                    Destroy(_otherSmallTexture);
                }

                Destroy(_mySmallTexture);
                Debug.Log("Texture copy operation completed successfully!");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during Graphics.CopyTexture: {ex.Message}");
            }
        }
    }
}