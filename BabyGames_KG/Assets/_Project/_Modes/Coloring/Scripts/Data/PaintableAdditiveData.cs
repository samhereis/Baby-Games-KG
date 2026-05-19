using Sirenix.OdinInspector;
using Spine.Unity;
using System;
using System.Collections.Generic;

namespace Modes.Coloring
{
    [Serializable]
    public class PaintableAdditiveData
    {
        [ShowInInspector] public Dictionary<SkeletonPartsRenderer, int> additivesSortingOrders = new();
    }
}