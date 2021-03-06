﻿using System.Web.Mvc;
using Compent.Extensions;
using Uintra.Core.Activity;
using Uintra.Core.Extensions;
using Uintra.Subscribe;

namespace Compent.Uintra.Core.Updater.Migrations._0._0._0._1.Steps.AggregateSubsteps
{
    public class OldSubscribeSettingsMigration
    {
        public void Execute()
        {
            CreateActivitySubscribeSettings();
        }

        private void CreateActivitySubscribeSettings()
        {
            var intranetActivityRepository = DependencyResolver.Current.GetService<IIntranetActivityRepository>();
            var activitySubscribeSettingService = DependencyResolver.Current.GetService<IActivitySubscribeSettingService>();
            var events = intranetActivityRepository.FindAll(activity => activity.Type == (int)IntranetActivityTypeEnum.Events).AsList();

            foreach (var @event in events)
            {
                var data = @event.JsonData.Deserialize<OldEventBase>();

                activitySubscribeSettingService.Create(
                    new ActivitySubscribeSettingDto
                    {
                        ActivityId = @event.Id,
                        CanSubscribe = data.CanSubscribe
                    });
            }
        }
    }
}