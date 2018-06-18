using System.Collections.Generic;

namespace Uintra.CentralFeed
{
    public interface IFeedListBuilder
    {
        IEnumerable<IFeedItem> BuildForFeed(IEnumerable<IFeedItem> rawItems, FeedFiltersState filterState, FeedSettings settings);
        IEnumerable<IFeedItem> BuildForLatestActivity(IEnumerable<IFeedItem> rawItems);
    }
}