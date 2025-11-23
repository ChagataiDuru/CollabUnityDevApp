using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnityDevHub.API.Data.Entities
{
    public class Commit
    {
        [Key]
        public int Id { get; set; }

        public Guid? TaskId { get; set; }

        [ForeignKey("TaskId")]
        public ProjectTask? Task { get; set; }

        [Required]
        public int RepositoryId { get; set; }

        [ForeignKey("RepositoryId")]
        public Repository Repository { get; set; } = null!;

        [Required]
        public string Hash { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public string AuthorName { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; }

        public string Url { get; set; } = string.Empty;
    }
}
