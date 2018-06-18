using System;
using System.Collections.Generic;
using System.Linq;
using Compent.Extensions;
using Uintra.CentralFeed;
using Uintra.Core.User;
using Uintra.Subscribe;

namespace Compent.Uintra.Hubs
{
    public class FeedListBuilder : IFeedListBuilder
    {
        private readonly ISubscribeService _subscribeService;
        private readonly IIntranetUserService<IIntranetUser> _intranetUserService;
        private readonly IComparer<IFeedItem> _itemSortingComparer;

        public FeedListBuilder(ISubscribeService subscribeService, IIntranetUserService<IIntranetUser> intranetUserService, IComparer<IFeedItem> itemSortingComparer)
        {
            _subscribeService = subscribeService;
            _intranetUserService = intranetUserService;
            _itemSortingComparer = itemSortingComparer;
        }

        public virtual IEnumerable<IFeedItem> BuildForFeed(IEnumerable<IFeedItem> rawItems, FeedFiltersState filterState, FeedSettings settings)
        {
            var filters = GetFiltersForFeed(filterState, settings);
            var filteredItems = FilterItems(rawItems, filters);

            var sortedItems = SortForFeed(filteredItems);

            return sortedItems;
        }

        public virtual IEnumerable<IFeedItem> BuildForLatestActivity(IEnumerable<IFeedItem> rawItems)
        {
            var sortedItems = SortForLatestActivity(rawItems);

            return sortedItems;
        }

        protected virtual IEnumerable<IFeedItem> FilterItems(IEnumerable<IFeedItem> unfilteredItems, List<Func<IFeedItem, bool>> filters) =>
            unfilteredItems.Where(item => filters.All(filter => filter(item)));

        protected virtual List<Func<IFeedItem, bool>> GetFiltersForFeed(FeedFiltersState filterState, FeedSettings settings)
        {
            var filters = new List<Func<IFeedItem, bool>>();

            if (filterState.SubscriberFilterSelected && settings.HasSubscribersFilter)
            {
                filters.Add(IsSubscribedFilter);
            }

            if (filterState.PinnedFilterSelected && settings.HasPinnedFilter)
            {
                filters.Add(IsPinnedFilter);
            }

            return filters;
        }

        protected virtual bool IsPinnedFilter(IFeedItem item) =>
            item.IsPinned;

        protected virtual bool IsSubscribedFilter(IFeedItem item) =>
            item is ISubscribable subscribable && _subscribeService.IsSubscribed(_intranetUserService.GetCurrentUser().Id, subscribable);

        protected virtual IOrderedEnumerable<IFeedItem> SortForFeed(IEnumerable<IFeedItem> items) =>
            items.Pipe(BaseSorting)
                .Pipe(ByPinSorting);

        protected virtual IOrderedEnumerable<IFeedItem> SortForLatestActivity(IEnumerable<IFeedItem> items) =>
            items.Pipe(BaseSorting);

        protected virtual IOrderedEnumerable<IFeedItem> BaseSorting(IEnumerable<IFeedItem> items) =>
            items.OrderBy(item => item, _itemSortingComparer);

        protected virtual IOrderedEnumerable<IFeedItem> ByPinSorting(IEnumerable<IFeedItem> items) =>
            items.OrderByDescending(el => el.IsPinActual);
    }
}