using Helpers;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace InterestGames
{
    public class Makeup_Washing_ItemBase : MonoBehaviour, ISelfValidator, IPointerDownHandler, IPointerUpHandler
    {
        public Action onFinish;

        public List<Makeup_Pena> items = new();
        public BoxCollider boxCollider;

        public void Validate(SelfValidationResult result)
        {
            boxCollider = GetComponent<BoxCollider>();
            this.TrySetDirty();
        }

        public virtual void Initialize()
        {
        }

        protected virtual void Awake()
        {
            foreach (var item in items)
            {
                item.isActive = false;
            }
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            foreach (var item in items)
            {
                item.isActive = true;
            }
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            foreach (var item in items)
            {
                item.isActive = false;
            }
        }
    }
}
