using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Identifiers
{
    public class IdentifierBase : MonoBehaviour
    {
        protected Dictionary<Type, Object> _components = new Dictionary<Type, Object>();

        public T Get<T>() where T : Object
        {
            T component = null;

            if (_components.ContainsKey(typeof(T)))
            {
                component = _components[typeof(T)] as T;
            }
            else
            {
                component = GetComponentInChildren<T>(true);
                if (component != null) _components.Add(typeof(T), component);
            }

            return component;
        }

        public List<T> TryGetAll_List<T>()
        {
            List<T> list = new List<T>();
            list = GetComponentsInChildren<T>(true)?.ToList();

            return list == null ? new List<T>() : list;
        }

        public bool TryGet<T>(out T result) where T : Component
        {
            result = Get<T>();
            return result != null;
        }
    }
}
