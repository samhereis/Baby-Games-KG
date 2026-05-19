using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Helpers
{
    public static class MonobehaviorHelper
    {
        public static void SetEnabled(this Collider monoBehaviour, bool isEnabled)
        {
            if (monoBehaviour == null) { return; }
            monoBehaviour.enabled = isEnabled;
        }

        public static void TrySetDirty(this Object monoBehaviour)
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(monoBehaviour);
#endif
        }

        public static void TrySetDirty(this MonoBehaviour monoBehaviour, MonoBehaviour anotherMonoBehaviour)
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(anotherMonoBehaviour);
#endif
        }

        public static void DeleteAllMissingScripts(GameObject gameObject)
        {
#if UNITY_EDITOR

            Debug.Log("Deleted missing scripts: " + GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject));
#endif
        }

        public static List<T> FindAll<T>() where T : Object
        {
            return Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();
        }

        public static T Find<T>() where T : Object
        {
            return Object.FindAnyObjectByType<T>(FindObjectsInactive.Include);
        }

        public static List<T> SpawnPrefab<T>(GameObject prefab, Vector3 startPoint, Vector3 endPoint, int numberOfPrefabs, Transform parent) where T : Component
        {
            List<T> result = new List<T>();

            // Validate inputs
            if (prefab == null)
            {
                Debug.LogWarning("Prefab is null! Please assign a valid prefab.");
                return result;
            }

            if (numberOfPrefabs <= 1)
            {
                Debug.LogWarning("Number of prefabs must be greater than 1 to calculate spacing.");
                return result;
            }

            // Calculate the total distance and spacing
            float totalDistance = Vector3.Distance(startPoint, endPoint);
            float spacing = totalDistance / (numberOfPrefabs - 1);

            // Calculate direction from start to end
            Vector3 direction = (endPoint - startPoint).normalized;

#if UNITY_EDITOR
            if (!Application.isPlaying && parent != null)
            {
                Undo.RegisterCreatedObjectUndo(parent.gameObject, "Spawn Prefabs");
            }
#endif

            for (int i = 0; i < numberOfPrefabs; i++)
            {
                // Calculate the position for this prefab
                Vector3 spawnPosition = startPoint + direction * (spacing * i);

                // Instantiate prefab
                GameObject spawnedPrefab =
#if UNITY_EDITOR
                    Application.isPlaying == false ?
                        PrefabUtility.InstantiatePrefab(prefab) as GameObject :
#endif
                        Object.Instantiate(prefab, spawnPosition, Quaternion.identity);

                if (spawnedPrefab == null)
                {
                    Debug.LogError("Failed to instantiate prefab. Ensure the prefab is valid.");
                    continue;
                }

                // Set position and parent
                spawnedPrefab.transform.position = spawnPosition;
                if (parent != null)
                {
                    spawnedPrefab.transform.SetParent(parent);
                    spawnedPrefab.transform.localEulerAngles = Vector3.zero;
                }

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    Undo.RegisterCreatedObjectUndo(spawnedPrefab, "Spawn Prefab");
                }
#endif

                // Add the component of type T to the result list
                T component = spawnedPrefab.GetComponent<T>();
                if (component != null)
                {
                    result.Add(component);
                }
            }

            return result;
        }
    }
}