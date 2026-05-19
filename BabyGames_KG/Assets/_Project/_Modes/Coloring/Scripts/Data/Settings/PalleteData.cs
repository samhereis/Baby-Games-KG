using Data;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modes.Coloring
{
    [Serializable]
    public class PalleteData
    {
        [SerializeField] private List<PalleteInstrumentData> _palleteDefaults = new();

        public IReadOnlyList<PalleteInstrumentData> palleteDefaults => _palleteDefaults;
    }
}