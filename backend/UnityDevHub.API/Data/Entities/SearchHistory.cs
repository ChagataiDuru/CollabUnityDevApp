using System.ComponentModel.DataAnnotations;

namespace UnityDevHub.API.Data.Entities;

public class SearchHistory
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }

    [Required]
    [MaxLength(255)]
    public string Query { get; set; } = string.Empty;

    public int? ResultCount { get; set; }

    public DateTime SearchedAt { get; set; } = DateTime.UtcNow;
}
