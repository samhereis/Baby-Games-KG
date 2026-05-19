using System.Threading.Tasks;
using UnityEngine;

namespace Modes.Coloring
{
    [RequireComponent(typeof(HasOverrides_Identifier))]
    public class OverrideBase : MonoBehaviour
    {
        public virtual Task Initialize()
        {
            return Task.CompletedTask;
        }

        public virtual Task Clear()
        {
            return Task.CompletedTask;
        }
    }
}