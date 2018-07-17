using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Compent.Extensions;
using Uintra.CentralFeed;
using Uintra.CentralFeed.Web;
using Uintra.Core;
using Uintra.Core.Activity;
using Uintra.Core.Attributes;
using Uintra.Core.Context;
using Uintra.Core.Extensions;
using Uintra.Core.Feed;
using Uintra.Core.User;
using Uintra.Core.User.Permissions;
using Uintra.Groups.Attributes;
using Uintra.Subscribe;
using static Uintra.Core.Context.Extensions.ContextExtensions;

namespace Uintra.Groups.Web
{
    public abstract class GroupFeedControllerBase : FeedControllerBase
    {
        private readonly IGroupFeedService _groupFeedService;
        private readonly IActivitiesServiceFactory _activitiesServiceFactory;
        private readonly IFeedTypeProvider _centralFeedTypeProvider;
        private readonly IIntranetUserService<IGroupMember> _intranetUserService;
        private readonly IGroupFeedContentService _groupFeedContentContentService;
        private readonly IGroupMemberService _groupMemberService;
        private readonly IStateService<FeedFiltersState> _feedFilterStateService;
        private readonly IPermissionsService _permissionsService;
        private readonly IFeedLinkService _feedLinkService;
        private readonly IFeedListBuilder _feedListAssembler;

        private bool IsCurrentUserGroupMember { get; set; }

        protected override string OverviewViewPath => "~/App_Plugins/Groups/Room/Feed/Overview.cshtml";
        protected override string DetailsViewPath => "~/App_Plugins/Groups/Room/Feed/Details.cshtml";
        protected override string CreateViewPath => "~/App_Plugins/Groups/Room/Feed/Create.cshtml";
        protected override string EditViewPath => "~/App_Plugins/Groups/Room/Feed/Edit.cshtml";
        protected override string ListViewPath => "~/App_Plugins/Groups/Room/Feed/List.cshtml";

        public override ContextType ControllerContextType { get; } = ContextType.GroupFeed;

        protected GroupFeedControllerBase(
            ISubscribeService subscribeService,
            IGroupFeedService groupFeedService,
            IActivitiesServiceFactory activitiesServiceFactory,
            IIntranetUserContentProvider intranetUserContentProvider,
            IFeedTypeProvider centralFeedTypeProvider,
            IIntranetUserService<IGroupMember> intranetUserService,
            IGroupFeedContentService groupFeedContentContentService,
            IGroupFeedLinkProvider groupFeedLinkProvider,
            IGroupMemberService groupMemberService,
            IStateService<FeedFiltersState> feedFilterStateService,
            IPermissionsService permissionsService,
            IContextTypeProvider contextTypeProvider,
            IFeedLinkService feedLinkService,
            IFeedListBuilder feedListAssembler)
            : base(
                  subscribeService,
                  groupFeedService,
                  intranetUserService,
                  feedFilterStateService,
                  centralFeedTypeProvider,
                  contextTypeProvider)
        {
            _groupFeedService = groupFeedService;
            _activitiesServiceFactory = activitiesServiceFactory;
            _centralFeedTypeProvider = centralFeedTypeProvider;
            _intranetUserService = intranetUserService;
            _groupFeedContentContentService = groupFeedContentContentService;
            _groupMemberService = groupMemberService;
            _feedFilterStateService = feedFilterStateService;
            _permissionsService = permissionsService;
            _feedLinkService = feedLinkService;
            _feedListAssembler = feedListAssembler;
        }

        #region Actions

        [HttpGet]
        public ActionResult Overview(Guid groupId)
        {
            var model = GetOverviewModel(groupId);
            return PartialView(OverviewViewPath, model);
        }

        [HttpGet]
        [NotFoundActivity]
        [NotFoundGroup]
        public virtual ActionResult Details(Guid id, Guid groupId)
        {
            var viewModel = GetDetailsViewModel(id, groupId);
            return PartialView(DetailsViewPath, viewModel);
        }

        [HttpGet]
        public ActionResult Create(Guid groupId)
        {
            var currentUser = _intranetUserService.GetCurrentUser();
            if (!_groupMemberService.IsGroupMember(groupId, currentUser))
                return new EmptyResult();

            var activityType = _groupFeedContentContentService.GetCreateActivityType(CurrentPage);
            var viewModel = GetCreateViewModel(activityType, groupId);
            return PartialView(CreateViewPath, viewModel);
        }

        [HttpGet]
        [NotFoundActivity]
        [NotFoundGroup]
        public virtual ActionResult Edit(Guid id, Guid groupId)
        {
            var viewModel = GetEditViewModel(id, groupId);
            return PartialView(EditViewPath, viewModel);
        }

