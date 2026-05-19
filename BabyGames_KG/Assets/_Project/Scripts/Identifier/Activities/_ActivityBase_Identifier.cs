using DataClasses;
using DataClasses.Consts;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using Interfaces.Services;
using System.Threading.Tasks;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Identifiers
{
    public class _ActivityBase_Identifier : IdentifierBase
    {
        public string version = "0";
        [field: SerializeField] public ActivityType type { get; protected set; }
        [field: SerializeField] public string assetFolderPath { get; protected set; }
        [field: SerializeField] public string assetPath { get; protected set; }

        [FoldoutGroup("Localization")] public string loc_table;
        [FoldoutGroup("Localization")] public string loc_entry;
        [FoldoutGroup("Localization")] public string loc_table_String;
        [FoldoutGroup("Localization")] public string loc_entry_String;

        [FoldoutGroup("Availability")] public bool alwaysUnlocked;


#if UNITY_EDITOR
        [FoldoutGroup("Editor")] public bool ignoreBuild;
#endif

        [Button]
        public virtual void UpdateData(Activity activity)
        {
#if UNITY_EDITOR
            string path = AssetDatabase.GetAssetPath(gameObject);
            if (!string.IsNullOrEmpty(path))
            {
                assetPath = path;
                assetFolderPath = System.IO.Path.GetDirectoryName(path);
                EditorUtility.SetDirty(this); // Mark as dirty to save changes in editor
            }

            loc_table = Constants_Localization.Categories;
            loc_table_String = Constants_Localization.Categories;

            activity.loc_tableName = loc_table;
            activity.loc_entryName = loc_entry;
            activity.loc_tableName_String = loc_table_String;
            activity.loc_entryName_String = loc_entry_String;
            activity.isAlwaysUnlocked = alwaysUnlocked;

            activity.isAlwaysUnlocked = alwaysUnlocked;
#endif
        }

        [Button]
        public virtual void UpdateVersion()
        {
#if UNITY_EDITOR
            version = DateTime.Now.ToString();
#endif
        }
        
        public virtual Task DownloadAdditionals(Activity activity, IContentDeliveryService contentDeliveryService, Action<float> onDownloadingUpdate = null)
        {
            return Task.CompletedTask;
        }
    }
}