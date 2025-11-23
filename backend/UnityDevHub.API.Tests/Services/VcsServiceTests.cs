using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using UnityDevHub.API.Data;
using UnityDevHub.API.Data.Entities;
using UnityDevHub.API.Models.DevOps;
using UnityDevHub.API.Services;
using Xunit;

namespace UnityDevHub.API.Tests.Services
{
    public class VcsServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<ILogger<VcsService>> _mockLogger;
        private readonly VcsService _vcsService;

        public VcsServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _mockLogger = new Mock<ILogger<VcsService>>();
            _vcsService = new VcsService(_context, _mockLogger.Object);
        }

        [Fact]
        public async Task AddRepositoryAsync_ShouldAddRepository_WhenProjectExists()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var project = new Project { Id = projectId, Name = "Test Project" };
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            var dto = new AddRepositoryDto
            {
                Type = RepositoryType.GitHub,
                Url = "https://github.com/test/repo",
                WebhookSecret = "secret"
            };

            // Act
            var result = await _vcsService.AddRepositoryAsync(projectId, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(projectId, result.ProjectId);
            Assert.Equal(dto.Url, result.Url);

            var repo = await _context.Repositories.FirstOrDefaultAsync(r => r.ProjectId == projectId);
            Assert.NotNull(repo);
        }

        [Fact]
        public async Task AddRepositoryAsync_ShouldThrowException_WhenProjectDoesNotExist()
        {
            // Arrange
            var dto = new AddRepositoryDto
            {
                Type = RepositoryType.GitHub,
                Url = "https://github.com/test/repo"
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _vcsService.AddRepositoryAsync(Guid.NewGuid(), dto));
        }

        [Fact]
        public async Task ProcessGitHubWebhookAsync_ShouldProcessCommits_WhenPayloadIsValid()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var project = new Project { Id = projectId, Name = "Test Project" };
            var repo = new Repository
            {
                Id = 1,
                ProjectId = projectId,
                Type = RepositoryType.GitHub,
                Url = "https://github.com/test/repo",
                WebhookSecret = "secret"
            };
            _context.Projects.Add(project);
            _context.Repositories.Add(repo);
            await _context.SaveChangesAsync();

            var payload = @"
            {
                ""repository"": {
                    ""html_url"": ""https://github.com/test/repo""
                },
                ""commits"": [
                    {
                        ""id"": ""commit_hash_1"",
                        ""message"": ""Initial commit"",
                        ""url"": ""http://commit/1"",
                        ""timestamp"": ""2023-01-01T12:00:00Z"",
                        ""author"": { ""name"": ""Author One"" }
                    }
                ]
            }";

            // Act
            await _vcsService.ProcessGitHubWebhookAsync(payload, "signature");

            // Assert
            var commit = await _context.Commits.FirstOrDefaultAsync(c => c.Hash == "commit_hash_1");
            Assert.NotNull(commit);
            Assert.Equal("Initial commit", commit.Message);
            Assert.Equal(repo.Id, commit.RepositoryId);
        }
    }
}
