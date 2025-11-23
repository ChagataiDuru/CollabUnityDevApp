using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using UnityDevHub.API.Data;
using UnityDevHub.API.Data.Entities;
using UnityDevHub.API.Models.Auth;
using UnityDevHub.API.Services;
using Xunit;

namespace UnityDevHub.API.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            _mockConfig = new Mock<IConfiguration>();
            _mockConfig.Setup(c => c["Jwt:Key"]).Returns("super_secret_key_that_is_long_enough_for_256_bit_encryption");
            _mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
            _mockConfig.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");

            _authService = new AuthService(_context, _mockConfig.Object);
        }

        [Fact]
        public async Task RegisterAsync_ShouldCreateUser_WhenUsernameIsUnique()
        {
            // Arrange
            var dto = new RegisterDto { Username = "newuser", Password = "password123", DisplayName = "New User" };

            // Act
            var result = await _authService.RegisterAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.AccessToken);
            Assert.NotNull(result.RefreshToken);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == "newuser");
            Assert.NotNull(user);
            Assert.Equal("New User", user.DisplayName);
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowException_WhenUsernameIsTaken()
        {
            // Arrange
            var existingUser = new User { Id = Guid.NewGuid(), Username = "existinguser", PasswordHash = "hash" };
            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();

            var dto = new RegisterDto { Username = "existinguser", Password = "password123" };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _authService.RegisterAsync(dto));
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnTokens_WhenCredentialsAreValid()
        {
            // Arrange
            var password = "password123";
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User { Id = Guid.NewGuid(), Username = "user", PasswordHash = passwordHash };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var dto = new LoginDto { Username = "user", Password = password };

            // Act
            var result = await _authService.LoginAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.AccessToken);
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowException_WhenUserDoesNotExist()
        {
            // Arrange
            var dto = new LoginDto { Username = "nonexistent", Password = "password" };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _authService.LoginAsync(dto));
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowException_WhenPasswordIsInvalid()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Username = "user", PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct") };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var dto = new LoginDto { Username = "user", Password = "wrong" };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _authService.LoginAsync(dto));
        }
    }
}
