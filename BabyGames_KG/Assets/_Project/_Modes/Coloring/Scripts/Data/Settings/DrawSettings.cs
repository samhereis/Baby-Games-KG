using Saratan.Coloring;
using System;
using System.Collections.Generic;
using static PaintIn3D.CwHitScreen;

namespace Modes.Coloring
{
    [Serializable]
    public class DrawSettings
    {
        [Serializable]
        public class DrawSettings_HitData { public ToolType toolType; public FrequencyType frequencyType; public int interval = 20; }

        public int undoMaxCount = 5;
        public List<DrawSettings_HitData> toolHitData = new List<DrawSettings_HitData>();

        public string[] settings_float = new string[]
        {
            "_GlitterAlpha",
            "_SparklesNoiseScale",
            "_SparklesNoiseScale_Multiplier",
            "_SparklesPower",
            "_SparklesSpeed",
            "_StarsNoiseScale",
            "_StarsNoiseScale_Multiplier",
            "_StarsPower",
            "_StarsSpeed",
            "_ZWrite",
            "_ZWriteControl",
            "_ZTest",
            "_QueueControl"
        };
    }
}