﻿using System;
using System.Linq;
using Uintra.Core.User;
using Uintra.Notification;
using Uintra.Notification.Base;
using Uintra.Notification.Configuration;

namespace Compent.Uintra.Core.Notification
{
    public class PopupNotifierService : INotifierService
    {
        private readonly INotificationSettingsService _notificationSettingsService;
        private readonly IIntranetUserService<IIntranetUser> _intranetUserService;
        private readonly INotificationModelMapper<PopupNotifierTemplate, PopupNotificationMessage> _notificationModelMapper;
        private readonly IPopupNotificationService _notificationsService;
        public Enum Type => NotifierTypeEnum.PopupNotifier;

        public PopupNotifierService(
            INotificationSettingsService notificationSettingsService,
            IIntranetUserService<IIntranetUser> intranetUserService,
            INotificationModelMapper<PopupNotifierTemplate, PopupNotificationMessage> notificationModelMapper,
        IPopupNotificationService notificationsService
            )
        {
            _notificationSettingsService = notificationSettingsService;
            _intranetUserService = intranetUserService;
            _notificationModelMapper = notificationModelMapper;
            _notificationsService = notificationsService;
        }

        public void Notify(NotifierData data)
        {
            var identity = new ActivityEventIdentity(CommunicationTypeEnum.Member, data.NotificationType).AddNotifierIdentity(Type);
            var settings = _notificationSettingsService.Get<PopupNotifierTemplate>(identity);

            if (settings == null || !settings.IsEnabled) return;
            var receivers = _intranetUserService.GetMany(data.ReceiverIds);

            var messages = receivers.Select(r => _notificationModelMapper.Map(data.Value, settings.Template, r));
            _notificationsService.Notify(messages);
        }
    }
}