using System;
using UnityEngine;

namespace Sounds
{
    [Serializable]
    public class SoundData
    {
        [field: SerializeField] public bool isMain { get; private set; } = false;
        [field: SerializeField] public float delay { get; private set; } = 0;
        [field: SerializeField] public bool loop { get; private set; } = false;
        [field: SerializeField] public bool disableOthers { get; private set; } = false;
        [field: SerializeField] public float volume { get; private set; } = 1;
        [field: SerializeField] public float maxDistance { get; private set; } = 50;
        [field: SerializeField] public float minDistance { get; private set; } = 0;
    }
}