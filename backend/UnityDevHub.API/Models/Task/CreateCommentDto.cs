using System.ComponentModel.DataAnnotations;

namespace UnityDevHub.API.Models.Task;

public class CreateCommentDto
{
    [Required]
    public string Content { get; set; } = string.Empty;
}
