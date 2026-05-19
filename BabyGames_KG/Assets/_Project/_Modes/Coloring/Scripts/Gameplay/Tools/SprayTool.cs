using PaintCore;
using PaintIn3D;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Modes.Coloring
{
    public class SprayTool : ToolBase, ISelfValidator
    {
        [SerializeField] private CwPaintDecal _p3DPaintDecal;

        public void Validate(SelfValidationResult result)
        {
            _p3DPaintDecal = GetComponent<CwPaintDecal>();
        }

        public override void Init(ColorInfo colorInfo)
        {
            base.Init(colorInfo);

            CwBlendMode blendMode = _p3DPaintDecal.BlendMode;
            blendMode.Color = colorInfo.color;
            blendMode.Channels.Set(1, 1, 1, 1);

            _p3DPaintDecal.BlendMode = blendMode;
            _p3DPaintDecal.Color = colorInfo.color;
        }

        public override void SetPaintable(Paintable_Identifier_Basic paintable)
        {
            if (paintable != null && paintable.TryGet<CwPaintableTexture>(out var cwPaintable))
            {
                _p3DPaintDecal.TargetTexture = cwPaintable;
            }
            else
            {
                _p3DPaintDecal.TargetTexture = null;
            }
        }
    }
}