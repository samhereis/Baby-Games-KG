using System;
using UnityEngine;

namespace Observables
{
    [Serializable]
    public class DataSignal<T>
    {
        private Action<T> _onInvoke;

        [field: SerializeField] public string eventName { get; private set; }
        [field: SerializeField] public int currentSubscribedObjectsCount { get; private set; }
        [field: SerializeField] public int timesInvoked { get; private set; }

        public DataSignal(string eventName)
        {
            this.eventName = eventName;
        }

        public void AddListener(Action<T> action)
        {
            _onInvoke += action;
            UpdateCurrentSubscribedObjectsCount();
        }

        public void RemoveListener(Action<T> action)
        {
            _onInvoke -= action;
            UpdateCurrentSubscribedObjectsCount();
        }

        public void Clear()
        {
            _onInvoke = null;
        }

        public void Invoke(T parameter)
        {
            _onInvoke?.Invoke(parameter);
            timesInvoked++;
        }

        private void UpdateCurrentSubscribedObjectsCount()
        {
            if (_onInvoke == null)
            {
                currentSubscribedObjectsCount = 0;
            }
            else
            {
                currentSubscribedObjectsCount = _onInvoke.GetInvocationList().Length;
            }
        }
    }
}