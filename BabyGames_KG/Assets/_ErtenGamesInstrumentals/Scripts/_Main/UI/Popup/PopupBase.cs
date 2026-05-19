using Interfaces;
using UnityEngine;

namespace UI.Popups
{
    public abstract class PopupBase : MonoBehaviour, IMenuWindow
    {
        public void Open()
        {
            Enable();
        }

        public void Close()
        {
            Disable();
        }

        public abstract void Enable(float? duration = null);
        public abstract void Disable(float? duration = null);
    }
}