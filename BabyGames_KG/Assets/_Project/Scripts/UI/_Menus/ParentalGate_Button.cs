using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ParentalGate_Button : MonoBehaviour
    {
        public Button button;
        public TextMeshProUGUI text;
        public int number;

        public void SetNumber(int number)
        {
            this.number = number;
            text.text = number.ToString();
        }
    }
}