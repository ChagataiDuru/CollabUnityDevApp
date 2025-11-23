namespace UnityDevHub.API.Models.Documentation
{
    public class PinnedDocDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Notes { get; set; }
        public Guid? SavedById { get; set; }
        public string? SavedByName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
