using Helpers;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace DataClasses.AssetReferences
{
    [Serializable]
    public class ExternalAssetReference_HasComponent<T> : IDisposable, ISelfValidator where T : Component
    {
        [field: SerializeField, FoldoutGroup("@key")] public string key { get; set; }

        [SerializeField, FoldoutGroup("@key")] public CachingMode cachingMode = CachingMode.DoNotCache;
        [SerializeField, FoldoutGroup("@key")] public ReferenceMode referenceMode = ReferenceMode.DirectReference;
        [Space, ShowInInspector, FoldoutGroup("@key")] private T _cached;
#if UNITY_EDITOR
        [field: SerializeField, FoldoutGroup("@key")] public List<string> errorsList { get; set; }
#endif

        [Space, SerializeField, FoldoutGroup("@key"), ShowIf("@showDirectReference")] private T reference;

        [Space, SerializeField, FoldoutGroup("@key"), ShowIf("@showResourceReference")] private string resourcePath;
#if UNITY_EDITOR
        [SerializeField, FoldoutGroup("@key"), ShowIf("@showResourceReference")] private T reference_Resources;
        [ShowInInspector, FoldoutGroup("@key"), ShowIf("@showResourceReference")] private T reference_Resources_Debug_Gameobject;
#endif

        [Space, SerializeField, FoldoutGroup("@key"), ShowIf("@showAddressables")] private AssetReferenceGameObject addressableReference;

        private bool showDirectReference => referenceMode == ReferenceMode.DirectReference;
        private bool showResourceReference => referenceMode == ReferenceMode.ResourcesFolder;
        private bool showAddressables => referenceMode == ReferenceMode.Addressables;

        public void Validate(SelfValidationResult result)
        {
            Validate();
        }

        [Button]
        public void Validate()
        {
#if UNITY_EDITOR

            errorsList.Clear();
            switch (referenceMode)
            {
                case ReferenceMode.DirectReference:
                    {
                        if (reference == null)
                        {
                            errorsList.Add("No DirectReference is set");
                            if (reference_Resources != null) { reference = reference_Resources; }
                            else if (addressableReference != null) { reference = addressableReference?.editorAsset?.GetComponent<T>(); }
                        }
                        else
                        {
                            reference_Resources = null;
                            addressableReference = null;
                        }

                        key = reference.name;
                        break;
                    }
                case ReferenceMode.ResourcesFolder:
                    {
                        if (reference_Resources == null)
                        {
                            errorsList.Add("No reference_Resources is set");
                            if (reference != null) { reference_Resources = reference; }
                            else if (addressableReference != null) { reference_Resources = addressableReference?.editorAsset?.GetComponent<T>(); }

                            key = reference?.name;
                        }
                        else
                        {
                            reference = null;
                            addressableReference = null;
                        }

                        string path = AssetDatabase.GetAssetPath(reference_Resources);
                        int resourcesIndex = path.IndexOf("Resources/");
                        if (resourcesIndex != -1)
                        {
                            string resourcePathFull = path.Substring(resourcesIndex + "Resources/".Length);
                            resourcePath = resourcePathFull.Replace(System.IO.Path.GetExtension(path), "");
                        }

                        reference_Resources_Debug_Gameobject = Resources.Load<GameObject>(resourcePath)?.GetComponent<T>();

                        key = reference_Resources_Debug_Gameobject?.name;

                        break;
                    }
                case ReferenceMode.Addressables:
                    {
                        if (addressableReference == null)
                        {
                            errorsList.Add("No Addressables is set");
                            if (reference != null) { addressableReference = new AssetReferenceGameObject(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(reference))); }
                            else if (reference_Resources != null) { addressableReference = new AssetReferenceGameObject(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(reference_Resources))); }
                        }
                        else
                        {
                            reference = null;
                            reference_Resources = null;
                        }
                        if (addressableReference.editorAsset.GetComponent<T>() == null) { errorsList.Add("Addressables Asset does not have the component"); return; }

                        key = addressableReference.editorAsset.name;
                        break;
                    }
            }

            if (Application.isPlaying == false)
            {
                _cached = null;
            }
#endif
        }

        public async Task<T> GetAssetAsync()
        {
            if (_cached != null) { return _cached; }

            T result = null;

            switch (referenceMode)
            {
                case ReferenceMode.DirectReference:
                    {
                        result = reference;
                        break;
                    }
                case ReferenceMode.ResourcesFolder:
                    {
                        result = Resources.Load<GameObject>(resourcePath)?.GetComponent<T>();
                        break;
                    }
                case ReferenceMode.Addressables:
                    {
                        var asset = await AddressablesHelper.GetAssetAsync<GameObject>(addressableReference);
                        result = asset.GetComponent<T>();

                        break;
                    }
            }

            if (cachingMode == CachingMode.Cache) { _cached = result; }
            return result;
        }

        public async Task<T> InstantiateAsync(Transform parent = null, Vector3? position = null)
        {
            if (position == null) { position = new Vector3(0, 0, 0); }
            return Object.Instantiate<T>(await GetAssetAsync(), position.Value, Quaternion.identity, parent);
        }

        public void Dispose()
        {
            if (addressableReference != null) { addressableReference.ReleaseAsset(); }
            if (_cached != null) { addressableReference.ReleaseInstance(_cached.gameObject); _cached = null; }
        }
    }
}