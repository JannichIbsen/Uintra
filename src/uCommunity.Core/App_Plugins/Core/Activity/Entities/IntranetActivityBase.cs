﻿using System;
using Newtonsoft.Json;

namespace uCommunity.Core.App_Plugins.Core.Activity.Entities
{
    public abstract class IntranetActivityBase
    {
        [JsonIgnore]
        public Guid Id { get; set; }

        [JsonIgnore]
        public IntranetActivityTypeEnum Type { get; set; }

        [JsonIgnore]
        public DateTime CreatedDate { get; set; }

        [JsonIgnore]
        public DateTime ModifyDate { get; set; }

        public string Title { get; set; }

        public string Teaser { get; set; }
            
        public string Description { get; set; }

        public bool IsHidden { get; set; }
    }
}