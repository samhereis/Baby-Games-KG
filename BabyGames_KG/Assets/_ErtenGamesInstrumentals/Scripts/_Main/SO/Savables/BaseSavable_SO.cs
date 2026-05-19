using System;
using UnityEngine;

namespace Settings
{
    [CreateAssetMenu(fileName = "BaseSavable_SO", menuName = "Scriptables/Settings/BaseSavable_SO")]
    public class BaseSavable_SO<T> : ScriptableObject
    {
        public Action<T> onValueChanged;

        [field: SerializeField] public T currentValue { get; protected set; }
        [field: SerializeField] public T defaultValue { get; protected set; } = default(T);
        [field: SerializeField] public string key { get; protected set; }

#if UNITY_EDITOR
        protected string _keyStartsWith = "";
        protected string _keyEndsWith = "_key";
#endif

        public virtual void Initialize()
        {

        }

        public virtual void SetData(T value)
        {
            currentValue = value;

            onValueChanged?.Invoke(value);
        }
    }
}