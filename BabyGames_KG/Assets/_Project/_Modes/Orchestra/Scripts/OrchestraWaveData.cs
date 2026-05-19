using _Project.Scripts._Modes.Sorting;
using CustomAttributes;
using DataClasses;
using Modes.Sorting;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project._Modes.Orchestra.Scripts
{
    [Serializable]
    public class OrchestraWaveUnit
    {
        public enum SearchMode
        {
            Is,
            Contains
        }

        public Action<OrchestraWaveUnit> onComplete;

        public string instrumentName = nameof(instrumentName);

        [FoldoutGroup("@instrumentName")] public SearchMode searchMode = SearchMode.Contains;
        [FoldoutGroup("@instrumentName")] public string animationName = "idle_";
        [FoldoutGroup("@instrumentName")] public string giveAnimation = "give_";
        [FoldoutGroup("@instrumentName")] public string finalAnimation = "final_";
        [FoldoutGroup("@instrumentName")] public string sortingLayerName = "Default";
        [FoldoutGroup("@instrumentName")] public List<KeyedObject<List<string>, int>> objectNames = new() { new(new() { "" }, 100) };
        [FoldoutGroup("@instrumentName")] public ForceObjectNamesData objectNames_force = new();
        [FoldoutGroup("@instrumentName")] public AudioClip animationAudio;
        [FoldoutGroup("@instrumentName")] public float animationAudio_Delay;
        [FoldoutGroup("@instrumentName")] public bool autoSet = false;

        [Fg_De] public DropZone_Identifier dropZone;
        [Fg_De] public Orchestra_Draggable draggable;
        [Fg_De] public DropZone_Identifier forceRightAnswer;
        [Fg_De] public List<MeshRenderer> relatedSkeletonParts = new();
        [Fg_De] public OrchestraCharacter_Identifier orchestraCharacter_Identifier;
        [Fg_De] public bool isComplete;

        [Button]
        public void Setup()
        {
            animationName = "idle_" + instrumentName;
            giveAnimation = "give_" + instrumentName;
            finalAnimation = "final_" + instrumentName;

            objectNames = new List<KeyedObject<List<string>, int>>()
            {
                new (new (){ instrumentName }, 100)
            };
        }
    }

    [Serializable]
    public class ForceObjectNamesData
    {
        public List<SpriteRenderer> _spriteRenderers;
    }

    [Serializable]
    public class OrchestraWaveData
    {
        public List<OrchestraWaveUnit> waveUnits = new();
    }
}