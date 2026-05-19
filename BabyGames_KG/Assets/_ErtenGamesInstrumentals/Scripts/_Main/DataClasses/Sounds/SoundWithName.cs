using SO;
using System;
using UnityEngine;

namespace Sounds
{
    [Serializable]
    public class SoundWithName : Sound
    {
        [field: SerializeField] public Sound_String_SO soundName { get; private set; }
    }
}