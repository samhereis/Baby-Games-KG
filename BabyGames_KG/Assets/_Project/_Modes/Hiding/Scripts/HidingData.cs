using DataClasses.AssetReferences;
using UnityEngine;

namespace _Project._Modes.Hiding.Scripts.SO
{
    [CreateAssetMenu(fileName = nameof(Hiding_Data), menuName = "Scriptables/ModeData/" + nameof(Hiding_Data))]
    public class Hiding_Data : ScriptableObject
    {
        [field: SerializeField] public ExternalAssetReference<Material> _panelItemMaterial_Gray;
        [field: SerializeField] public ExternalAssetReference<Material> _panelItemMaterial_Normal;
    }
}