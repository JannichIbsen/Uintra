namespace Uintra.CentralFeed
{
    public class FeedFilterStateUpdateModel
    {
        public bool ShowFilters { get; set; }
        public bool ShowSubscribed { get; set; }
        public bool ShowPinned { get; set; }
        public bool IncludeBulletin { get; set; }
        public int SelectedActivityTypeId { get; set; }
    }
}