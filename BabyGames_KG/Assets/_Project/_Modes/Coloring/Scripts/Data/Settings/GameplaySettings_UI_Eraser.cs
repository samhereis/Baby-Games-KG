using System;
using UnityEngine;

namespace Modes.Coloring
{
    [Serializable]
    public class GameplaySettings_UI_Eraser
    {
        public Sprite onSprite;
        public Sprite offSprite;
        public float offAlpha = 0.5f;
        public float onOnYPosition;
        public float onOffYPosition;
    }
}