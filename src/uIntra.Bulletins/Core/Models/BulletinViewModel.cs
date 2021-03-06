﻿using System;
using Uintra.Core.Activity;
using Uintra.Core.LinkPreview;

namespace Uintra.Bulletins
{
    public class BulletinViewModel : IntranetActivityViewModelBase
    {
        public string Description { get; set; }
        public DateTime PublishDate { get; set; }
        public string Media { get; set; }
        public LinkPreviewViewModel LinkPreview { get; set; }
    }
}