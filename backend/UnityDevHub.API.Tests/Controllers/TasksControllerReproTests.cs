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
using UnityDevHub.API.Services;
using UnityDevHub.API.Controllers;
using UnityDevHub.API.Data;
using UnityDevHub.API.Data.Entities;
using UnityDevHub.API.Hubs;
using UnityDevHub.API.Models.Task;
using Xunit;

namespace UnityDevHub.API.Tests.Controllers
{
    public class TasksControllerReproTests
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IHubContext<ProjectHub>> _mockHub;
        private readonly Mock<IClientProxy> _mockClientProxy;
        private readonly Mock<IHubClients> _mockHubClients;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly TasksController _controller;
        private readonly Guid _userId;

        public TasksControllerReproTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            _mockHub = new Mock<IHubContext<ProjectHub>>();
            _mockHubClients = new Mock<IHubClients>();
            _mockClientProxy = new Mock<IClientProxy>();
            _mockNotificationService = new Mock<INotificationService>();

            _mockHub.Setup(h => h.Clients).Returns(_mockHubClients.Object);
            _mockHubClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
            _mockHubClients.Setup(c => c.User(It.IsAny<string>())).Returns(_mockClientProxy.Object);

            _controller = new TasksController(_context, _mockHub.Object, _mockNotificationService.Object);

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
        public async Task MoveTask_ShouldNotThrowException_WhenNewPositionIsOutOfBounds()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var columnId = Guid.NewGuid();

            var task1 = new ProjectTask { Id = Guid.NewGuid(), ProjectId = projectId, ColumnId = columnId, Position = 0, Title = "Task 1" };
            var task2 = new ProjectTask { Id = Guid.NewGuid(), ProjectId = projectId, ColumnId = columnId, Position = 1, Title = "Task 2" };

            _context.Tasks.AddRange(task1, task2);
            await _context.SaveChangesAsync();

            // Move Task 1 to Position 10 (which is out of bounds, as there is only 1 other task in the list when we exclude task1)
            // tasksInColumn will have [Task 2], count = 1.
            // Insert(10, task1) will throw.
            var dto = new MoveTaskDto { NewColumnId = columnId, NewPosition = 10 };

            // Act
            var result = await _controller.MoveTask(task1.Id, dto);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var t1 = await _context.Tasks.FindAsync(task1.Id);
            // It should have been clamped to the end
            Assert.Equal(1, t1.Position);
        }

        [Fact]
        public async Task MoveTask_ShouldNotThrowException_WhenNewPositionIsNegative()
        {
             // Arrange
            var projectId = Guid.NewGuid();
            var columnId = Guid.NewGuid();

            var task1 = new ProjectTask { Id = Guid.NewGuid(), ProjectId = projectId, ColumnId = columnId, Position = 0, Title = "Task 1" };
            var task2 = new ProjectTask { Id = Guid.NewGuid(), ProjectId = projectId, ColumnId = columnId, Position = 1, Title = "Task 2" };

            _context.Tasks.AddRange(task1, task2);
            await _context.SaveChangesAsync();

            var dto = new MoveTaskDto { NewColumnId = columnId, NewPosition = -1 };

            // Act
            var result = await _controller.MoveTask(task1.Id, dto);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var t1 = await _context.Tasks.FindAsync(task1.Id);
            // It should have been clamped to the beginning
            Assert.Equal(0, t1.Position);
        }
    }
}
