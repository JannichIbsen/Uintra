using System.Collections.Generic;
using Compent.Uintra.Hubs;
using Uintra.Core.Activity;
using Uintra.Core.Jobs;

namespace Compent.Uintra.Jobs
{
    public class ExtendedUpdateActivityCacheJob : UpdateActivityCacheJob
    {
        private readonly IFeedHubService _feedHubService;

        public ExtendedUpdateActivityCacheJob(IEnumerable<IIntranetActivityService<IIntranetActivity>> activityServices, IFeedHubService feedHubService): base(activityServices)
        {
            _feedHubService = feedHubService;
        }


        public override void Action()
        {
            if (ProcessActivities())
            {
                _feedHubService.NotifyFeedUpdate();
            }   
        }
    }
}