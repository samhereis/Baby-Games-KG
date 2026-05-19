using Zenject;

namespace Services
{
    public class DiService
    {
        private static DiContainer container { get; set; }

        public static void AddContainer(DiContainer newContainer)
        {
            container = newContainer;
        }

        public static void Inject(object obj)
        {
            container?.Inject(obj);
        }

        public static T Get<T>(string id = "")
        {
            return container.ResolveId<T>(id);
        }

        public static T Get<T>()
        {
            return container.Resolve<T>();
        }
    }
}
