using UnityDevHub.API.Data.Entities;

namespace UnityDevHub.API.Models.DevOps
{
    public class RepositoryDto
    {
        public int Id { get; set; }
        public Guid ProjectId { get; set; }
        public RepositoryType Type { get; set; }
        public string Url { get; set; } = string.Empty;
        public string? WebhookSecret { get; set; }
    }
}
