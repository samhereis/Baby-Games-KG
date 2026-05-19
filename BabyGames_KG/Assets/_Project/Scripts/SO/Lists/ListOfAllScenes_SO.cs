using DataClasses;
using UI.Helpers;
using UnityEngine;

namespace SO
{
    [CreateAssetMenu(menuName = "Scriptables/Lists/" + nameof(ListOfAllScenes_SO), fileName = nameof(ListOfAllScenes_SO))]
    public class ListOfAllScenes_SO : ScriptableObject
    {
        [field: SerializeField] public AScene mainMenu_Scene { get; private set; }

        [Header("Gameplay")]
        [field: SerializeField, Space] public AScene gameplay_Scene_Coloring { get; private set; }
        [field: SerializeField] public AScene gameplay_Scene_Puzzle { get; private set; }
        [field: SerializeField] public AScene gameplay_Scene_Orchestra { get; private set; }
        [field: SerializeField] public AScene gameplay_Scene_Video { get; private set; } = new("Gameplay_Scene_Video");
        [field: SerializeField] public AScene gameplay_Scene_Hiding { get; private set; }
        [field: SerializeField] public AScene gameplay_Scene_Bubble { get; private set; }
        [field: SerializeField] public AScene gameplay_Scene_Carwash { get; private set; }
        [field: SerializeField] public AScene gameplay_Scene_WhoLivesWhere { get; private set; }
        [field: SerializeField] public AScene gameplay_Scene_CarTuning { get; private set; }
        [field: SerializeField] public AScene gameplay_Scene_Princess { get; private set; }
        [field: SerializeField] public AScene gameplay_Scene_ColorfulTrain { get; private set; }
        [field: SerializeField] public AScene gameplay_Scene_CookingSalade { get; private set; }
    }
}