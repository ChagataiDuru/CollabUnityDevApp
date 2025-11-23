using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnityDevHub.API.Data.Entities
{
    public class TimeLog
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid TaskId { get; set; }

        [ForeignKey("TaskId")]
        public ProjectTask Task { get; set; } = null!;

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        [Required]
        public int DurationMinutes { get; set; }

        public string? Description { get; set; }

        public bool IsManual { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
