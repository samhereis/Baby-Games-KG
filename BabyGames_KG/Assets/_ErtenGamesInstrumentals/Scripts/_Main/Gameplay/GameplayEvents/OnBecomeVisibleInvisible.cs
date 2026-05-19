using UnityEngine;
using UnityEngine.Events;

namespace GameplayEvents
{
    public class OnBecomeVisibleInvisible : MonoBehaviour
    {
        [field: SerializeField]  public bool isVisible { get; private set; } = true;

        [field: SerializeField]  public UnityEvent onBecomeVisible { get; private set; }
         [field: SerializeField] public UnityEvent onBecomeInvisible { get; private set; }

        [field: SerializeField]  private MonoBehaviour[] _activateOnVisible;
         [field: SerializeField] private MonoBehaviour[] _deactivateOnVisible;

        private void OnBecameVisible()
        {
            SetVisible(true);

            onBecomeVisible?.Invoke();
        }

        private void OnBecameInvisible()
        {
            SetVisible(false);

            onBecomeInvisible?.Invoke();
        }

        private void SetVisible(bool visible)
        {
            foreach (var monobeh in _activateOnVisible)
            {
                monobeh.enabled = visible == true;
            }

            foreach (var monobeh in _deactivateOnVisible)
            {
                monobeh.enabled = visible == false;
            }

            isVisible = visible;
        }
    }
}