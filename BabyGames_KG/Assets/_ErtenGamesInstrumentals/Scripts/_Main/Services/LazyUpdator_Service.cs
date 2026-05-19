using Helpers;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Services
{
    public class LazyUpdator_Service
    {
        private static LazyUpdator_Service _instance_;
        public static LazyUpdator_Service instance
        {
            get
            {
                if (_instance_ == null) { _instance_ = new(); }
                return _instance_;
            }
        }

        [SerializeField] private List<Func<Task>> _tasks = new List<Func<Task>>();

        [SerializeField] private bool _isRunning = false;
        [ShowInInspector] private int _tasksCount => _tasks.Count;

        public virtual void AddToQueue(Func<Task> task)
        {
            if (_tasks.Count == 0)
            {
                _isRunning = false;
                DoLazyUpdateWithAwait(Add);
            }
            else Add();

            void Add() { _tasks.SafeAdd(task); }
        }

        public virtual void RemoveFromQueue(Func<Task> task)
        {
            _tasks.SafeRemove(task);
        }

        protected virtual void RemoveAllNullTasks()
        {
            _tasks.RemoveNulls();
        }

        protected virtual async void DoLazyUpdateWithAwait(Action doBeforeUpdate = null)
        {
            if (_isRunning) return;

            doBeforeUpdate?.Invoke();

            try
            {
                while (_tasks.Count > 0 && Application.isPlaying)
                {
                    _isRunning = true;

                    RemoveAllNullTasks();

                    var actions = _tasks.ToArray();
                    foreach (Func<Task> action in actions)
                    {
                        if (action == null)
                        {
                            _tasks.Remove(action);
                        }
                        else
                        {
                            await action?.Invoke();
                        }
                        await AsyncHelper.NextFrame();
                    }
                }
            }
            finally
            {
                _isRunning = false;
            }
        }
    }
}