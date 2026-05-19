using TMPro;
using UnityEngine;

namespace Video
{
    public class VideoTest : MonoBehaviour
    {
        public TextMeshProUGUI textTemplate;    // Assign one TMP element
        public RectTransform viewport;
        public RectTransform holder;         // Mask panel for scrolling
        public float scrollSpeed = 100f;
        public float extraGap = 50f;            // ← Space (in pixels) between repeats

        private TextMeshProUGUI txt1, txt2;
        private RectTransform rt1, rt2;
        private float textWidth;

        void Awake()
        {
            if (textTemplate == null || viewport == null)
            {
                Debug.LogError("Assign textTemplate and viewport!");
                enabled = false;
                return;
            }

            txt1 = Instantiate(textTemplate, holder);
            txt2 = Instantiate(textTemplate, holder);
            rt1 = txt1.rectTransform;
            rt2 = txt2.rectTransform;

            textTemplate.gameObject.SetActive(false);
            UpdateText(txt1.text);
        }

        public void UpdateText(string newContent)
        {
            txt1.text = txt2.text = newContent;
            txt1.ForceMeshUpdate(); // ensure preferredWidth is accurate
            textWidth = txt1.preferredWidth;

            Vector2 size = new Vector2(textWidth, rt1.sizeDelta.y);
            rt1.sizeDelta = rt2.sizeDelta = size;

            rt1.anchoredPosition = new Vector2(0, rt1.anchoredPosition.y);
            rt2.anchoredPosition = new Vector2(textWidth + extraGap, rt2.anchoredPosition.y);
        }

        void Update()
        {
            float move = scrollSpeed * Time.deltaTime;
            rt1.anchoredPosition -= new Vector2(move, 0);
            rt2.anchoredPosition -= new Vector2(move, 0);

            if (rt1.anchoredPosition.x + textWidth < 0)
            {
                rt1.anchoredPosition = new Vector2(rt2.anchoredPosition.x + textWidth + extraGap, rt1.anchoredPosition.y);
                Swap();
            }
        }

        void Swap()
        {
            (rt1, rt2) = (rt2, rt1);
            (txt1, txt2) = (txt2, txt1);
        }
    }
}