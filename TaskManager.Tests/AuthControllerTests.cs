using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskManager.Api.Controllers;
using TaskManager.Api.Data;
using TaskManager.Api.Models;
using Xunit;

namespace TaskManager.Tests
{
    public class AuthControllerTests
    {
        private ApplicationDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetUsers_ReturnsAllUsers()
        {
            var context = GetInMemoryContext();
            context.Users.Add(new User { FullName = "Alice", Email = "alice@example.com", PasswordHash = "x", Role = UserRole.User });
            context.Users.Add(new User { FullName = "Bob", Email = "bob@example.com", PasswordHash = "x", Role = UserRole.Admin });
            await context.SaveChangesAsync();

            var logger = new LoggerFactory().CreateLogger<AuthController>();
            var tokenService = new FakeTokenService();
            var controller = new AuthController(context, tokenService, logger);

            var result = await controller.GetUsers();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }
    }
}