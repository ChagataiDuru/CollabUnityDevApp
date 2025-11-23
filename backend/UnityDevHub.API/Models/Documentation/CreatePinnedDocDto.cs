using System.ComponentModel.DataAnnotations;

namespace UnityDevHub.API.Models.Documentation
{
    public class CreatePinnedDocDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Url { get; set; } = string.Empty;

        public string? Description { get; set; }
        public string? Notes { get; set; }
    }
}
