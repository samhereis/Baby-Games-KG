namespace Factories
{
    public interface IFactory<TType>
    {
        public TType Create();
    }
}