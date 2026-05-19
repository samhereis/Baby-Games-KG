using Helpers;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DataClasses.AssetReferences
{
    [Serializable]
    public class ExternalAssetsReference_HasComponent<T> where T : Component
    {
        [field: SerializeField, FoldoutGroup("@key")] public string key { get; set; }

        [SerializeField, FoldoutGroup("@key")] private CachingMode cachingMode = CachingMode.Cache;
        [SerializeField, FoldoutGroup("@key")] private ReferenceMode referenceMode = ReferenceMode.ResourcesFolder;

        [Space, ShowInInspector, FoldoutGroup("@key")] private List<T> _cached;
#if UNITY_EDITOR
        [field: SerializeField, FoldoutGroup("@key")] public List<string> errorsList { get; set; }
#endif

        [Space, SerializeField, FoldoutGroup("@key"), ShowIf("@showDirectReference")] private List<T> reference;

        [Space, SerializeField, FoldoutGroup("@key"), ShowIf("@showResourceReference")] private List<string> resourcePath;
#if UNITY_EDITOR
        [SerializeField, FoldoutGroup("@key"), ShowIf("@showResourceReference")] private List<T> reference_Resources;
        [SerializeField, FoldoutGroup("@key"), ShowIf("@showResourceReference")] private List<T> reference_Resources_Debug;
#endif
        [Space, SerializeField, FoldoutGroup("@key"), ShowIf("@showAddressables")] private List<AssetReferenceGameObject> addressableReference;

        private bool showDirectReference => referenceMode == ReferenceMode.DirectReference;
        private bool showResourceReference => referenceMode == ReferenceMode.ResourcesFolder;
        private bool showAddressables => referenceMode == ReferenceMode.Addressables;

        [Button]
        public void Validate()
        {
#if UNITY_EDITOR

            errorsList.Clear();
            switch (referenceMode)
            {
                case ReferenceMode.DirectReference:
                    {
                        reference_Resources = null;
                        addressableReference = null;

                        break;
                    }
                case ReferenceMode.ResourcesFolder:
                    {
                        resourcePath.Clear();
                        reference_Resources_Debug.Clear();

                        foreach (var asset in reference_Resources)
                        {
                            string resultPath = string.Empty;
                            string resourcePathFull = string.Empty;

                            string path = AssetDatabase.GetAssetPath(asset);
                            int resourcesIndex = path.IndexOf("Resources/");
                            if (resourcesIndex != -1)
                            {
                                resourcePathFull = path.Substring(resourcesIndex + "Resources/".Length);
                                resultPath = resourcePathFull.Replace(System.IO.Path.GetExtension(path), "");
                                resourcePath.Add(resultPath);
                            }

                            reference_Resources_Debug.Add(Resources.Load<GameObject>(resultPath)?.GetComponent<T>());
                        }

                        break;
                    }
                case ReferenceMode.Addressables:
                    {
                        reference = null;
                        reference_Resources = null;

                        break;
                    }
            }

            key = typeof(T).Name;

            if (Application.isPlaying == false)
            {
                _cached = null;
            }
#endif
        }

        public async Task<List<T>> GetAssetsAsync()
        {
            if (_cached != null) { return _cached; }

            List<T> result = new();

            switch (referenceMode)
            {
                case ReferenceMode.DirectReference:
                    {
                        result = reference;

                        break;
                    }
                case ReferenceMode.ResourcesFolder:
                    {
                        foreach (var asset in resourcePath)
                        {
                            result.Add(Resources.Load<GameObject>(asset)?.GetComponent<T>());
                        }

                        break;
                    }
                case ReferenceMode.Addressables:
                    {
                        foreach (var reference in addressableReference)
                        {
                            var asset = await AddressablesHelper.GetAssetAsync<GameObject>(reference);
                            result.Add(asset.GetComponent<T>());
                        }

                        if (result.Count > 0) { _cached = result; }

                        break;
                    }
            }

            if (cachingMode == CachingMode.Cache) { _cached = result; }
            return result;
        }

        public async Task<T> GetRandomAssetAsync()
        {
            List<T> result = await GetAssetsAsync();
            return result.GetRandom();
        }
    }
}