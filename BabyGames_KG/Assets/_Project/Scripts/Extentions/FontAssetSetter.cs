using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Helpers
{
    [DisallowMultipleComponent]
    public class FontAssetSetter : MonoBehaviour
    {
        public TMP_Text[] proUGUI;
        public TMP_FontAsset fontAsset;

        [Button]
        private void FillTexts()
        {
            proUGUI = GetComponentsInChildren<TMP_Text>(true);
        }

        [Button]
        private void SetFontAssets()
        {
            foreach (var t in proUGUI)
            {
                t.font = fontAsset;
            }
        }



        [Button]
        private void DisableMaskable(bool enalbe = false)
        {
            foreach (var t in proUGUI)
            {
                t.maskable = enalbe;
            }
        }
    }
}