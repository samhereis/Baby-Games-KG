using PaintCore;
using PaintIn3D;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Modes.Coloring
{
    public class MarkerTool : ToolBase, ISelfValidator
    {
        [SerializeField] private CwPaintSphere _p3DPaintSphere;

        public void Validate(SelfValidationResult result)
        {
            _p3DPaintSphere = GetComponent<CwPaintSphere>();
        }

        public override void Init(ColorInfo colorInfo)
        {
            base.Init(colorInfo);

            _p3DPaintSphere.Color = colorInfo.color;
            _p3DPaintSphere.BlendMode.Channels.Set(1, 1, 1, 0);
        }

        public override void SetPaintable(Paintable_Identifier_Basic paintable)
        {
            if (paintable != null && paintable.TryGet<CwPaintableTexture>(out var cwPaintable))
            {
                _p3DPaintSphere.TargetTexture = cwPaintable;
            }
            else
            {
                _p3DPaintSphere.TargetTexture = null;
            }
        }
    }
}