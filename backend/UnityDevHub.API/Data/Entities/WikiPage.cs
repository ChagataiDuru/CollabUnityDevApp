using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnityDevHub.API.Data.Entities
{
    public class WikiPage
    {
        [Key]
        public Guid Id { get; set; }

        public Guid ProjectId { get; set; }
        [ForeignKey("ProjectId")]
        public Project Project { get; set; } = null!;

        public Guid? ParentId { get; set; }
        [ForeignKey("ParentId")]
        public WikiPage? Parent { get; set; }

        public ICollection<WikiPage> Children { get; set; } = new List<WikiPage>();

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Guid? LastEditorId { get; set; }
        [ForeignKey("LastEditorId")]
        public User? LastEditor { get; set; }
    }
}
