using Assets._Project._Modes.Video.Scripts;
using Bubble;
using CarTuning;
using Carwash;
using ColorfulTrain;
using Coloring.CheckForParents;
using Coloring.ForParents;
using CoockingSalade;
using DataClasses.AssetReferences;
using Modes.Coloring;
using Modes.Puzzle;
using Modes.Sorting;
using Saratan.Coloring.Gallery;
using Sirenix.OdinInspector;
using System;
using UI.Helpers;
using UI.Menus;
using UI.Popups;
using UnityEngine;

namespace SO
{
    [CreateAssetMenu(menuName = "Scriptables/Lists/" + nameof(ListOfAllMenus_SO), fileName = nameof(ListOfAllMenus_SO))]
    public class ListOfAllMenus_SO : ScriptableObject, IDisposable
    {
        [field: SerializeField] public ExternalAssetReference_HasComponent<ActivityCategories_Menu> categoriesMenu { get; private set; }
        [field: SerializeField] public ExternalAssetReference_HasComponent<Activities_Menu> activitiesMenu { get; private set; }
        [field: SerializeField] public ExternalAssetReference_HasComponent<Activities_Menu_Colorings> activitiesMenu_Coloring { get; private set; }
        [field: SerializeField] public ExternalAssetReference_HasComponent<Activities_Menu_Video> activitiesMenu_Video { get; private set; }
        [field: SerializeField] public ExternalAssetReference_HasComponent<SettingsMenu> settingsMenu { get; private set; }
        [field: SerializeField] public ExternalAssetReference_HasComponent<PurchaseMenu> purchaseMenu { get; private set; }
        [field: SerializeField] public ExternalAssetReference_HasComponent<ParentalGateMenu> parentalGateMenu { get; private set; }

        [field: SerializeField] public ExternalAssetReference_HasComponent<WinMenu_Universal> winMenu_Universal { get; private set; }

        [field: SerializeField, FoldoutGroup("Gameplay")] public ExternalAssetReference_HasComponent<GameplayMenu_Coloring> gameplayMenu_Coloring { get; private set; }
        [field: SerializeField, FoldoutGroup("Gameplay")] public ExternalAssetReference_HasComponent<GameplayMenu_Puzzle> gameplayMenu_Puzzle { get; private set; }
        [field: SerializeField, FoldoutGroup("Gameplay")] public ExternalAssetReference_HasComponent<GameplayMenu_Orchestra> gameplayMenu_Orchestra { get; private set; }
        [field: SerializeField, FoldoutGroup("Gameplay")] public ExternalAssetReference_HasComponent<GameplayMenu_Video> gameplayMenu_Video { get; private set; }
        [field: SerializeField, FoldoutGroup("Gameplay")] public ExternalAssetReference_HasComponent<GameplayMenu_Hiding> gameplayMenu_Hiding { get; private set; }
        [field: SerializeField, FoldoutGroup("Gameplay")] public ExternalAssetReference_HasComponent<GameplayMenu_Bubble> gameplayMenu_Bubble { get; private set; }
        [field: SerializeField, FoldoutGroup("Gameplay")] public ExternalAssetReference_HasComponent<GameplayMenu_Carwash> gameplayMenu_Carwash { get; private set; }
        [field: SerializeField, FoldoutGroup("Gameplay")] public ExternalAssetReference_HasComponent<GameplayMenu_WhoLivesWhere> gamgaeplayMenu_WhoLivesWhere { get; private set; }
        [field: SerializeField, FoldoutGroup("Gameplay")] public ExternalAssetReference_HasComponent<GameplayMenu_CarTuning> gameplayMenu_CarTuning { get; private set; }
        [field: SerializeField, FoldoutGroup("Gameplay")] public ExternalAssetReference_HasComponent<GameplayMenu_Princess> gameplayMenu_Princess { get; private set; }
        [field: SerializeField, FoldoutGroup("Gameplay")] public ExternalAssetReference_HasComponent<GameplayMenu_RoomCleaning> gameplayMenus_RoomCleaning { get; private set; }
        [field: SerializeField, FoldoutGroup("Gameplay")] public ExternalAssetReference_HasComponent<GameplayMenu_Makeup> gameplayMenus_Makeug { get; private set; }
        [field: SerializeField, FoldoutGroup("Gameplay")] public ExternalAssetReference_HasComponent<GameplayMenu_ColorfulTrain> gamgaeplayMenu_ColorfulTrain { get; private set; }
        [field: SerializeField, FoldoutGroup("Gameplay")] public ExternalAssetReference_HasComponent<GameplayMenu_CoockingSalade> gamgaeplayMenu_CoockingSalade { get; private set; }

        [Header("Popups")]
        //[field: SerializeField, FoldoutGroup("Popup")] public ExternalAssetReference_HasComponent<InfoPopup> subscriptionExpDate_Popup { get; private set; }
        [field: SerializeField, FoldoutGroup("Popup")] public ExternalAssetReference_HasComponent<InfoPopup> subscriptionExp_Popup { get; private set; }

        public void Dispose()
        {
        }
    }
}