        public ActionResult List(GroupFeedListModel model)
        {
            var filtersState = _feedFilterStateService.Get();
            var centralFeedType = _centralFeedTypeProvider[filtersState.SelectedActivityTypeId];
            var items = GetGroupFeedItems(centralFeedType, model.GroupId);
            var tabSettings = _groupFeedService.GetSettings(centralFeedType);

            var filteredItems = _feedListAssembler.BuildForFeed(items, _feedFilterStateService.Get(), tabSettings).ToList();

            var centralFeedModel = GetFeedListViewModel(model, filteredItems, centralFeedType);

            return PartialView(ListViewPath, centralFeedModel);
        }
        #endregion

        protected virtual IEnumerable<IFeedItem> GetGroupFeedItems(Enum type, Guid groupId)
        {
            return type is CentralFeedTypeEnum.All
                ? _groupFeedService.GetFeed(groupId).OrderByDescending(item => item.PublishDate)
                : _groupFeedService.GetFeed(type, groupId);
        }

        protected virtual FeedListViewModel GetFeedListViewModel(GroupFeedListModel model, List<IFeedItem> items, Enum centralFeedType)
        {
            var itemsList = items.AsList();

            var settings = _groupFeedService
                .GetAllSettings()
                .AsList();

            var tabSettings = settings
                .Single(s => ExactScalar(s.Type, centralFeedType))
                .Map<FeedTabSettings>();

            var takeCount = model.Page * ItemsPerPage;

            var viewedItems = itemsList.Take(takeCount);

            var currentUserId = _intranetUserService.GetCurrentUser().Id;
            IsCurrentUserGroupMember = _groupMemberService.IsGroupMember(model.GroupId, currentUserId);

            return new FeedListViewModel
            {
                Feed = GetFeedItems(viewedItems, settings),
                TabSettings = tabSettings,
                Type = centralFeedType,
                BlockScrolling = itemsList.Count < takeCount
            };
        }

        protected override ActivityFeedOptions GetActivityFeedOptions(Guid id)
        {
            return new ActivityFeedOptions
            {
                Links = _feedLinkService.GetLinks(id),
                IsReadOnly = !IsCurrentUserGroupMember
            };
        }

        protected virtual GroupFeedOverviewModel GetOverviewModel(Guid groupId)
        {
            var currentUser = _intranetUserService.GetCurrentUser();
            var tabType = _groupFeedContentContentService.GetFeedTabType(CurrentPage);

            var tabs = _groupFeedContentContentService.GetActivityTabs(CurrentPage, currentUser, groupId);
            var activityTabs = tabs.Where(t => t.Type != null).Map<List<ActivityFeedTabViewModel>>();

            var model = new GroupFeedOverviewModel
            {
                Tabs = activityTabs,
                TabsWithCreateUrl = GetTabsWithCreateUrl(activityTabs).Where(tab => _permissionsService.IsCurrentUserHasAccess(tab.Type, IntranetActivityActionEnum.Create)),
                CurrentType = tabType,
                GroupId = groupId,
                IsGroupMember = _groupMemberService.IsGroupMember(groupId, currentUser)
            };
            return model;
        }

        protected virtual CreateViewModel GetCreateViewModel(Enum activityType, Guid groupId)
        {
            var links = _feedLinkService.GetCreateLinks(activityType, groupId);
            var settings = _groupFeedService.GetSettings(activityType);

            return new CreateViewModel
            {
                Links = links,
                Settings = settings
            };
        }

        protected virtual EditViewModel GetEditViewModel(Guid id, Guid groupId)
        {
            var service = _activitiesServiceFactory.GetService<IIntranetActivityService>(id);
            var links = _feedLinkService.GetLinks(id);
            var settings = _groupFeedService.GetSettings(service.Type);

            var viewModel = new EditViewModel
            {
                Id = id,
                Links = links,
                Settings = settings
            };
            return viewModel;
        }

        protected virtual DetailsViewModel GetDetailsViewModel(Guid id, Guid groupId)
        {
            var service = _activitiesServiceFactory.GetService<IIntranetActivityService>(id);
            var currentUserId = _intranetUserService.GetCurrentUser().Id;
            IsCurrentUserGroupMember = _groupMemberService.IsGroupMember(groupId, currentUserId);
            var options = GetActivityFeedOptions(id);
            var settings = _groupFeedService.GetSettings(service.Type);

            var viewModel = new DetailsViewModel
            {
                Id = id,
                Options = options,
                Settings = settings
            };
            return viewModel;
        }
    }
}