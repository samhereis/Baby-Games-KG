using Sirenix.OdinInspector;
using System;
using UnityEditor;
using UnityEngine;

namespace DataClasses
{
    [Serializable]
    public class AScene : ISelfValidator
    {
        [field: SerializeField] public string scene { get; private set; }

#if UNITY_EDITOR
        [SerializeField] private SceneAsset sceneReference;
#endif

        public AScene(string scenName)
        {
            scene = scenName;
        }

        public void Validate(SelfValidationResult result)
        {
#if UNITY_EDITOR
            if (sceneReference == null) { result.AddError("No Scene Reference"); return; }
            scene = sceneReference.name;
#endif
        }
    }
}