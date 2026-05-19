using DG.Tweening;
using System;
using System.Collections.Generic;
using Helpers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay
{
    [ExecuteInEditMode]
    public sealed class Levitator : MonoBehaviour
    {
        public Vector3Data rotationData = new Vector3Data(25f, 25f, 25);
        public Vector3Data positionData = new Vector3Data(0.25f, 0.25f, 0.25f);
        public Vector3Data scaleData = new Vector3Data(0.5f, 0.25f, 0.25f);

        [Header("Settings")]
        public bool rotationEnable = true;
        public bool positionEnable = true;
        public bool scaleEnable = true;

        [Header("Debug")]
        public bool rotationReached = false;
        public bool positionReached = false;
        public bool scaleReached = false;


        private void OnEnable()
        {
            rotationReached = true;
            positionReached = true;
            scaleReached = true;

            rotationData.originalValue = transform.localEulerAngles;
            positionData.originalValue = transform.localPosition;
            scaleData.originalValue = transform.localScale;
        }

        private void OnDisable()
        {
            transform.DOKill();
        }

        private void Update()
        {
            if (rotationEnable) { Rotate(); }
            if (positionEnable) { Position(); }
            if (scaleEnable) { Scale(); }
        }

        private void Rotate()
        {
            if (rotationReached)
            {
                rotationReached = false;

                positionData.UpdateDuration();
                positionData.UpdateValue();

                transform.DOLocalRotate(rotationData.originalValue + rotationData.currentTargetValue, rotationData.currentDuration)
                    .SetEase(rotationData.eases.GetRandom())
                    .OnComplete(() => rotationReached = true);
            }
        }

        private void Position()
        {
            if (positionReached)
            {
                positionReached = false;

                positionData.UpdateDuration();
                positionData.UpdateValue();

                var target = positionData.currentTargetValue;

                transform.DOLocalMove(positionData.originalValue + target, positionData.currentDuration)
                    .SetEase(positionData.eases.GetRandom())
                    .OnComplete(() => positionReached = true);
            }
        }

        private void Scale()
        {
            if (scaleReached)
            {
                scaleReached = false;

                scaleData.UpdateDuration();
                scaleData.UpdateValue();

                var target = scaleData.currentTargetValue;

                transform.DOScale(scaleData.originalValue + target, scaleData.currentDuration)
                    .SetEase(scaleData.eases.GetRandom())
                    .OnComplete(() => scaleReached = true);
            }
        }

        [Serializable]
        public class Vector3Data
        {
            public enum Mode { Value, Random }
            public Mode _mode;
            public List<Ease> eases = new List<Ease>() {Ease.Linear};

            [field: SerializeField] public Vector2 duration = new Vector2(0.25f, 0.75f);

            [Space(5)]
            [field: SerializeField] public Vector2 valueRandomX = new Vector2(-1, 1);
            [field: SerializeField] public Vector2 valueRandomY = new Vector2(-1, 1);
            [field: SerializeField] public Vector2 valueRandomZ = new Vector2(-1, 1);

            [field: SerializeField] public Vector3 originalValue;
            [field: SerializeField] public Vector3 currentTargetValue;
            [field: SerializeField] public float currentDuration = 0;

            public Vector3Data(float x, float y, float z)
            {
                valueRandomX = new Vector2(-x, x);
                valueRandomY = new Vector2(-y, y);
                valueRandomZ = new Vector2(-z, z);
            }
            public void UpdateDuration()
            {
                if (_mode == Mode.Value)
                {
                    currentDuration = duration.x;
                }
                else if (_mode == Mode.Random)
                {
                    currentDuration = Random.Range(duration.x, duration.y);
                }
            }

            public void UpdateValue()
            {
                if (_mode == Mode.Value)
                {
                    if (currentTargetValue.x == valueRandomX.x) { currentTargetValue.x = valueRandomX.y; }
                    else { currentTargetValue.x = valueRandomX.x; }
                    if (currentTargetValue.y == valueRandomY.x) { currentTargetValue.y = valueRandomY.y; }
                    else { currentTargetValue.y = valueRandomY.x; }
                    if (currentTargetValue.z == valueRandomZ.x) { currentTargetValue.z = valueRandomZ.y; }
                    else { currentTargetValue.z = valueRandomZ.x; }
                }
                else if (_mode == Mode.Random)
                {
                    float x = Random.Range(valueRandomX.x, valueRandomX.y);
                    float y = Random.Range(valueRandomY.x, valueRandomY.y);
                    float z = Random.Range(valueRandomZ.x, valueRandomZ.y);

                    currentTargetValue = new Vector3(x, y, z);
                }
            }
        }
    }
}