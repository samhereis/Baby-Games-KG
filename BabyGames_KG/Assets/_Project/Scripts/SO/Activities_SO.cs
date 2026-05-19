using System.Collections.Generic;
using DataClasses;
using UnityEngine;

namespace SO
{
    [CreateAssetMenu(fileName = nameof(Activities_SO), menuName = "ScriptableObjects/" + nameof(Activities_SO))]
    public class Activities_SO : ScriptableObject
    {
        public const string BUNDLE_NAME = "_activities_so";

        public string version;
        public List<ActivityCategory_SO> activityCategories = new List<ActivityCategory_SO>();
        public List<KeyedObject<string, float>> floatSettings = new();
    }
}