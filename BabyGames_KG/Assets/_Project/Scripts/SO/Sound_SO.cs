using Sounds;
using UnityEngine;

namespace SO.Lists
{
    [CreateAssetMenu(fileName = nameof(Sound_SO), menuName = "Scriptables/" + nameof(Sound_SO))]
    public class Sound_SO : ScriptableObject
    {
        [field: SerializeField] public Sound sound;
    }
}