using UnityEngine;

namespace Modes.Coloring
{
    public static class Constants
    {
        public static string DRAWABLE_ATLAS_PART => "-back";
        public static string OUTLINE_ATLAS_PART => "-outline";

        public static LayerMask SNAPSHOTTABLE => LayerMask.NameToLayer("Snapshot");

        public static string CURRENT_CATEGORY => nameof(CURRENT_CATEGORY);
    }
}