﻿using Uintra.Events;

namespace Compent.Uintra.Core.Events
{
    public class EventExtendedEditModel : EventEditModel
    {
        public bool CanSubscribe { get; set; }
        public string SubscribeNotes { get; set; }
        public bool CanEditSubscribe { get; set; }
        public string TagIdsData { get; set; }
    }
}