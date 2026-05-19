using UnityEngine;

namespace UI.Helpers
{
    public class ResetContentHolderPosition : MonoBehaviour
    {
        private void OnEnable()
        {
            Vector3 current = GetComponent<RectTransform>().anchoredPosition3D;
            current.y = 0;

            GetComponent<RectTransform>().anchoredPosition3D = current;
        }
    }
}
