namespace UnityDevHub.API.Models.Task
{
    public class CommitDto
    {
        public int Id { get; set; }
        public string Hash { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Url { get; set; } = string.Empty;
    }
}
