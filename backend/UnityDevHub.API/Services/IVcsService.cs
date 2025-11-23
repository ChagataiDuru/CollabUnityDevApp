using UnityDevHub.API.Data.Entities;
using UnityDevHub.API.Models.DevOps;

namespace UnityDevHub.API.Services
{
    public interface IVcsService
    {
        Task<RepositoryDto> AddRepositoryAsync(Guid projectId, AddRepositoryDto dto);
        Task<IEnumerable<RepositoryDto>> GetRepositoriesAsync(Guid projectId);
        Task ProcessGitHubWebhookAsync(string payload, string signature);
    }
}
