using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UIElements;

namespace CarTuning
{
    public class Princess_DressBase : MonoBehaviour
    {
        public string skinName;

        public Vector3 initialPosition;
        public Vector3 initialScale = new(1f, 1f, 1f);
        public Vector3 dragScale = new(1f, 1f, 1f);

        [SerializeField] private Vector3 _currentPosition;

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
            _currentPosition = position;
            transform.position = _currentPosition;
        }

        public virtual void Drop()
        {
            transform.DOMove(initialPosition, 0.25f).SetEase(Ease.OutBack);
            transform.DOScale(initialScale, 0.25f).SetEase(Ease.OutBack);
        }

        public virtual void SetVisible(bool isVisible)
        {
            gameObject.SetActive(isVisible);
        }
    }
}