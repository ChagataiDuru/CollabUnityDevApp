using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using UnityDevHub.API.Controllers;
using UnityDevHub.API.Data;
using UnityDevHub.API.Data.Entities;
using UnityDevHub.API.Hubs;
using UnityDevHub.API.Models.Task;
using Xunit;

namespace UnityDevHub.API.Tests.Controllers
{
    public class TasksControllerTests
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IHubContext<ProjectHub>> _mockHub;
        private readonly Mock<IClientProxy> _mockClientProxy;
        private readonly Mock<IHubClients> _mockHubClients;
        private readonly TasksController _controller;
        private readonly Guid _userId;

        public TasksControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            _mockHub = new Mock<IHubContext<ProjectHub>>();
            _mockHubClients = new Mock<IHubClients>();
            _mockClientProxy = new Mock<IClientProxy>();

            _mockHub.Setup(h => h.Clients).Returns(_mockHubClients.Object);
            _mockHubClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
            _mockHubClients.Setup(c => c.User(It.IsAny<string>())).Returns(_mockClientProxy.Object);

            _controller = new TasksController(_context, _mockHub.Object);

            _userId = Guid.NewGuid();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, _userId.ToString()),
                new Claim(ClaimTypes.Name, "testuser")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task CreateTask_ShouldCreateTask_WithCorrectPosition()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var columnId = Guid.NewGuid();

            // Pre-populate a task to test position increment
            var existingTask = new ProjectTask { Id = Guid.NewGuid(), ProjectId = projectId, ColumnId = columnId, Position = 0, Title = "Existing" };
            _context.Tasks.Add(existingTask);
            await _context.SaveChangesAsync();

            var dto = new CreateTaskDto
            {
                ColumnId = columnId,
                Title = "New Task",
                Description = "Desc",
                Priority = 1
            };

            // Act
            var result = await _controller.CreateTask(projectId, dto);

            // Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedDto = Assert.IsType<TaskDto>(actionResult.Value);

            Assert.Equal(1, returnedDto.Position); // Should be after existing task (0)
            Assert.Equal(projectId, returnedDto.ProjectId);

            // Verify SignalR call
            _mockClientProxy.Verify(c => c.SendCoreAsync("TaskCreated", It.Is<object[]>(o => o.Length == 1), default), Times.Once);
        }

        [Fact]
        public async Task MoveTask_ShouldReorderTasks_WithinSameColumn()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var columnId = Guid.NewGuid();

            var task1 = new ProjectTask { Id = Guid.NewGuid(), ProjectId = projectId, ColumnId = columnId, Position = 0, Title = "Task 1" };
            var task2 = new ProjectTask { Id = Guid.NewGuid(), ProjectId = projectId, ColumnId = columnId, Position = 1, Title = "Task 2" };
            var task3 = new ProjectTask { Id = Guid.NewGuid(), ProjectId = projectId, ColumnId = columnId, Position = 2, Title = "Task 3" };

            _context.Tasks.AddRange(task1, task2, task3);
            await _context.SaveChangesAsync();

            // Move Task 1 to Position 2 (end)
            var dto = new MoveTaskDto { NewColumnId = columnId, NewPosition = 2 };

            // Act
            var result = await _controller.MoveTask(task1.Id, dto);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var tasks = await _context.Tasks.OrderBy(t => t.Position).ToListAsync();

            // Expected Order: Task 2 (0), Task 3 (1), Task 1 (2)
            Assert.Equal(task2.Id, tasks[0].Id);
            Assert.Equal(0, tasks[0].Position);

            Assert.Equal(task3.Id, tasks[1].Id);
            Assert.Equal(1, tasks[1].Position);

            Assert.Equal(task1.Id, tasks[2].Id);
            Assert.Equal(2, tasks[2].Position);
        }

        [Fact]
        public async Task MoveTask_ShouldMoveTask_ToDifferentColumn()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var col1 = Guid.NewGuid();
            var col2 = Guid.NewGuid();

            var task1 = new ProjectTask { Id = Guid.NewGuid(), ProjectId = projectId, ColumnId = col1, Position = 0, Title = "Task 1" };
            var task2 = new ProjectTask { Id = Guid.NewGuid(), ProjectId = projectId, ColumnId = col2, Position = 0, Title = "Task 2" };

            _context.Tasks.AddRange(task1, task2);
            await _context.SaveChangesAsync();

            // Move Task 1 to Column 2, Position 0 (before Task 2)
            var dto = new MoveTaskDto { NewColumnId = col2, NewPosition = 0 };

            // Act
            var result = await _controller.MoveTask(task1.Id, dto);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var t1 = await _context.Tasks.FindAsync(task1.Id);
            var t2 = await _context.Tasks.FindAsync(task2.Id);

            Assert.Equal(col2, t1.ColumnId);
            Assert.Equal(0, t1.Position);

            Assert.Equal(1, t2.Position); // Task 2 should shift down
        }
    }
}
