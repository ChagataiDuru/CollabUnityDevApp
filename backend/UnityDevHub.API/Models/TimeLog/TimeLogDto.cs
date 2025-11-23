namespace UnityDevHub.API.Models.TimeLog
{
    public class TimeLogDto
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int DurationMinutes { get; set; }
        public string? Description { get; set; }
        public bool IsManual { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
