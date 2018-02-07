﻿using System;
using uIntra.Core.Links;
using uIntra.Core.User;

namespace uIntra.Bulletins
{
    public class BulletinPreviewViewModel
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public DateTime PublishDate { get; set; }
        public IIntranetUser Owner { get; set; }
        public Enum ActivityType { get; set; }
        public ActivityLinks Links { get; set; }
    }
}