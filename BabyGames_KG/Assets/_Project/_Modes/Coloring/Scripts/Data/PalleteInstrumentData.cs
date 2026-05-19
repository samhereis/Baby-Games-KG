using Saratan.Coloring;
using System;
using UnityEngine;

namespace Modes.Coloring
{
    [Serializable]
    public class PalleteInstrumentData
    {
        [field: SerializeField] public ToolType toolType { get; private set; }
        [field: SerializeField] public int index { get; private set; }
    }
}