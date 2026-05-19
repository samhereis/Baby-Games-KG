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
    public class ExternalAssetReference<T> where T : Object
    {
        [field: SerializeField, FoldoutGroup("@key")] public string key { get; set; }

        [SerializeField, FoldoutGroup("@key")] public CachingMode cachingMode = CachingMode.Cache;
        [SerializeField, FoldoutGroup("@key")] public ReferenceMode referenceMode = ReferenceMode.DirectReference;

        [Space, ShowInInspector, FoldoutGroup("@key")] private T _cached;

#if UNITY_EDITOR
        [field: SerializeField, FoldoutGroup("@key")] public List<string> errorsList { get; set; }
#endif

        [Space, SerializeField, FoldoutGroup("@key"), ShowIf("@showDirectReference")] private T reference;

        [Space, SerializeField, FoldoutGroup("@key"), ShowIf("@showResourceReference")] private string resourcePath;
#if UNITY_EDITOR
        [SerializeField, FoldoutGroup("@key"), ShowIf("@showResourceReference")] private T reference_Resources;
        [SerializeField, FoldoutGroup("@key"), ShowIf("@showResourceReference")] private T reference_Resources_Debug;
#endif

        [Space, SerializeField, FoldoutGroup("@key"), ShowIf("@showAddressables")] private AssetReference addressableReference;

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
                        if (reference == null)
                        {
                            errorsList.Add("No DirectReference is set");
                            if (reference_Resources != null)
                            {
                                reference = reference_Resources;
                            }

                            if (addressableReference != null && reference == null)
                            {
                                reference = addressableReference.editorAsset as T;
                            }
                        }
                        else
                        {
                            reference_Resources = null;
                            addressableReference = null;

                            key = reference.name;
                        }

                        break;
                    }
                case ReferenceMode.ResourcesFolder:
                    {
                        if (reference_Resources == null)
                        {
                            errorsList.Add("No reference_Resources is set");
                            if (reference != null) { reference_Resources = reference; }
                            if (addressableReference != null && reference == null) { reference_Resources = addressableReference?.editorAsset as T; }

                            key = reference_Resources?.name;
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
                            string resourcePath = path.Substring(resourcesIndex + "Resources/".Length);
                            this.resourcePath = resourcePath.Replace(System.IO.Path.GetExtension(path), "");
                        }

                        reference_Resources_Debug = Resources.Load<T>(resourcePath);
                        key = reference_Resources_Debug?.name;

                        break;
                    }
                case ReferenceMode.Addressables:
                    {
                        if (addressableReference == null)
                        {
                            errorsList.Add("No Addressables is set");
                            if (reference != null) { addressableReference = new AssetReference(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(reference))); }
                            if (reference_Resources != null && reference == null) { addressableReference = new AssetReference(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(reference_Resources))); }
                        }
                        else
                        {
                            reference = null;
                            reference_Resources = null;
                        }
                        if (addressableReference.editorAsset is T == false) { errorsList.Add("Addressables Asset is not the needed type"); return; }

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

        public void SetObject(T obj)
        {
#if UNITY_EDITOR
            switch (referenceMode)
            {
                case ReferenceMode.DirectReference: { reference = obj; break; }
                case ReferenceMode.ResourcesFolder: { reference_Resources = obj; break; }
                case ReferenceMode.Addressables: { reference = obj; break; }
            }

            Validate();
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
                        result = Resources.Load<T>(resourcePath);
                        break;
                    }
                case ReferenceMode.Addressables:
                    {
                        result = await AddressablesHelper.GetAssetAsync<T>(addressableReference);
                        if (result != null) { _cached = result; }

                        break;
                    }
            }

            if (cachingMode == CachingMode.Cache) { _cached = result; }
            return result;
        }
    }
}