using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UnityDevHub.API.Controllers;
using UnityDevHub.API.Data;
using UnityDevHub.API.Data.Entities;
using UnityDevHub.API.Models.Project;
using Xunit;

namespace UnityDevHub.API.Tests.Controllers
{
    public class ProjectsControllerTests
    {
        private readonly ApplicationDbContext _context;
        private readonly ProjectsController _controller;
        private readonly Guid _userId;

        public ProjectsControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new ProjectsController(_context);

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
        public async Task CreateProject_ShouldCreateProjectAndDefaultColumns()
        {
            // Arrange
            var dto = new CreateProjectDto
            {
                Name = "New Project",
                Description = "Description",
                ColorTheme = "#000000"
            };

            // Act
            var result = await _controller.CreateProject(dto);

            // Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedDto = Assert.IsType<ProjectDto>(actionResult.Value);

            Assert.Equal(dto.Name, returnedDto.Name);
            Assert.Equal(_userId, returnedDto.CreatedById);

            // Verify DB
            var project = await _context.Projects.FindAsync(returnedDto.Id);
            Assert.NotNull(project);

            // Verify columns
            var columns = await _context.TaskColumns.Where(tc => tc.ProjectId == project.Id).ToListAsync();
            Assert.Equal(3, columns.Count); // To Do, In Progress, Done

            // Verify membership
            var member = await _context.ProjectMembers.FirstOrDefaultAsync(pm => pm.ProjectId == project.Id && pm.UserId == _userId);
            Assert.NotNull(member);
            Assert.Equal(ProjectRole.Owner, member.Role);
        }

        [Fact]
        public async Task GetProjects_ShouldReturnOnlyUserProjects()
        {
            // Arrange
            // Create a project where user is member
            var project1 = new Project { Id = Guid.NewGuid(), Name = "My Project" };
            _context.Projects.Add(project1);
            _context.ProjectMembers.Add(new ProjectMember { ProjectId = project1.Id, UserId = _userId, Role = ProjectRole.Member });

            // Create a project where user is NOT member
            var project2 = new Project { Id = Guid.NewGuid(), Name = "Other Project" };
            _context.Projects.Add(project2);
            // No membership for project2

            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetProjects();

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var projects = Assert.IsAssignableFrom<IEnumerable<ProjectDto>>(actionResult.Value);

            Assert.Single(projects);
            Assert.Contains(projects, p => p.Id == project1.Id);
        }

        [Fact]
        public async Task UpdateProject_ShouldUpdate_WhenUserIsOwner()
        {
            // Arrange
            var project = new Project { Id = Guid.NewGuid(), Name = "Old Name" };
            _context.Projects.Add(project);
            _context.ProjectMembers.Add(new ProjectMember { ProjectId = project.Id, UserId = _userId, Role = ProjectRole.Owner });
            await _context.SaveChangesAsync();

            var dto = new UpdateProjectDto { Name = "New Name", Description = "Desc", ColorTheme = "Color" };

            // Act
            var result = await _controller.UpdateProject(project.Id, dto);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var updatedProject = await _context.Projects.FindAsync(project.Id);
            Assert.Equal("New Name", updatedProject!.Name);
        }

        [Fact]
        public async Task DeleteProject_ShouldReturnForbid_WhenUserIsNotOwner()
        {
            // Arrange
            var project = new Project { Id = Guid.NewGuid(), Name = "Project" };
            _context.Projects.Add(project);
            _context.ProjectMembers.Add(new ProjectMember { ProjectId = project.Id, UserId = _userId, Role = ProjectRole.Member }); // Not Owner
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteProject(project.Id);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }
    }
}
