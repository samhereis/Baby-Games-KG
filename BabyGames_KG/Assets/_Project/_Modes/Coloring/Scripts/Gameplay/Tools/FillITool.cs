using PaintCore;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Modes.Coloring
{
    public class FillITool : ToolBase, ISelfValidator
    {
        [SerializeField] private CwPaintFill _p3DPaintFill;

        public void Validate(SelfValidationResult result)
        {
            _p3DPaintFill = GetComponent<CwPaintFill>();
        }

        public override void Init(ColorInfo colorInfo)
        {
            base.Init(colorInfo);

            CwBlendMode blendMode = _p3DPaintFill.BlendMode;
            blendMode.Color = colorInfo.color;
            _p3DPaintFill.Color = colorInfo.color;
            _p3DPaintFill.BlendMode.Channels.Set(1, 1, 1, 1);
        }

        public override void SetPaintable(Paintable_Identifier_Basic paintable)
        {

        }
    }
}