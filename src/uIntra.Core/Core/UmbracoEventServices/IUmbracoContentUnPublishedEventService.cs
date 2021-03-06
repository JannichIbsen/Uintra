﻿using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Publishing;

namespace Uintra.Core.UmbracoEventServices
{
    public interface IUmbracoContentUnPublishedEventService
    {
        void ProcessContentUnPublished(IPublishingStrategy sender, PublishEventArgs<IContent> e);
    }
}