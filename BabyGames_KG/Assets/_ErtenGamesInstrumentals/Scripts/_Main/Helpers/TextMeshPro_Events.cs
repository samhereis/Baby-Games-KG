using DataClasses;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Helpers
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    [DisallowMultipleComponent]
    public class TextMeshPro_Events : MonoBehaviour, IPointerClickHandler
    {
        public UnityEvent onClickedOnText;
        public List<KeyedObject<string, UnityEvent<string>>> onClickedOnLink;

        public void OnPointerClick(PointerEventData eventData)
        {
            TMP_Text pTextMeshPro = GetComponent<TMP_Text>();
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(pTextMeshPro, eventData.position, Camera.main);  // If you are not in a Canvas using Screen Overlay, put your camera instead of null
            if (linkIndex >= 0)
            {
                TMP_LinkInfo linkInfo = pTextMeshPro.textInfo.linkInfo[linkIndex];
                string link = linkInfo.GetLinkID();

                onClickedOnLink.Find(x => x.key == link)?.value?.Invoke(link);
            }
            else
            {
                onClickedOnText?.Invoke();
            }
        }
    }
}