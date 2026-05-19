using DataClasses.AssetReferences;
using UnityEngine;

namespace _Project._Modes.Hiding.Scripts.SO
{
    [CreateAssetMenu(fileName = nameof(Bubble_Data), menuName = "Scriptables/ModeData/" + nameof(Bubble_Data))]
    public class Bubble_Data : ScriptableObject
    {
        [field: SerializeField] public ExternalAssetReference<Material> _panelItemMaterial_Gray;
        [field: SerializeField] public ExternalAssetReference<Material> _panelItemMaterial_Normal;
    }
}