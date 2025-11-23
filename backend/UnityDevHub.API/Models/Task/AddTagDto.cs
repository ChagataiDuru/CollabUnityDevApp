using System.ComponentModel.DataAnnotations;

namespace UnityDevHub.API.Models.Task;

public class AddTagDto
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(7)]
    public string Color { get; set; } = "#6366f1"; // Default indigo
}
