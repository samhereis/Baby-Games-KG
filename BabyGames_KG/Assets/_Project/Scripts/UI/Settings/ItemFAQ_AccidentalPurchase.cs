using DataClasses;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Coloring.ForParents
{
    public class ItemFAQ_AccidentalPurchase : MonoBehaviour
    {
        [SerializeField] List<KeyedObject<string, Button>> _buttonLinks = new();

        public void OpenUrl(Button button)
        {
            string url = _buttonLinks.Find(x => x.value == button).key;
            Application.OpenURL(url);
        }
    }
}
