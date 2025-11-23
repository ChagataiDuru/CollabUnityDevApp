using System.ComponentModel.DataAnnotations;

namespace UnityDevHub.API.Models.Task;

public class CreateChecklistItemDto
{
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;
}
