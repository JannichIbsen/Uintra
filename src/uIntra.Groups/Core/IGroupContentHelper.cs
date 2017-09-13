using System;
using System.Collections.Generic;
using uIntra.CentralFeed;
using uIntra.Core.TypeProviders;
using uIntra.Core.User;
using Umbraco.Core.Models;

namespace uIntra.Groups
{
    public interface IGroupContentHelper
    {
        IPublishedContent GetGroupRoomPage();
        IPublishedContent GetCreateGroupPage();
        IPublishedContent GetOverviewPage();
        IPublishedContent GetEditPage();
        bool IsGroupRoomPage(IPublishedContent currentPage);
        IEnumerable<FeedTabModel> GetTabs(Guid groupId, IIntranetUser user, IPublishedContent currentContent);
        IIntranetType GetTabType(IPublishedContent content);
        bool IsGroupPage(IPublishedContent currentPage);
        IPublishedContent GetMyGroupsOverviewPage();
        IPublishedContent GetDeactivatedGroupPage();
    }
}