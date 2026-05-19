using DataClasses;
using Sounds;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SO
{
    [CreateAssetMenu(menuName = "Scriptables/" + nameof(BackgroundMusic_Data), fileName = nameof(BackgroundMusic_Data))]
    public class BackgroundMusic_Data : ScriptableObject, IDisposable
    {
        [field: SerializeField] public List<KeyedObject<string, Sound>> backgroundMusic { get; private set; }

        public void Dispose()
        {
        }
    }
}