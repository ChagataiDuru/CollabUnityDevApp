namespace UnityDevHub.API.Models.Documentation
{
    public class SearchHistoryDto
    {
        public Guid Id { get; set; }
        public string Query { get; set; } = string.Empty;
        public DateTime SearchedAt { get; set; }
    }
}
