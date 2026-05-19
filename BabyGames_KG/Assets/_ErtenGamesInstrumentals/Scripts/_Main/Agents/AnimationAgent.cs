using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Agents
{
    public class AnimationAgent : MonoBehaviour
    {
        public Action<string> onAnimationCallback;

        [Required]
        [field: SerializeField] public Animator animator { get; private set; }

        private void Awake()
        {
            if (animator == null) { animator = GetComponentInChildren<Animator>(true); }
        }

        public void CallCallback(string callbackName)
        {
            onAnimationCallback?.Invoke(callbackName);
        }

        public void PlayAnimation(int animationHash)
        {
            animator.Play(animationHash);
        }

        public void PlayAnimation(string animationName)
        {
            animator?.Play(animationName);
        }

        public void CrossFade(int animationHash, float duration = 0.5f)
        {
            animator.CrossFade(animationHash, duration);
        }

        public void CrossFade(string animationName, float duration = 0.5f)
        {
            animator.CrossFade(animationName, duration);
        }

        [ContextMenu(nameof(TryEnableProjectionToRagdoll))]
        public void TryEnableProjectionToRagdoll()
        {
            foreach (var joint in GetComponentsInChildren<CharacterJoint>()) joint.enableProjection = true;
        }

        [ContextMenu(nameof(TryDisableProjectionToRagdoll))]
        public void TryDisableProjectionToRagdoll()
        {
            foreach (var joint in GetComponentsInChildren<CharacterJoint>()) joint.enableProjection = false;
        }

        [ContextMenu(nameof(TryEnableContinuousCollisionDetectionInChildRigidBodies))]
        public void TryEnableContinuousCollisionDetectionInChildRigidBodies()
        {
            foreach (var rigidbody in GetComponentsInChildren<Rigidbody>()) rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        [ContextMenu(nameof(TryDisableContinuousCollisionDetectionInChildRigidBodies))]
        public void TryDisableContinuousCollisionDetectionInChildRigidBodies()
        {
            foreach (var rigidbody in GetComponentsInChildren<Rigidbody>()) rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
        }

        [ContextMenu(nameof(TryEnableUpdateOffCameraOnAllSkinnedMeshRenderers))]
        public void TryEnableUpdateOffCameraOnAllSkinnedMeshRenderers()
        {
            foreach (var skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>()) skinnedMeshRenderer.updateWhenOffscreen = true;
        }

        [ContextMenu(nameof(TryDisableUpdateOffCameraOnAllSkinnedMeshRenderers))]
        public void TryDisableUpdateOffCameraOnAllSkinnedMeshRenderers()
        {
            foreach (var skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>()) skinnedMeshRenderer.updateWhenOffscreen = false;
        }
    }
}