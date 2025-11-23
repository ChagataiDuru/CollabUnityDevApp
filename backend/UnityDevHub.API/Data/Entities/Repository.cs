using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnityDevHub.API.Data.Entities
{
    public enum RepositoryType
    {
        GitHub,
        GitLab
    }

    public class Repository
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid ProjectId { get; set; }

        [ForeignKey("ProjectId")]
        public Project Project { get; set; } = null!;

        [Required]
        public RepositoryType Type { get; set; }

        [Required]
        public string Url { get; set; } = string.Empty;

        public string? WebhookSecret { get; set; }

        public ICollection<Commit> Commits { get; set; } = new List<Commit>();
        public ICollection<Build> Builds { get; set; } = new List<Build>();
    }
}
