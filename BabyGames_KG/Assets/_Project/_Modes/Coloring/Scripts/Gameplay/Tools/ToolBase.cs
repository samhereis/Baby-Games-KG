using Loggers;
using PaintIn3D;
using Services;
using System;
using UnityEngine;
using Zenject;

namespace Modes.Coloring
{
    public abstract class ToolBase : MonoBehaviour
    {
        [SerializeField] private ToolType _type = ToolType.Brush;

        public ToolType type { get => _type; }

        [Inject] protected GameController _gameController;

        public virtual void Init(ColorInfo colorInfo)
        {
            try
            {
                DiService.Inject(this);
                if (_gameController.model.gameSettings == null) { return; }

                CwHitScreen p3DHitScreen = GetComponent<CwHitScreen>();
                if (p3DHitScreen == null) { return; }

                DrawSettings.DrawSettings_HitData drawSettings_HitData = _gameController.model.gameSettings.drawSettings.toolHitData.Find(x => x.toolType == _type);
                if (drawSettings_HitData == null) { return; }

                GetComponent<CwHitScreen>().Frequency = drawSettings_HitData.frequencyType;
                GetComponent<CwHitScreen>().Interval = drawSettings_HitData.interval;
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex, $"Could not appy draw data ({gameObject.name}): ");
            }
        }

        public abstract void SetPaintable(Paintable_Identifier_Basic paintableTexture);
    }
}