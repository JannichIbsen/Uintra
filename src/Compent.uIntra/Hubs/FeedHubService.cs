using System;
using Microsoft.AspNet.SignalR;
using Uintra.CentralFeed;

namespace Compent.Uintra.Hubs
{
    public class FeedHubService : IFeedHubService
    {
        private readonly Lazy<IHubContext> _lazyCtx = new Lazy<IHubContext>(GlobalHost.ConnectionManager.GetHubContext<FeedStateHub>);
        private IHubContext HubContext => _lazyCtx.Value;
        private readonly IStateService<FeedFiltersState> _feedFilterStateService;

        public FeedHubService(IStateService<FeedFiltersState> feedFilterStateService)
        {
            _feedFilterStateService = feedFilterStateService;
        }

        public void NotifyFeedUpdate() => 
            HubContext.Clients.All.feedUpdate();

        public void NotifyFiltersStateUpdate() =>
            HubContext.Clients.All.filtersStateUpdate(_feedFilterStateService.Get());
    }
}