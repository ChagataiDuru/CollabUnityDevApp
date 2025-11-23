using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnityDevHub.API.Data.Entities
{
    public enum BuildStatus
    {
        Pending,
        Success,
        Failure,
        Cancelled
    }

    public class Build
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid ProjectId { get; set; }

        [ForeignKey("ProjectId")]
        public Project Project { get; set; } = null!;

        [Required]
        public int RepositoryId { get; set; }

        [ForeignKey("RepositoryId")]
        public Repository Repository { get; set; } = null!;

        [Required]
        public BuildStatus Status { get; set; }

        public string CommitHash { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; }

        public string Url { get; set; } = string.Empty;
    }
}
