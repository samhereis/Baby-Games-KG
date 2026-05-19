using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.World.Helpers
{
    public sealed class PoolersManager : MonoBehaviour
    {
        [Required]
        [SerializeField] private List<ScriptableObject> _poolerBases = new List<ScriptableObject>();

        private void OnDisable()
        {
            foreach (var pooler in _poolerBases)
            {
                (pooler as IDisposable).Dispose();
            }
        }
    }
}