using System.ComponentModel.DataAnnotations;

namespace UnityDevHub.API.Data.Entities;

public class TaskAttachment
{
    public Guid Id { get; set; }

    public Guid TaskId { get; set; }
    public ProjectTask? Task { get; set; }

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    public long FileSize { get; set; }

    [MaxLength(100)]
    public string? ContentType { get; set; }

    public Guid? UploadedById { get; set; }
    public User? UploadedBy { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
