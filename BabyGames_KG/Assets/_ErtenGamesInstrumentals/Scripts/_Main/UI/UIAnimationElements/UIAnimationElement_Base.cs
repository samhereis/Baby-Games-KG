using DG.Tweening;
using System;
using UnityEngine;

namespace UI.UIAnimationElements
{
    public class UIAnimationElement_Base : MonoBehaviour
    {
        [SerializeField] protected BaseSettings _baseSettings = new BaseSettings();

        protected virtual void Awake()
        {
            TurnOff(0);
        }

        protected virtual void OnEnable()
        {
            TurnOn();
        }

        protected virtual void OnDisable()
        {
            TurnOff(0);
        }

        public virtual void TurnOff(float? duration = null)
        {

        }

        public virtual void TurnOn(float? duration = null)
        {

        }

        [Serializable]
        protected class BaseSettings
        {
            public Ease onEase = Ease.OutBack;
            public Ease offEase = Ease.OutBack;

            public float turnOffDuration = 0.2f;
            public float turnOnDuration = 0.5f;
        }
    }
}