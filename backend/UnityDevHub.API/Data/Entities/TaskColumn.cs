using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnityDevHub.API.Data.Entities;

public class TaskColumn
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int Position { get; set; }

    [MaxLength(50)]
    public string Color { get; set; } = "#64748b";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
