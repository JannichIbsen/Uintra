﻿using System;
using uIntra.Core.TypeProviders;

namespace uIntra.CentralFeed
{
    public interface IFeedItem
    {
        Guid Id { get; }

        Guid CreatorId { get; set; }

        IIntranetType Type { get; }

        DateTime PublishDate { get; }

        DateTime ModifyDate { get; }

        bool IsPinned { get; }

        bool IsPinActual { get; }
    }
}