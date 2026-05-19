using PaintCore;
using PaintIn3D;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Modes.Coloring
{
    public class GlitterTool : ToolBase, ISelfValidator
    {
        [SerializeField] private CwPaintDecal _paintSphere;

        public void Validate(SelfValidationResult result)
        {
            _paintSphere = GetComponent<CwPaintDecal>();
        }

        public override void Init(ColorInfo colorInfo)
        {
            base.Init(colorInfo);

            if (colorInfo == null)
            {
                return;
            }

            if (colorInfo.pattern != null)
            {
                _paintSphere.Texture = colorInfo.pattern.texture;
                _paintSphere.Shape = colorInfo.pattern.texture;
                _paintSphere.TileTexture = colorInfo.pattern.texture;
            }
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