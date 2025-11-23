using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UnityDevHub.API.Controllers;
using UnityDevHub.API.Data;
using UnityDevHub.API.Data.Entities;
using UnityDevHub.API.Models.Analytics;
using Xunit;

namespace UnityDevHub.API.Tests.Controllers
{
    public class AnalyticsControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly AnalyticsController _controller;
        private readonly Project _testProject;

        public AnalyticsControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            var user = new User { Id = Guid.NewGuid(), UserName = "testuser", FullName = "Test User" };
            _testProject = new Project { Id = Guid.NewGuid(), Name = "Test Project", CreatedById = user.Id };

            _context.Users.Add(user);
            _context.Projects.Add(_testProject);
            _context.SaveChanges();

            _controller = new AnalyticsController(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetCompletionRate_ShouldCalculateCorrectly()
        {
            // Setup columns
            var todoCol = new TaskColumn { Id = Guid.NewGuid(), ProjectId = _testProject.Id, Name = "To Do" };
            var doneCol = new TaskColumn { Id = Guid.NewGuid(), ProjectId = _testProject.Id, Name = "Done" };

            _context.TaskColumns.AddRange(todoCol, doneCol);

            // Setup tasks
            _context.Tasks.Add(new ProjectTask { Id = Guid.NewGuid(), ProjectId = _testProject.Id, Title = "Task 1", ColumnId = todoCol.Id });
            _context.Tasks.Add(new ProjectTask { Id = Guid.NewGuid(), ProjectId = _testProject.Id, Title = "Task 2", ColumnId = doneCol.Id });
            _context.Tasks.Add(new ProjectTask { Id = Guid.NewGuid(), ProjectId = _testProject.Id, Title = "Task 3", ColumnId = doneCol.Id });

            await _context.SaveChangesAsync();

            var result = await _controller.GetCompletionRate(_testProject.Id);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var rate = Assert.IsType<CompletionRateDto>(okResult.Value);

            Assert.Equal(3, rate.TotalTasks);
            Assert.Equal(2, rate.CompletedTasks);
            // 2/3 * 100 = 66.666...
            Assert.Equal(66.67, rate.RatePercentage);
        }

        [Fact]
        public async Task GetActivityHeatmap_ShouldAggregateData()
        {
            var date = DateTime.UtcNow.Date;

            // Task Created Today
            _context.Tasks.Add(new ProjectTask { Id = Guid.NewGuid(), ProjectId = _testProject.Id, Title = "Task 1", CreatedAt = date });

            // Wiki Page Created Today
            _context.WikiPages.Add(new WikiPage { Id = Guid.NewGuid(), ProjectId = _testProject.Id, Title = "Wiki 1", CreatedAt = date });

            await _context.SaveChangesAsync();

            var result = await _controller.GetActivityHeatmap(_testProject.Id, null);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var points = Assert.IsType<List<HeatmapPointDto>>(okResult.Value);

            // Should find the date with count 2
            var point = points.Find(p => p.Date == date);
            Assert.NotNull(point);
            Assert.Equal(2, point.Count);
        }
    }
}
