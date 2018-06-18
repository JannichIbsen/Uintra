using System;
using System.Collections.Generic;
using System.Linq;
using Compent.Uintra.Hubs;
using Uintra.Core.Activity;
using Uintra.Core.Jobs.Models;
using Uintra.Core.PagePromotion;
using Uintra.Events;
using Uintra.News;

namespace Compent.Uintra.Jobs
{
    public class ActivityPublicationJob : BaseIntranetJob
    {
        private readonly IEnumerable<IIntranetActivityService<IIntranetActivity>> _activityServices;
        private readonly IFeedHubService _feedHubService;

        private const int DecisionTimeDifference = 1;

        public ActivityPublicationJob(IEnumerable<IIntranetActivityService<IIntranetActivity>> activityServices, IFeedHubService feedHubService)
        {
            _activityServices = activityServices;
            _feedHubService = feedHubService;
        }

        public override void Action()
        {
            var activities = _activityServices.SelectMany(service => service.GetAll());

            var currentTime = DateTime.UtcNow;
            if (activities.Any(activity => IsHavePublishStateChange(activity, currentTime)))
            {
                _feedHubService.NotifyFeedUpdate();
            }

            base.Action();
        }

        protected virtual bool IsHavePublishStateChange(IIntranetActivity activity, DateTime currentTime)
        {
            switch (activity)
            {
                case NewsBase news:
                    return JustChangedPublishState(news.PublishDate, currentTime) ||
                           news.UnpublishDate.HasValue && JustChangedPublishState(news.UnpublishDate.Value, currentTime);
                case EventBase @event:
                    return JustChangedPublishState(@event.PublishDate, currentTime);
                case PagePromotionBase pagePromotion:
                    return JustChangedPublishState(pagePromotion.PublishDate, currentTime);
                default:
                    return false;
            }
        }

        private static bool JustChangedPublishState(DateTime currentDate, DateTime changeDate) =>
            currentDate >= changeDate && currentDate <= currentDate.AddMinutes(DecisionTimeDifference);
    }
}