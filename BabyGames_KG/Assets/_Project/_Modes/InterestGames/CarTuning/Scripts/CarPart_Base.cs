using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CarTuning
{
    public class CarPart_Base : MonoBehaviour
    {
        public Vector3 initialPosition;
        public Vector3 initialScale = new(1f, 1f, 1f);
        public Vector3 dragScale = new(0.25f, 0.25f, 0.25f);
        public float particleScale = 2;

        public CarPart_Base second;

        [Button]
        public void Validate()
        {
            initialPosition = transform.position;
            initialScale = transform.localScale;
        }

        private void OnEnable()
        {
            transform.localScale = dragScale;
        }

        public virtual void Move(Vector3 position)
        {
            transform.position = position;
        }

        public virtual void Drop()
        {
            Move_FX.MakeDoneParticle(initialPosition, particleScale);
            transform.DOMove(initialPosition, 0.25f).SetEase(Ease.OutBack);
            transform.DOScale(initialScale, 0.25f).SetEase(Ease.OutBack);
        }

        public virtual void SetVisible(bool isVisible)
        {
            gameObject.SetActive(isVisible);
        }
    }
}