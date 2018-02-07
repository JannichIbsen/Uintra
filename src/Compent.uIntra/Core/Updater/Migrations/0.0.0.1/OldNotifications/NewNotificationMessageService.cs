using System;
using uIntra.Core.TypeProviders;
using uIntra.Core.User;
using uIntra.Notification;
using uIntra.Notification.Base;
using uIntra.Notification.Configuration;

namespace Compent.uIntra.Installer.Migrations
{
    public class NewNotificationMessageService
    {
        private readonly INotificationModelMapper<UiNotifierTemplate, UiNotificationMessage> _notificationModelMapper;
        private readonly INotificationSettingsService _notificationSettingsService;
        private readonly IIntranetUserService<IIntranetUser> _intranetUserService;

        public NewNotificationMessageService(
            INotificationModelMapper<UiNotifierTemplate, UiNotificationMessage> notificationModelMapper,
            INotificationSettingsService notificationSettingsService,
            IIntranetUserService<IIntranetUser> intranetUserService)
        {
            _notificationModelMapper = notificationModelMapper;
            _notificationSettingsService = notificationSettingsService;
            _intranetUserService = intranetUserService;
        }

        private Enum UiNotifierType => NotifierTypeEnum.UiNotifier;

        internal UiNotificationMessage GetUiNotificationMessage(
            Guid receiverId,
            Enum activityType,
            Enum notificationType,
            INotifierDataValue newValue)
        {
            var notificationIdentity = new ActivityEventNotifierIdentity(activityType, notificationType, UiNotifierType);
            var template = _notificationSettingsService.Get<UiNotifierTemplate>(notificationIdentity).Template;
            var receiver = _intranetUserService.Get(receiverId);
            var message = _notificationModelMapper.Map(newValue, template, receiver);
            return message;
        }
    }
}