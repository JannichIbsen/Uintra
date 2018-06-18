using System;
using System.Collections.Generic;
using System.Linq;
using Compent.Extensions;
using Uintra.CentralFeed;
using Uintra.Core.Caching;
using Uintra.Core.Context;
using Uintra.Core.Extensions;
using Uintra.Groups;

namespace Compent.Uintra.Core.CentralFeed
{
    public class CentralFeedService : FeedService, ICentralFeedService
    {
        private readonly IEnumerable<IFeedItemService> _feedItemServices;

        public CentralFeedService(
            IEnumerable<IFeedItemService> feedItemServices,
            ICacheService cacheService)
            : base(feedItemServices, cacheService)
        {
            _feedItemServices = feedItemServices;
        }

        public IEnumerable<IFeedItem> GetFeed(Enum type)
        {
            var services = ContextExtensions.ExactScalar(type, CentralFeedTypeEnum.All)
                ? _feedItemServices
                : _feedItemServices.Single(s => s.Type.ToInt() == type.ToInt()).ToEnumerable();

            var result = services
                .SelectMany(service => service.GetItems())
                .Where(IsCentralFeedActivity);

            return result;
        }

        private static bool IsCentralFeedActivity(IFeedItem item) =>
            (item as IGroupActivity)?.GroupId == null;
    }
}