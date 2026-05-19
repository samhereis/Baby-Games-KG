#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Helpers
{
    public static class PrefabHelper
    {
        public static T AddToPrefabAsChild<T>(this GameObject prefab, GameObject gameObject)
        {
#if UNITY_EDITOR
            string prefabPath = AssetDatabase.GetAssetPath(prefab);
            GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);
            gameObject.transform.SetParent(prefabContents.transform);
            PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabContents);
#endif
            return prefab.GetComponent<T>();
        }
    }
}