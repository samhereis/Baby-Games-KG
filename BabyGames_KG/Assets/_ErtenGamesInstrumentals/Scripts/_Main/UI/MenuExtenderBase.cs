using System;
using UnityEngine;

namespace UI.Menus
{
    public abstract class MenuExtenderBase<TMenu> : MonoBehaviour where TMenu : MenuBase
    {
        public TMenu window
        {
            get
            {
                if (_baseSettings.window == null) { _baseSettings.window = GetComponent<TMenu>(); }

                return _baseSettings.window;
            }
        }

        [SerializeField] protected CanvasWindowExtendorBaseSettings _baseSettings = new CanvasWindowExtendorBaseSettings();

        [Serializable]
        public class CanvasWindowExtendorBaseSettings
        {
            public TMenu window;
        }
    }
}