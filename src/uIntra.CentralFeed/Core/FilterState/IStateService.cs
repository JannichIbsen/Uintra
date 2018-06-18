namespace Uintra.CentralFeed
{
    public interface IStateService<T>
    {
        void Save(T stateModel);
        T Get();
        T GetDefaults();
    }
}