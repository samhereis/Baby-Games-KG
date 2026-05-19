using Services;
using Zenject;

namespace DI
{
    // Поставь рядом с Scene Context
    public class ContainerSetter : MonoInstaller
    {
        public override void InstallBindings()
        {
            DiService.AddContainer(Container);
        }
    }
}
