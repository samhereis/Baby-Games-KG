using PaintCore;
using PaintIn3D;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Modes.Coloring
{
    public class TextureTool : ToolBase, ISelfValidator
    {
        [SerializeField] private CwPaintSphere _paintSphere;

        public void Validate(SelfValidationResult result)
        {
            _paintSphere = GetComponent<CwPaintSphere>();
        }

        public override void Init(ColorInfo colorInfo)
        {
            base.Init(colorInfo);

            if (colorInfo == null) { return; }

            if (colorInfo.pattern != null) { _paintSphere.TileTexture = colorInfo.pattern.texture; }
            _paintSphere.Color = colorInfo.color;

            _paintSphere.BlendMode.Channels.Set(1, 1, 1, 0);
        }

        public override void SetPaintable(Paintable_Identifier_Basic paintable)
        {
            if (paintable != null && paintable.TryGet<CwPaintableTexture>(out var cwPaintable))
            {
                _paintSphere.TargetTexture = cwPaintable;
            }
            else
            {
                _paintSphere.TargetTexture = null;
            }
        }
    }
}