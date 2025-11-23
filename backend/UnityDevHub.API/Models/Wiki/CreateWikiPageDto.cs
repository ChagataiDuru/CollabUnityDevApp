using System.ComponentModel.DataAnnotations;

namespace UnityDevHub.API.Models.Wiki
{
    public class CreateWikiPageDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public Guid? ParentId { get; set; }
    }
}
