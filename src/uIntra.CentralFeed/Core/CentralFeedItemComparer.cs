﻿using System;
using System.Collections.Generic;

namespace uIntra.CentralFeed
{
    public class CentralFeedItemComparer : IComparer<ICentralFeedItem>
    {
        private readonly DateTime _currentDate;

        public CentralFeedItemComparer()
        {
            _currentDate = DateTime.Now.Date;
        }

        public int Compare(ICentralFeedItem x, ICentralFeedItem y)
        {
            if (IsCurrent(x) && IsCurrent(y))
            {
                return DateTime.Compare(y.PublishDate, x.PublishDate);
            }

            if (IsFuture(x) && IsFuture(y))
            {
                return DateTime.Compare(x.PublishDate, y.PublishDate);
            }

            if (IsPast(x) && IsPast(y))
            {
                return DateTime.Compare(y.PublishDate, x.PublishDate);
            }

            if (IsCurrent(x) && IsFuture(y) || IsCurrent(x) && IsPast(y))
            {
                return -1;
            }

            if (IsFuture(x) && IsPast(y))
            {
                return -1;
            }

            if (IsFuture(x) && IsCurrent(y))
            {
                return 1;
            }

            if (IsPast(x) && IsCurrent(y) || IsPast(x) && IsFuture(y))
            {
                return 1;
            }

            return DateTime.Compare(x.PublishDate, y.PublishDate);
        }

        private bool IsFuture(ICentralFeedItem item)
        {
            return item.PublishDate.Date > _currentDate.Date;
        }

        private bool IsCurrent(ICentralFeedItem item)
        {
            return item.PublishDate.Date == _currentDate.Date;
        }

        private bool IsPast(ICentralFeedItem item)
        {
            return item.PublishDate.Date < _currentDate.Date;
        }
    }
}