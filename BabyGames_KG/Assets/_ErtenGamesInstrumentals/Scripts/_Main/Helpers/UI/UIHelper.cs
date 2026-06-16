using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Helpers
{
    public class UIHelper : MonoBehaviour
    {
        private static PointerEventData _eventDataCurrentPosition;
        private static List<RaycastResult> _raycastResults;

        [Button]
        public void DisableAllRaycastTargets()
        {
            foreach (var item in GetComponentsInChildren<Graphic>())
            {
                item.raycastTarget = false;
            }
        }

        public static bool IsPointOverUI()
        {
            return IsPointOverUI(Pointer.current.position.ReadValue());
        }

        public static bool IsPointOverUI(Vector2 pos)
        {
            if (EventSystem.current == null) return false;
            var ped = new PointerEventData(EventSystem.current) { position = pos };
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(ped, results);

            foreach (var r in results)
            {
                if (r.gameObject.GetComponent<Graphic>() != null) { return true; }
            }

            return false;
        }

        public static Vector2 GetWorlPositonOfCanvasElement(RectTransform rectTransform, Camera camera)
        {
            RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, rectTransform.position, camera, out var worldPoint);

            return worldPoint;
        }
    }
}