using System.ComponentModel.DataAnnotations;

namespace UnityDevHub.API.Models.Project;

public class CreateProjectDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(50)]
    public string ColorTheme { get; set; } = "#6366f1";
}
