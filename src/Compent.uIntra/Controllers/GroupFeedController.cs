﻿using uIntra.CentralFeed;
using uIntra.Core.Activity;
using uIntra.Core.User;
using uIntra.Groups;
using uIntra.Groups.Web;
using uIntra.Subscribe;

namespace Compent.uIntra.Controllers
{
    public class GroupFeedController : GroupFeedControllerBase
    {
        public GroupFeedController(
            ICentralFeedContentHelper centralFeedContentHelper,
            ISubscribeService subscribeService,
            ICentralFeedService centralFeedService,
            IActivitiesServiceFactory activitiesServiceFactory,
            IIntranetUserContentHelper intranetUserContentHelper,
            ICentralFeedTypeProvider centralFeedTypeProvider,
            IIntranetUserService<IIntranetUser> intranetUserService,
            IGroupContentHelper groupContentHelper) 
            : base(centralFeedContentHelper, subscribeService, centralFeedService, activitiesServiceFactory, intranetUserContentHelper, centralFeedTypeProvider, intranetUserService, groupContentHelper)
        {
        }
    }
}