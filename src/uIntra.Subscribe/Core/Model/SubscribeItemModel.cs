﻿using System;

namespace Uintra.Subscribe
{
    public class SubscribeItemModel
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public bool IsNotificationDisabled { get; set; }
    }
}