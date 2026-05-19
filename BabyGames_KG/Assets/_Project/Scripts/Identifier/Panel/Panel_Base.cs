using System.Threading.Tasks;

namespace Identifiers
{
    public enum PanelMode { Scale, Move }

    public abstract class Panel_Base : IdentifierBase
    {
        public PanelMode mode;

        public virtual Task Appear()
        {
            return Task.CompletedTask;
        }

        public virtual Task Disppear()
        {
            return Task.CompletedTask;
        }
    }
}