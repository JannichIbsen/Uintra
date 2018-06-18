using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Uintra.CentralFeed;

namespace Compent.Uintra.Hubs
{   
    public class FeedStateHub : Hub
    {
        private readonly IStateService<FeedFiltersState> _feedFilterStateService;
        private readonly IFeedHubService _feedHubService;

        public FeedStateHub(IStateService<FeedFiltersState> feedFilterStateService, IFeedHubService feedHubService)
        {
            _feedFilterStateService = feedFilterStateService;
            _feedHubService = feedHubService;
        }

        public void UpdateFiltersState(FeedFilterStateUpdateModel updateModel)
        {
            var previousState = _feedFilterStateService.Get();
            var newState = MapToFilterState(updateModel);

            _feedFilterStateService.Save(newState);
            if (IsOpenCloseChange(previousState, newState))
            {
                _feedHubService.NotifyFeedUpdate();
            }
        }

        public FeedFiltersState ResetFiltersState()
        {
            var defaults = _feedFilterStateService.GetDefaults();
            _feedFilterStateService.Save(defaults);
            return defaults;
        }

        protected virtual bool IsOpenCloseChange(FeedFiltersState oldState, FeedFiltersState newState) =>
            oldState.IsFiltersOpened != newState.IsFiltersOpened;


        protected virtual FeedFiltersState MapToFilterState(FeedFilterStateUpdateModel model)
        {
            return new FeedFiltersState
            {
                PinnedFilterSelected = model.ShowPinned,
                BulletinFilterSelected = model.IncludeBulletin,
                SubscriberFilterSelected = model.ShowSubscribed,
                SelectedActivityTypeId = model.SelectedActivityTypeId,
                IsFiltersOpened =  model.ShowFilters
            };
        }
    }
}