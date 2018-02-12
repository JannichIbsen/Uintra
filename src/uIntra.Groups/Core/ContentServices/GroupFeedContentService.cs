﻿using System;
using System.Collections.Generic;
using System.Linq;
using Uintra.CentralFeed;
using Uintra.Core.Activity;
using Uintra.Core.Extensions;
using Uintra.Core.Grid;
using Uintra.Core.TypeProviders;
using Uintra.Core.User;
using Umbraco.Core.Models;
using Umbraco.Web;
using static Uintra.Groups.Constants.GroupConstants;

namespace Uintra.Groups
{
    public class GroupFeedContentService : FeedContentServiceBase, IGroupFeedContentService
    {
        private readonly IGroupService _groupService;
        private readonly IGroupFeedLinkService _groupFeedLinkService;
        private readonly IGroupContentProvider _contentProvider;
        private readonly IActivityTypeProvider _activityTypeProvider;

        protected override string FeedPluginAlias { get; } = GroupFeedPluginAlias;
        protected override string ActivityCreatePluginAlias { get; } = GroupActivityCreatePluginAlias;

        public GroupFeedContentService(
            IFeedTypeProvider feedTypeProvider,
            IGridHelper gridHelper,
            IGroupService groupService,
            IGroupFeedLinkService groupFeedLinkService,
            IGroupContentProvider contentProvider,
            IActivityTypeProvider activityTypeProvider)
              : base(feedTypeProvider, gridHelper)
        {
            _groupService = groupService;
            _groupFeedLinkService = groupFeedLinkService;
            _contentProvider = contentProvider;
            _activityTypeProvider = activityTypeProvider;
        }

        public ActivityFeedTabModel GetMainFeedTab(IPublishedContent currentPage, Guid groupId)
        {
            var groupRoom = _contentProvider.GetGroupRoomPage();
            var type = GetFeedTabType(groupRoom);
            var result = new ActivityFeedTabModel
            {
                Content = groupRoom,
                Type = type,
                IsActive = groupRoom.Id == currentPage.Id,
                Links = _groupFeedLinkService.GetCreateLinks(type, groupId)
            };
            return result;
        }

        public IEnumerable<ActivityFeedTabModel> GetActivityTabs(IPublishedContent currentPage, IIntranetUser user, Guid groupId)
        {
            yield return GetMainFeedTab(currentPage, groupId);

            foreach (var content in _contentProvider.GetRelatedPages())
            {
                var tabType = GetFeedTabType(content);

                var activityTypeIds = _activityTypeProvider.All.Select(t => t.ToInt());

                if (activityTypeIds.Contains(tabType.ToInt()))
                {
                    var tab = new ActivityFeedTabModel
                    {
                        Content = content,
                        Type = tabType,
                        IsActive = content.IsAncestorOrSelf(currentPage),
                        Links = _groupFeedLinkService.GetCreateLinks(tabType, groupId)
                    };

                    yield return tab;
                }
            }
        }

        public IEnumerable<PageTabModel> GetPageTabs(IPublishedContent currentPage, IIntranetUser user, Guid groupId)
        {
            Func<IPublishedContent, bool> skipPage = GetPageSkipResolver(user, groupId);

            foreach (var content in _contentProvider.GetRelatedPages())
            {
                if (skipPage(content)) continue;
                var tabType = GetFeedTabType(content);
                var activityTypeIds = _activityTypeProvider.All.Select(t => t.ToInt());
                if (activityTypeIds.Contains(tabType.ToInt())) continue;

                yield return GetPageTab(currentPage, content, groupId);
            }
        }

        private Func<IPublishedContent, bool> GetPageSkipResolver(IIntranetUser user, Guid groupId)
        {
            var canEdit = _groupService.CanEdit(groupId, user);
            var editGroupPage = _contentProvider.GetEditPage();

            var deactivatedPage = _contentProvider.GetDeactivatedGroupPage();

            bool SkipPage(IPublishedContent content) => !canEdit && AreSamePages(editGroupPage, content) || AreSamePages(deactivatedPage, content);
            return SkipPage;
        }

        private PageTabModel GetPageTab(IPublishedContent currentPage, IPublishedContent content, Guid groupId)
        {
            return new PageTabModel
            {
                Content = content,
                IsActive = content.IsAncestorOrSelf(currentPage),
                Title = content.Name,
                Link = content.Url.AddGroupId(groupId)
            };
        }

        private static bool AreSamePages(IPublishedContent f, IPublishedContent s) => f.Id == s.Id;
    }
}