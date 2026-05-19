using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace DI
{
    public class AutoBinder : MonoInstaller
    {
        [SerializeField] private List<ComponentsPack> _componentPacks = new();

        public override void InstallBindings()
        {
            foreach (var componentPack in _componentPacks)
            {
                foreach (var component in componentPack.components)
                {
                    Container.BindInstances(component);
                }
            }
        }
    }

    [Serializable]
    public class ComponentsPack
    {
        public string name;
        public Object[] components;
    }
}