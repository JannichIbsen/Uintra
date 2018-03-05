using System;
using Newtonsoft.Json;
using Uintra.Core.Activity;
using Uintra.Core.Location;
using Uintra.Core.User;

namespace Uintra.News
{
    public interface INewsBase : IIntranetActivity
    {
        int? UmbracoCreatorId { get; set; }
        Guid CreatorId { get; set; }
        Guid OwnerId { get; set; }
        DateTime PublishDate { get; set; }
        DateTime? UnpublishDate { get; set; }
    }

    public class NewsBase : IntranetActivity, IHaveCreator, IHaveOwner, INewsBase
    {
        public int? UmbracoCreatorId { get; set; }
        public Guid CreatorId { get; set; }
        public Guid OwnerId { get; set; }
        public DateTime PublishDate { get; set; }
        public DateTime? UnpublishDate { get; set; }
    }
}