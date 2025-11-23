using System.ComponentModel.DataAnnotations;

namespace UnityDevHub.API.Models.TaskColumn;

public class UpdateColumnDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Color { get; set; } = "#64748b";
}
