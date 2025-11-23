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
using UnityDevHub.API.Models.Wiki;
using Xunit;

namespace UnityDevHub.API.Tests.Controllers
{
    public class WikiControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly WikiController _controller;
        private readonly User _testUser;
        private readonly Project _testProject;

        public WikiControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            // Seed Data
            _testUser = new User { Id = Guid.NewGuid(), UserName = "testuser", Email = "test@example.com", FullName = "Test User" };
            _testProject = new Project { Id = Guid.NewGuid(), Name = "Test Project", CreatedById = _testUser.Id };

            _context.Users.Add(_testUser);
            _context.Projects.Add(_testProject);
            _context.SaveChanges();

            _controller = new WikiController(_context);

            // Mock User
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, _testUser.Id.ToString()),
                new Claim(ClaimTypes.Name, _testUser.UserName)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task CreatePage_ShouldCreatePage()
        {
            var dto = new CreateWikiPageDto
            {
                Title = "Home Page",
                Content = "Welcome to the wiki"
            };

            var result = await _controller.CreatePage(_testProject.Id, dto);

            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var pageDto = Assert.IsType<WikiPageDto>(actionResult.Value);

            Assert.Equal("Home Page", pageDto.Title);
            Assert.Equal("Welcome to the wiki", pageDto.Content);
            Assert.Equal(_testProject.Id, pageDto.ProjectId);
        }

        [Fact]
        public async Task GetProjectWiki_ShouldReturnTreeStructure()
        {
            // Create root page
            var rootPage = new WikiPage { Id = Guid.NewGuid(), ProjectId = _testProject.Id, Title = "Root", Content = "", LastEditorId = _testUser.Id };
            _context.WikiPages.Add(rootPage);
            await _context.SaveChangesAsync();

            // Create child page
            var childPage = new WikiPage { Id = Guid.NewGuid(), ProjectId = _testProject.Id, Title = "Child", Content = "", ParentId = rootPage.Id, LastEditorId = _testUser.Id };
            _context.WikiPages.Add(childPage);
            await _context.SaveChangesAsync();

            var result = await _controller.GetProjectWiki(_testProject.Id);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var pages = Assert.IsType<List<WikiPageDto>>(okResult.Value);

            Assert.Single(pages); // Only one root page
            Assert.Equal("Root", pages[0].Title);
            Assert.Single(pages[0].Children); // Child page nested
            Assert.Equal("Child", pages[0].Children[0].Title);
        }
    }
}
