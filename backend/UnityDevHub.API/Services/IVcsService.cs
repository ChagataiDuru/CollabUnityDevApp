using UnityDevHub.API.Data.Entities;
using UnityDevHub.API.Models.DevOps;

namespace UnityDevHub.API.Services
{
    /// <summary>
    /// Interface for version control system services.
    /// </summary>
    public interface IVcsService
    {
        /// <summary>
        /// Adds a repository to a project.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project.</param>
        /// <param name="dto">The repository details.</param>
        /// <returns>The added repository information.</returns>
        Task<RepositoryDto> AddRepositoryAsync(Guid projectId, AddRepositoryDto dto);

        /// <summary>
        /// Retrieves all repositories associated with a project.
        /// </summary>
        /// <param name="projectId">The unique identifier of the project.</param>
        /// <returns>A list of repositories.</returns>
        Task<IEnumerable<RepositoryDto>> GetRepositoriesAsync(Guid projectId);

        /// <summary>
        /// Processes a GitHub webhook payload.
        /// </summary>
        /// <param name="payload">The webhook payload content.</param>
        /// <param name="signature">The webhook signature for verification.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task ProcessGitHubWebhookAsync(string payload, string signature);
    }
}
