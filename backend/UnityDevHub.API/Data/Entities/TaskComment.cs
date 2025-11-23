using System.ComponentModel.DataAnnotations;

namespace UnityDevHub.API.Data.Entities;

public class TaskComment
{
    public Guid Id { get; set; }

    public Guid TaskId { get; set; }
    public ProjectTask? Task { get; set; }

    public Guid? UserId { get; set; }
    public User? User { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
