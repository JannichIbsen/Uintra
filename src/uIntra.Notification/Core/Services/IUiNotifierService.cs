﻿using System;
using System.Collections.Generic;

namespace uIntra.Notification
{
    public interface IUiNotifierService : INotifierService
    {
        IEnumerable<Notification> GetMany(Guid receiverId, int count, out int totalCount);
        int GetNotNotifiedCount(Guid receiverId);
        void ViewNotification(Guid id);
    }
}