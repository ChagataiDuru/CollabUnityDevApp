using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using UnityDevHub.API.Data;
using UnityDevHub.API.Data.Entities;
using UnityDevHub.API.Models.DevOps;

namespace UnityDevHub.API.Services
{
    /// <summary>
    /// Implementation of the IVcsService interface.
    /// </summary>
    public class VcsService : IVcsService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VcsService> _logger;

        public VcsService(ApplicationDbContext context, ILogger<VcsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<RepositoryDto> AddRepositoryAsync(Guid projectId, AddRepositoryDto dto)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                throw new KeyNotFoundException("Project not found");
            }

            var repository = new Repository
            {
                ProjectId = projectId,
                Type = dto.Type,
                Url = dto.Url,
                WebhookSecret = dto.WebhookSecret
            };

            _context.Repositories.Add(repository);
            await _context.SaveChangesAsync();

            return new RepositoryDto
            {
                Id = repository.Id,
                ProjectId = repository.ProjectId,
                Type = repository.Type,
                Url = repository.Url,
                WebhookSecret = repository.WebhookSecret
            };
        }

        /// <inheritdoc />
        public async Task<IEnumerable<RepositoryDto>> GetRepositoriesAsync(Guid projectId)
        {
            return await _context.Repositories
                .Where(r => r.ProjectId == projectId)
                .Select(r => new RepositoryDto
                {
                    Id = r.Id,
                    ProjectId = r.ProjectId,
                    Type = r.Type,
                    Url = r.Url,
                    WebhookSecret = r.WebhookSecret
                })
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task ProcessGitHubWebhookAsync(string payload, string signature)
        {
            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;

            // 1. Identify Repository
            if (!root.TryGetProperty("repository", out var repoElement) ||
                !repoElement.TryGetProperty("html_url", out var urlElement))
            {
                _logger.LogWarning("Invalid GitHub payload: missing repository url");
                return;
            }

            var repoUrl = urlElement.GetString();
            var repository = await _context.Repositories
                .Include(r => r.Project)
                .FirstOrDefaultAsync(r => r.Url == repoUrl && r.Type == RepositoryType.GitHub);

            if (repository == null)
            {
                _logger.LogWarning($"Repository not found for URL: {repoUrl}");
                return;
            }

            // Verify signature if secret is present
            if (!string.IsNullOrEmpty(repository.WebhookSecret))
            {
                // TODO: Implement signature verification
                // HMACSHA256 with repository.WebhookSecret
            }

            // 2. Process Commits
            if (root.TryGetProperty("commits", out var commitsElement))
            {
                foreach (var commitElement in commitsElement.EnumerateArray())
                {
                    var hash = commitElement.GetProperty("id").GetString() ?? "";
                    var message = commitElement.GetProperty("message").GetString() ?? "";
                    var url = commitElement.GetProperty("url").GetString() ?? "";
                    var timestampStr = commitElement.GetProperty("timestamp").GetString();
                    var timestamp = DateTime.TryParse(timestampStr, out var ts) ? ts : DateTime.UtcNow;
                    
                    var authorName = "Unknown";
                    if (commitElement.TryGetProperty("author", out var authorElement))
                    {
                        authorName = authorElement.GetProperty("name").GetString() ?? "Unknown";
                    }

                    // Check if commit already exists
                    if (await _context.Commits.AnyAsync(c => c.Hash == hash && c.RepositoryId == repository.Id))
                    {
                        continue;
                    }

                    // Parse Task ID from message (e.g. "Fixes #123")
                    Guid? taskId = null;
                    var match = Regex.Match(message, @"#(\d+)");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int taskNumber))
                    {
                        var task = await _context.Tasks
                            .FirstOrDefaultAsync(t => t.ProjectId == repository.ProjectId && t.TaskNumber == taskNumber);
                        
                        if (task != null)
                        {
                            taskId = task.Id;
                        }
                    }

                    var commit = new Commit
                    {
                        RepositoryId = repository.Id,
                        TaskId = taskId,
                        Hash = hash,
                        Message = message,
                        AuthorName = authorName,
                        Timestamp = timestamp.ToUniversalTime(),
                        Url = url
                    };

                    _context.Commits.Add(commit);
                }
                await _context.SaveChangesAsync();
            }
        }
    }
}
