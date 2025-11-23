namespace UnityDevHub.API.Models.Wiki
{
    public class WikiPageDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid? ParentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid? LastEditorId { get; set; }
        public string? LastEditorName { get; set; }
        public List<WikiPageDto> Children { get; set; } = new List<WikiPageDto>();
    }
}
