using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManager.Api.Controllers;
using TaskManager.Api.Data;
using TaskManager.Api.DTOs;
using TaskManager.Api.Models;
using Xunit;

namespace TaskManager.Tests
{
    public class AuthControllerAdditionalTests
    {
        private ApplicationDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private AuthController GetController(ApplicationDbContext context)
        {
            var logger = new LoggerFactory().CreateLogger<AuthController>();
            var tokenService = new FakeTokenService();
            return new AuthController(context, tokenService, logger);
        }

        // --- Register ---

        [Fact]
        public async Task Register_CreatesUser_WhenEmailIsNew()
        {
            var context = GetInMemoryContext();
            var controller = GetController(context);

            var dto = new RegisterDto
            {
                FullName = "New Person",
                Email = "new@example.com",
                Password = "password123"
            };

            var result = await controller.Register(dto);

            Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(1, await context.Users.CountAsync());
        }

        [Fact]
        public async Task Register_HashesPassword_NotStoredAsPlainText()
        {
            var context = GetInMemoryContext();
            var controller = GetController(context);

            var dto = new RegisterDto
            {
                FullName = "New Person",
                Email = "new2@example.com",
                Password = "password123"
            };

            await controller.Register(dto);

            var savedUser = await context.Users.FirstAsync();
            Assert.NotEqual("password123", savedUser.PasswordHash);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenEmailAlreadyExists()
        {
            var context = GetInMemoryContext();
            context.Users.Add(new User
            {
                FullName = "Existing",
                Email = "taken@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("whatever"),
                Role = UserRole.User
            });
            await context.SaveChangesAsync();

            var controller = GetController(context);

            var dto = new RegisterDto
            {
                FullName = "Another Person",
                Email = "taken@example.com",
                Password = "password123"
            };

            var result = await controller.Register(dto);

            Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(1, await context.Users.CountAsync());
        }

        // --- Login ---

        [Fact]
        public async Task Login_Succeeds_WithCorrectCredentials()
        {
            var context = GetInMemoryContext();
            context.Users.Add(new User
            {
                FullName = "Correct User",
                Email = "correct@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("rightpassword"),
                Role = UserRole.User
            });
            await context.SaveChangesAsync();

            var controller = GetController(context);

            var dto = new LoginDto { Email = "correct@example.com", Password = "rightpassword" };

            var result = await controller.Login(dto);

            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WithWrongPassword()
        {
            var context = GetInMemoryContext();
            context.Users.Add(new User
            {
                FullName = "Correct User",
                Email = "correct2@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("rightpassword"),
                Role = UserRole.User
            });
            await context.SaveChangesAsync();

            var controller = GetController(context);

            var dto = new LoginDto { Email = "correct2@example.com", Password = "wrongpassword" };

            var result = await controller.Login(dto);

            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenEmailDoesNotExist()
        {
            var context = GetInMemoryContext();
            var controller = GetController(context);

            var dto = new LoginDto { Email = "nobody@example.com", Password = "whatever" };

            var result = await controller.Login(dto);

            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }
    }
}