using Identifiers;

namespace Modes.Coloring
{
    public class Tools_Identifier : IdentifierBase
    {
        public void Activate()
        {
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }
    }
}