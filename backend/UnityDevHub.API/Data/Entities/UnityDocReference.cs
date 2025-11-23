using System.ComponentModel.DataAnnotations;

namespace UnityDevHub.API.Data.Entities;

public class UnityDocReference
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Url { get; set; } = string.Empty;

    public string? Description { get; set; }
    public string? Notes { get; set; }

    public Guid? SavedById { get; set; }
    public User? SavedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
