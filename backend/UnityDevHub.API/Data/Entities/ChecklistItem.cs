using System.ComponentModel.DataAnnotations;

namespace UnityDevHub.API.Data.Entities;

public class ChecklistItem
{
    public Guid Id { get; set; }

    public Guid TaskId { get; set; }
    public ProjectTask? Task { get; set; }

    [Required]
    [MaxLength(255)]
    public string Text { get; set; } = string.Empty;

    public bool IsCompleted { get; set; } = false;

    public int Position { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
