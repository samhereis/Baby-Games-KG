using DataClasses.AssetReferences;
using UnityEngine;

namespace _Project._Modes.Orchestra.Scripts.SO
{
    [CreateAssetMenu(fileName = nameof(Puzzle_Data), menuName = "Scriptables/ModeData/" + nameof(Puzzle_Data))]
    public class Puzzle_Data : ScriptableObject
    {
        [field: SerializeField] public ExternalAssetReference_HasComponent<ParticleSystem> confetti;
    }
}