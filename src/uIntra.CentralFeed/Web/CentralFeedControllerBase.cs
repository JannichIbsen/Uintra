﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using uIntra.CentralFeed.App_Plugins.CentralFeed.Models;
using uIntra.Core.Activity;
using uIntra.Core.Extentions;
using uIntra.Core.User;
using uIntra.Subscribe;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;

namespace uIntra.CentralFeed.Web
{
    public abstract class CentralFeedControllerBase : SurfaceController
    {
        protected virtual string OverviewViewPath { get; } = "~/App_Plugins/CentralFeed/View/CentralFeedOverView.cshtml";
        protected virtual string ListViewPath { get; } = "~/App_Plugins/CentralFeed/View/CentralFeedList.cshtml";
        protected virtual string NavigationViewPath { get; } = "~/App_Plugins/CentralFeed/View/Navigation.cshtml";

        private readonly ICentralFeedService _centralFeedService;
        private readonly ICentralFeedContentHelper _centralFeedContentHelper;
        private readonly IActivitiesServiceFactory _activitiesServiceFactory;
        private readonly ISubscribeService subscribeService;
        private readonly IIntranetUserService<IIntranetUser> intranetUserService;
        protected const int ItemsPerPage = 8;

        protected CentralFeedControllerBase(
            ICentralFeedService centralFeedService,
            ICentralFeedContentHelper centralFeedContentHelper,
            IActivitiesServiceFactory activitiesServiceFactory,
            ISubscribeService subscribeService,
            IIntranetUserService<IIntranetUser> intranetUserService)
        {
            _centralFeedService = centralFeedService;
            _centralFeedContentHelper = centralFeedContentHelper;
            _activitiesServiceFactory = activitiesServiceFactory;
            this.subscribeService = subscribeService;
            this.intranetUserService = intranetUserService;
        }

        public virtual ActionResult Overview()
        {
            var tabType = _centralFeedContentHelper.GetTabType(CurrentPage);
            var filtersState = _centralFeedContentHelper.GetFiltersState();
            var model = new CentralFeedOverviewModel
            {
                CurrentType = tabType,
                FiltersState = filtersState.Map<CentralFeedFiltersStateModel>()
            };
            return PartialView(OverviewViewPath, model);
        }

        public virtual ActionResult List(CentralFeedListModel model)
        {
            var items = GetCentralFeedItems(model.Type.GetHashCode().ToEnum<IntranetActivityTypeEnum>());

            var tabSettings = _centralFeedService.GetSettings(model.Type);

            var filteredItems = ApplyFilters(items, model, tabSettings).ToList();

            var currentVersion = _centralFeedService.GetFeedVersion(filteredItems);

            if (model.Version.HasValue && currentVersion == model.Version.Value)
            {
                return null;
            }

            var take = model.Page * ItemsPerPage;
            var pagedItemsList = filteredItems.OrderBy(IsPinActual).ThenByDescending(el => el.PublishDate).Take(take).ToList();

            FillActivityDetailLinks(pagedItemsList);

            var centralFeedModel = new CentralFeedListViewModel
            {
                Version = _centralFeedService.GetFeedVersion(filteredItems),
                Items = pagedItemsList,
                Settings = _centralFeedService.GetAllSettings(),
                Type = model.Type,
                BlockScrolling = filteredItems.Count < take,
                ShowPinned = model.ShowPinned,
                IncludeBulletin = model.IncludeBulletin ?? false,
                ShowSubscribed = model.ShowSubscribed ?? false
            };


            _centralFeedContentHelper.SaveFiltersState(new CentralFeedFiltersStateModel()
            {
                PinnedFilterSelected = centralFeedModel.ShowPinned,
                BulletinFilterSelected = centralFeedModel.IncludeBulletin,
                SubscriberFilterSelected = centralFeedModel.ShowSubscribed
            });
            return PartialView(ListViewPath, centralFeedModel);
        }

        public virtual ActionResult Tabs()
        {
            return PartialView(NavigationViewPath, GetTypes().ToList());
        }

        public virtual JsonResult CacheVersion()
        {
            var version = _centralFeedService.GetFeedVersion(Enumerable.Empty<ICentralFeedItem>());
            return Json(new { Result = version }, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult AvailableActivityTypes()
        {
            var activityTypes = _centralFeedService.GetAllSettings().Select(s => (CentralFeedTypeEnum)s.Type);
            var activityTypeModelList = activityTypes.Select(a => new { Id = a.GetHashCode(), Name = a.ToString() }).ToList();
            activityTypeModelList.Insert(0, new { Id = CentralFeedTypeEnum.All.GetHashCode(), Name = CentralFeedTypeEnum.All.ToString() });

            return Json(activityTypeModelList, JsonRequestBehavior.AllowGet);
        }

        protected virtual List<ICentralFeedItem> GetCentralFeedItems(IntranetActivityTypeEnum? type)
        {
            if (type == null)
            {
                var items = _centralFeedService.GetFeed().ToList();
                items.Sort(new CentralFeedComparer());
                return items;
            }

            return _centralFeedService.GetFeed(type.Value).ToList();
        }

        protected virtual IEnumerable<CentralFeedTypeModel> GetTypes()
        {
            var allSettings = _centralFeedService.GetAllSettings();
            foreach (var singleSetting in allSettings)
            {
                yield return new CentralFeedTypeModel
                {
                    Type = singleSetting.Type,
                    CreateUrl = singleSetting.CreatePage.Url,
                    TabUrl = singleSetting.OverviewPage.Url,
                    HasSubscribersFilter = singleSetting.HasSubscribersFilter,

                };
            }
        }

        protected virtual bool IsPinActual(ICentralFeedItem item)
        {
            return _centralFeedService.IsPinActual(item);
        }

        protected void FillActivityDetailLinks(IEnumerable<ICentralFeedItem> items)
        {
            var currentPage = GetCurrentPage();

            foreach (var type in items.Select(i => i.Type).Distinct())
            {
                var service = _activitiesServiceFactory.GetService<IIntranetActivityService>(type);
                ViewData.SetActivityDetailsPageUrl(type, service.GetDetailsPage(currentPage).Url);
            }
        }

        protected virtual IPublishedContent GetCurrentPage()
        {
            if (_centralFeedContentHelper.IsCentralFeedPage(CurrentPage))
            {
                return _centralFeedContentHelper.GetOverviewPage();
            }

            return null;
        }

        protected virtual IEnumerable<ICentralFeedItem> ApplyFilters(IEnumerable<ICentralFeedItem> items, CentralFeedListModel model, CentralFeedSettings settings)
        {
            if (model.ShowSubscribed.GetValueOrDefault() && settings.HasSubscribersFilter)
            {
                items = items.Where(i => i is ISubscribable && subscribeService.IsSubscribed(intranetUserService.GetCurrentUser().Id, (ISubscribable)i));
            }

            if (!model.IncludeBulletin.GetValueOrDefault() && settings.HasBulletinFilter)
            {
                items = items.Where(i => i.Type != IntranetActivityTypeEnum.Bulletin);
            }

            if (model.ShowPinned && settings.HasPinnedFilter)
            {
                items = items.Where(i => i.IsPinned);
            }

            return items;
        }

    }
}