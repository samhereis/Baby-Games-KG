using PaintCore;
using PaintIn3D;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Modes.Coloring
{
    public class StikerTool : ToolBase, ISelfValidator
    {
        [SerializeField] private CwPaintDecal _paint;
        [SerializeField] private CwHitScreen _hitScreen;

        public void Validate(SelfValidationResult result)
        {
            _paint = GetComponent<CwPaintDecal>();
            _hitScreen = GetComponent<CwHitScreen>();
        }

        public override void Init(ColorInfo colorInfo)
        {
            base.Init(colorInfo);

            _paint.Radius = _gameController.model.gameSettings.decalsRadius;
            _hitScreen.Connector.HitSpacing = _gameController.model.gameSettings.decalsHitSpacing;

            if (colorInfo == null) { return; }

            if (colorInfo.pattern != null)
            {
                _paint.Texture = colorInfo.pattern.texture; _paint.Shape = colorInfo.pattern.texture;
            }
            _paint.Color = colorInfo.color;
            _paint.BlendMode.Channels.Set(1, 1, 1, 0);
        }

        public override void SetPaintable(Paintable_Identifier_Basic paintable)
        {
            if (paintable != null && paintable.TryGet<CwPaintableTexture>(out var cwPaintable))
            {
                _paint.TargetTexture = cwPaintable;
            }
            else
            {
                _paint.TargetTexture = null;
            }
        }
    }
}