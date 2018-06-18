namespace Compent.Uintra.Hubs
{
    public interface IFeedHubService
    {
        void NotifyFeedUpdate();
        void NotifyFiltersStateUpdate();
    }
}