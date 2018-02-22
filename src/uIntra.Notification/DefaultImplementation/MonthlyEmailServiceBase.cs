﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BCLExtensions;
using Extensions;
using Uintra.Core.Activity;
using Uintra.Core.ApplicationSettings;
using Uintra.Core.Exceptions;
using Uintra.Core.Extensions;
using Uintra.Core.User;
using Uintra.Notification.Base;
using Uintra.Notification.Configuration;
using Uintra.Notification.MailModels;

namespace Uintra.Notification
{
    public abstract class MonthlyEmailServiceBase : IMonthlyEmailService
    {
        private readonly IMailService _mailService;
        private readonly IExceptionLogger _logger;
        private readonly IIntranetUserService<IIntranetUser> _intranetUserService;
        private readonly IApplicationSettings _applicationSettings;

        protected MonthlyEmailServiceBase(IMailService mailService,
            IIntranetUserService<IIntranetUser> intranetUserService,
            IExceptionLogger logger,
            IApplicationSettings applicationSettings)
        {
            _mailService = mailService;
            _intranetUserService = intranetUserService;
            _logger = logger;
            _applicationSettings = applicationSettings;
        }

        ///<summary>
        ///This method created only for QA-controller using cause it doesn't contain day  checking condition
        ///</summary>
        public void CreateAndSendMail()
        {
            var currentDate = DateTime.Now;
            var allUsers = _intranetUserService.GetAll();
            var monthlyMails = allUsers
                .Select(user => GetUserActivitiesFilteredByUserTags(user.Id).Map(userActivities => TryGetMonthlyMail(userActivities, user)))
                .ToList();

            foreach (var monthlyMail in monthlyMails)
            {
                if (!monthlyMail.HasValue)
                {
                    continue;
                }

                monthlyMail.Do(some: mail =>
                {
                    try
                    {
                        
                        _mailService.SendMailByTypeAndDay(
                            mail.monthlyMail,
                            mail.user.Email,
                            currentDate,
                            NotificationTypeEnum.MonthlyMail);
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(ex);
                    }
                });
            }
        }

        public void ProcessMonthlyEmail()
        {
            if (!IsSendingDay())
            {
                return;
            }

            CreateAndSendMail();
        }

        protected (IIntranetUser user, MonthlyMailBase monthlyMail)? TryGetMonthlyMail(
            IEnumerable<(IIntranetActivity activity, string detailsLink)> activities,
            IIntranetUser user)
        {
            var activityList = activities.AsList();
            if (activityList.Any())
            {
                var activityListString = GetActivityListString(activityList);
                var monthlyMail = GetMonthlyMailModel<MonthlyMailBase>(activityListString, user);
                return (user, monthlyMail);
            }
            else
            {
                return default;
            }
        }

        protected abstract IEnumerable<(IIntranetActivity activity, string detailsLink)> GetUserActivitiesFilteredByUserTags(Guid userId);

        protected virtual T GetMonthlyMailModel<T>(string userActivities, IIntranetUser user) where T : MonthlyMailBase, new()
        {
            var recipient = new MailRecipient { Email = user.Email, Name = user.DisplayedName };
            return new T
            {
                FullName = user.DisplayedName,
                ActivityList = userActivities,
                Recipients = recipient.ToListOfOne()
            };
        }

        protected virtual bool IsSendingDay()
        {
            var currentDate = DateTime.Now;

            if (currentDate.Day != _applicationSettings.MonthlyEmailJobDay) return false;

            return true;
        }

        private string GetActivityListString(IEnumerable<(IIntranetActivity activity, string link)> activities) => activities
            .Aggregate(
                new StringBuilder(),
                (builder, activity) => builder.AppendLine($"<a href='{activity.link}'>{activity.activity.Title}</a></br>"))
            .ToString();

    }
}
