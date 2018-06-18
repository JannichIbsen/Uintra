using System;
using System.Collections.Generic;

namespace Uintra.CentralFeed
{
    public interface IFeedService
    {
        FeedSettings GetSettings(Enum type);
        IEnumerable<FeedSettings> GetAllSettings();
    }
}