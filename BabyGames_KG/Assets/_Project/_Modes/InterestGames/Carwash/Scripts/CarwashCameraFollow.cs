using DG.Tweening;
using Loggers;
using Spine.Unity;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Carwash
{
    public class CarwashCameraFollow : MonoBehaviour
    {
        public static CarwashCameraFollow instance;

        [SerializeField] private BoneFollower _boneFollower;

        [SerializeField] protected bool _doFollow = false;
        [SerializeField] protected float _followSpeed = 1f;
        public Vector3 offset;
        public Vector3 cameraInitialPosition;

        private void Awake()
        {
            instance = this;
        }

        private void OnEnable()
        {
            cameraInitialPosition = Camera.main.transform.position;
        }

        private void Update()
        {
            if (_doFollow == false) { return; }

            offset.z = -25;
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, transform.position + offset, _followSpeed * Time.deltaTime);
        }

        public async Task Follow(SkeletonAnimation skeletonAnimation, string boneName)
        {
            try
            {
                transform.position = Vector3.zero;

                _boneFollower.skeletonRenderer = skeletonAnimation;
                _boneFollower.SetBone(boneName);

                await Camera.main.transform.DOMoveX(transform.position.x, 1).AsyncWaitForCompletion();
                _doFollow = true;
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex);
            }
        }

        public void StopFollow()
        {
            _doFollow = false;
        }
    }
}