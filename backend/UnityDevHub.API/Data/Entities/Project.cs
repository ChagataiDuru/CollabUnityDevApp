using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnityDevHub.API.Data.Entities;

public class Project
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(50)]
    public string ColorTheme { get; set; } = "#6366f1";

    public Guid? CreatedById { get; set; }
    public User? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Repository> Repositories { get; set; } = new List<Repository>();
    public ICollection<Build> Builds { get; set; } = new List<Build>();
    public ICollection<Sprint> Sprints { get; set; } = new List<Sprint>();
}
