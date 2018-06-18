using Uintra.Core;
using Uintra.Core.Extensions;

namespace Uintra.CentralFeed
{
    public class FeedStateService : CookieStateService<FeedFiltersState>
    {
        public FeedStateService(ICookieProvider cookieProvider) : base(cookieProvider)
        {
        }

        protected override string StateCookieName { get; } = "feedFiltersState";

        public override FeedFiltersState GetDefaults() =>
            new FeedFiltersState
            {
                BulletinFilterSelected = true,
                SelectedActivityTypeId = CentralFeedTypeEnum.All.ToInt()
            };
    }
}