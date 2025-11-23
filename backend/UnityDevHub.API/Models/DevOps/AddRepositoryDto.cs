using System.ComponentModel.DataAnnotations;
using UnityDevHub.API.Data.Entities;

namespace UnityDevHub.API.Models.DevOps
{
    public class AddRepositoryDto
    {
        [Required]
        public RepositoryType Type { get; set; }

        [Required]
        [Url]
        public string Url { get; set; } = string.Empty;

        public string? WebhookSecret { get; set; }
    }
}
