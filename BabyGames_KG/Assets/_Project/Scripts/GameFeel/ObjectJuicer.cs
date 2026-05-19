using System;
using System.Threading;
using CustomAttributes;
using DG.Tweening;
using Helpers;
using Loggers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Project.Scripts.GameFeel
{
    public sealed class ObjectJuicer : MonoBehaviour
    {
        [Fg_Se] public Vector3 scaleIntensity = new Vector3(0.025f, 0.025f, 0f);
        [Fg_Se] public float rotationIntensity = 0f;
        [Fg_Se] public float speed = 5f;
        [Fg_Se] public float returnSpeed = 1;

        [Fg_De, SerializeField] public Vector3 initialScale;
        [Fg_De, SerializeField] public Quaternion initialRotation;
        [Fg_De, SerializeField] public bool isAnimating;

        private void Update()
        {
            if (isAnimating) { ApplyJuice(); }
        }

        [Button]
        public void StartJamming()
        {
            initialScale = transform.localScale;
            initialRotation = transform.localRotation;
            isAnimating = true;
        }

        [Button]
        public async void StopJamming(bool immediately = false)
        {
            try
            {
                if (isAnimating == false) { return; }
                isAnimating = false;

                await AsyncHelper.NextFrame();
                ReturnToNormal(immediately);
            } catch (Exception e) { CustomLogger.instance.LogException(e); }
        }

        private void ApplyJuice()
        {
            float timeIndex = Time.time * speed;
            float sinValue = Mathf.Sin(timeIndex);
            float newX = initialScale.x + (sinValue * scaleIntensity.x);
            float newY = initialScale.y + (-sinValue * scaleIntensity.y);

            float cosValue = Mathf.Cos(timeIndex);
            float tilt = cosValue * rotationIntensity;

            transform.localScale = new Vector3(newX, newY, initialScale.z);
            transform.localRotation = initialRotation * Quaternion.Euler(0, 0, tilt);
        }

        private void ReturnToNormal(bool immediately = false)
        {
            if (immediately)
            {
                transform.localScale = initialScale;
                transform.rotation = initialRotation;
            }
            else
            {
                transform?.DOScale(initialScale, returnSpeed);
                transform?.DORotateQuaternion(initialRotation, returnSpeed);
            }
        }
    }
}