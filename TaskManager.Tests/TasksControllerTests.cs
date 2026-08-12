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
    public class TasksControllerTests
    {
        private ApplicationDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task CreateTask_AddsTaskToDatabase()
        {
            var context = GetInMemoryContext();
            var logger = new LoggerFactory().CreateLogger<TasksController>();
            var controller = new TasksController(context, logger);
            controller.ControllerContext = TestHelpers.CreateControllerContextForUser(userId: 1, role: "User");

            var dto = new TaskCreateUpdateDto
            {
                Title = "Write project report",
                Priority = TaskPriority.High,
                Status = TaskManager.Api.Models.TaskStatus.Pending
            };

            var result = await controller.CreateTask(dto);

            Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(1, await context.Tasks.CountAsync());
        }

        [Fact]
        public async Task GetTasks_ReturnsOnlyCurrentUsersTasks_WhenNotAdmin()
        {
            var context = GetInMemoryContext();
            context.Tasks.Add(new TaskItem { Title = "User 1 task", UserId = 1 });
            context.Tasks.Add(new TaskItem { Title = "User 2 task", UserId = 2 });
            await context.SaveChangesAsync();

            var logger = new LoggerFactory().CreateLogger<TasksController>();
            var controller = new TasksController(context, logger);
            controller.ControllerContext = TestHelpers.CreateControllerContextForUser(userId: 1, role: "User");

            var result = await controller.GetTasks();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var tasks = Assert.IsAssignableFrom<IEnumerable<TaskReadDto>>(okResult.Value);
            Assert.Single(tasks);
        }

        [Fact]
        public async Task DeleteTask_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            var context = GetInMemoryContext();
            var logger = new LoggerFactory().CreateLogger<TasksController>();
            var controller = new TasksController(context, logger);
            controller.ControllerContext = TestHelpers.CreateControllerContextForUser(userId: 1, role: "User");

            var result = await controller.DeleteTask(999);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}