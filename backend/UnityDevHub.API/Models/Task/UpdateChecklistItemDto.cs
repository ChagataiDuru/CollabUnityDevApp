using System.ComponentModel.DataAnnotations;

namespace UnityDevHub.API.Models.Task;

public class UpdateChecklistItemDto
{
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;
    
    public bool IsCompleted { get; set; }
}
