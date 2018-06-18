using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Compent.Extensions;
using Uintra.Core;
using Uintra.Core.Context;
using Uintra.Core.Extensions;
using Uintra.Core.Feed;
using Uintra.Core.User;
using Uintra.Subscribe;

namespace Uintra.CentralFeed.Web
{
    public abstract class FeedControllerBase : ContextController
    {
        protected abstract string OverviewViewPath { get; }
        protected abstract string ListViewPath { get; }

        protected abstract string DetailsViewPath { get; }
        protected abstract string CreateViewPath { get; }
        protected abstract string EditViewPath { get; }

        protected virtual int ItemsPerPage => 8;

        private readonly IFeedService _feedService;

        protected FeedControllerBase(
            ISubscribeService subscribeService,
            IFeedService feedService,
            IIntranetUserService<IIntranetUser> intranetUserService,
            IStateService<FeedFiltersState> feedFilterStateService,
            IFeedTypeProvider centralFeedTypeProvider,
            IContextTypeProvider contextTypeProvider): base(contextTypeProvider)
        {
            _feedService = feedService;
        }
        
        public virtual JsonResult AvailableActivityTypes()
        {
            var activityTypes = _feedService
                .GetAllSettings()
                .Where(s => !s.ExcludeFromAvailableActivityTypes)
                .Select(s => ( Id:s.Type.ToInt(), Name: s.Type.ToString()))
                .Select(a => new { a.Id, a.Name })
                .OrderBy(el => el.Id);

            return Json(activityTypes, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult CacheVersion() => 
            Json(new { Result = long.MaxValue }, JsonRequestBehavior.AllowGet);


        protected virtual IEnumerable<FeedItemViewModel> GetFeedItems(IEnumerable<IFeedItem> items, IEnumerable<FeedSettings> settings)
        {
            var activitySettings = settings
                .ToDictionary(s => s.Type.ToInt());

            var result = items
                .Select(i => MapFeedItemToViewModel(i, activitySettings));

            return result;
        }

        protected virtual FeedItemViewModel MapFeedItemToViewModel(IFeedItem i, Dictionary<int, FeedSettings> settings)
        {
            var options = GetActivityFeedOptions(i.Id);
            return new FeedItemViewModel
            {
                Activity = i,
                Options = options,
                ControllerName = settings[i.Type.ToInt()].Controller
            };
        }

        protected abstract ActivityFeedOptions GetActivityFeedOptions(Guid activityId);

        protected static IEnumerable<ActivityFeedTabViewModel> GetTabsWithCreateUrl(IEnumerable<ActivityFeedTabViewModel> tabs) =>
            tabs.Where(t => !IsTypeForAllActivities(t.Type) && t.Links.Create.HasValue());

        protected static bool IsTypeForAllActivities(Enum type) =>
            type is CentralFeedTypeEnum.All;   
    }
}
