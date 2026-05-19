using DG.Tweening;
using Gameplay;
using Helpers;
using Observables;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Coocking
{
    public class CookingCake_Butter : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public ObservableValue<bool> hasDropped = new("");

        public DroppableGeneral_SimpleController dropable;
        public Dropable_General item;

        public List<Transform> dropTarget = new();
        public Transform[] parts;

        private void OnEnable()
        {
            item = GetComponent<Dropable_General>();
            item.hasDropped.AddListener(OnHasDropped);
        }

        private void OnDisable()
        {
            item.hasDropped.RemoveListener(OnHasDropped);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            foreach (var item in parts)
            {
                item.GetComponent<SpriteRenderer>().sortingLayerName = "Background";
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            foreach (var item in parts)
            {
                item.GetComponent<SpriteRenderer>().sortingLayerName = "Default";
            }
        }

        private async void OnHasDropped(bool obj)
        {
            var targetsCopy = dropTarget.Shuffle_Copy();

            for (int i = 0; i < targetsCopy.Count; i++)
            {
                var part = parts[i];
                var target = targetsCopy[i];

                part.DOMove(target.position, 1);
                part.DORotate(new Vector3(0, 0, 0), 1);
            }

            await AsyncHelper.DelayFloat(1);
            hasDropped?.ChangeValue(true);
        }
    }
}