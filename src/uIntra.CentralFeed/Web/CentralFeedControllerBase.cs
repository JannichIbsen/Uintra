﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using uIntra.CentralFeed.App_Plugins.CentralFeed.Models;
using uIntra.Core.Activity;
using uIntra.Core.Extentions;
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
        protected const int ItemsPerPage = 8;

        protected CentralFeedControllerBase(ICentralFeedService centralFeedService, ICentralFeedContentHelper centralFeedContentHelper, IActivitiesServiceFactory activitiesServiceFactory)
        {
            _centralFeedService = centralFeedService;
            _centralFeedContentHelper = centralFeedContentHelper;
            _activitiesServiceFactory = activitiesServiceFactory;
        }

        public virtual ActionResult Overview()
        {
            var tabType = _centralFeedContentHelper.GetTabType(CurrentPage);
            var model = new CentralFeedOverviewModel
            {
                CurrentType = tabType
            };
            return PartialView(OverviewViewPath, model);
        }

        public virtual ActionResult List(CentralFeedListModel model)
        {
            var items = GetCentralFeedItems(model.Type.GetHashCode().ToEnum<IntranetActivityTypeEnum>());

            var currentVersion = _centralFeedService.GetFeedVersion(items);

            if (model.Version.HasValue && currentVersion == model.Version.Value)
            {
                return null;
            }

            var take = model.Page * ItemsPerPage;
            var pagedItemsList = items.OrderBy(IsPinActual).ThenByDescending(el => el.PublishDate).Take(take).ToList();

            FillActivityDetailLinks(pagedItemsList);

            var centralFeedModel = new CentralFeedListViewModel
            {
                Version = _centralFeedService.GetFeedVersion(items),
                Items = pagedItemsList,
                Settings = _centralFeedService.GetAllSettings(),
                Type = model.Type,
                BlockScrolling = items.Count < take,
                ShowSubscribed = model.ShowSubscribed ?? false
            };

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
                    HasSubscribersFilter = singleSetting.HasSubscribersFitler
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
    }
}