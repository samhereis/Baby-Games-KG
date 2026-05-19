using Helpers;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Pooling
{
    public class PoolerBase<T> : ScriptableObject, IDisposable where T : Component
    {
        [Required]
        [field: SerializeField] public T poolable { get; protected set; }

        [FoldoutGroup("Settings"), SerializeField] protected bool _setParent = true;
        [FoldoutGroup("Settings"), SerializeField] private int _defaultSpawnQuantity = 2;

        [FoldoutGroup("Debug"), SerializeField] protected Queue<T> _poolablesQueue = new Queue<T>();
        [FoldoutGroup("Debug"), SerializeField] protected List<T> _poolablesDequeued = new List<T>();
        [FoldoutGroup("Debug"), SerializeField] protected List<T> _poolablesQueued = new List<T>();
        [FoldoutGroup("Debug"), SerializeField] private int _spawnCount = 0;
        [FoldoutGroup("Debug"), SerializeField] private Transform _parent;

        public virtual void Initialize(Transform parent)
        {
            _parent = parent;
        }

        public virtual async void Initialize()
        {
            Dispose();
            await SpawnAsync(_defaultSpawnQuantity, _parent);
        }

        public virtual async Task SpawnAsync(int quantity = 5, Transform parent = null)
        {
            for (int i = 0; i < quantity; i++)
            {
                await AsyncHelper.Skip();

                var poolableInstance = Instantiate(poolable, parent);
                poolableInstance.gameObject.name += _spawnCount;

                PutIn(poolableInstance);

                _spawnCount++;

                if (_spawnCount >= int.MaxValue - 1) { _spawnCount = 0; }
            }
        }

        public virtual void Spawn(int quantity = 5, Transform parent = null)
        {
            for (int i = 0; i < quantity; i++)
            {
                var poolableInstance = Instantiate(poolable, parent);
                poolableInstance.gameObject.name += _spawnCount;

                PutIn(poolableInstance);

                _spawnCount++;

                if (_spawnCount >= int.MaxValue - 1) { _spawnCount = 0; }
            }
        }

        public virtual T PutOff(Transform position, Quaternion rotation, Transform parent = null)
        {
            return PutOff(position.position, rotation, parent);
        }

        public virtual T PutOff(Transform position, Transform parent = null)
        {
            return PutOff(position.position, Quaternion.identity, parent);
        }

        public virtual T PutOff(Vector3 position, Transform parent = null)
        {
            return PutOff(position, Quaternion.identity, parent);
        }

        public virtual T PutOff(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            T poolable;

            if (_poolablesQueue.Count < 1) Spawn(_defaultSpawnQuantity, _parent);

            poolable = _poolablesQueue.Dequeue();

            if (poolable == null)
            {
                Dispose();
                Spawn(1, _parent);

                poolable = _poolablesQueue.Dequeue();
            }

            _poolablesDequeued.SafeAdd(poolable);
            _poolablesQueued.SafeRemove(poolable);

            if (parent != null) poolable.transform.parent = parent;

            poolable.transform.position = position;
            poolable.transform.rotation = rotation;

            poolable.gameObject.SetActive(true);

            poolable.transform.position = position;
            poolable.transform.rotation = rotation;

            return poolable;
        }

        public virtual void PutIn(T poolable)
        {
            if (poolable)
            {
                try
                {
                    _poolablesQueue?.Enqueue(poolable);
                    _poolablesDequeued?.SafeRemove(poolable);
                    _poolablesQueued?.SafeAdd(poolable);

                    poolable.gameObject.SetActive(false);

                    if (_setParent) poolable.transform.parent = _parent;
                }
                finally
                {

                }
            }
        }

        public virtual async void PutInAll()
        {
            var copy = new List<T>();
            copy.AddRange(_poolablesDequeued);

            foreach (var poolable in copy)
            {
                PutIn(poolable);
                await AsyncHelper.Skip();
            }

            Dispose();
        }

        [Button]
        public void Dispose()
        {
            _poolablesQueue?.Clear();
            _poolablesQueue = new Queue<T>();

            _poolablesDequeued?.Clear();
            _poolablesDequeued = new List<T>();

            _poolablesQueued?.Clear();
            _poolablesQueued = new List<T>();

            _spawnCount = 0;
        }
    }
}