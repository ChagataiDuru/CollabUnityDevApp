using System.ComponentModel.DataAnnotations;

namespace UnityDevHub.API.Data.Entities;

public class TaskTag
{
    public Guid Id { get; set; }

    public Guid TaskId { get; set; }
    public ProjectTask? Task { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Color { get; set; } = "#6366f1";
}
