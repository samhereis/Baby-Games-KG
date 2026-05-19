using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace IdentityCards
{
    [Serializable]
    public class IdentityCardBase<T>
    {
        [field: FoldoutGroup("Base"), SerializeField] public string targetName { get; protected set; }
        [field: FoldoutGroup("Base"), SerializeField] public T target { get; protected set; }

        public void SetTargetName(string targetName)
        {
            this.targetName = targetName;
        }

        public void SetTarget(T target, bool autoSetTargetName = true)
        {
            this.target = target;
            if (autoSetTargetName == true) Validate();
        }

        public virtual void Validate()
        {
            if (this.target != null)
            {
                targetName = target.ToString();
            }
        }
    }
}