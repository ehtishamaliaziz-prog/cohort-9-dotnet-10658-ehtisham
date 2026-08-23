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
    public class TaskReassignmentTests
    {
        private ApplicationDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task UpdateTask_ReassignsTask_WhenRequesterIsAdmin()
        {
            var context = GetInMemoryContext();
            var task = new TaskItem { Title = "Original task", UserId = 1 };
            context.Tasks.Add(task);
            await context.SaveChangesAsync();

            var logger = new LoggerFactory().CreateLogger<TasksController>();
            var controller = new TasksController(context, logger);
            controller.ControllerContext = TestHelpers.CreateControllerContextForUser(userId: 99, role: "Admin");

            var dto = new TaskCreateUpdateDto
            {
                Title = "Original task",
                UserId = 2
            };

            var result = await controller.UpdateTask(task.Id, dto);

            Assert.IsType<NoContentResult>(result);
            var updatedTask = await context.Tasks.FindAsync(task.Id);
            Assert.Equal(2, updatedTask!.UserId);
        }

        [Fact]
        public async Task UpdateTask_DoesNotReassignTask_WhenRequesterIsNotAdmin()
        {
            var context = GetInMemoryContext();
            var task = new TaskItem { Title = "Original task", UserId = 1 };
            context.Tasks.Add(task);
            await context.SaveChangesAsync();

            var logger = new LoggerFactory().CreateLogger<TasksController>();
            var controller = new TasksController(context, logger);
            controller.ControllerContext = TestHelpers.CreateControllerContextForUser(userId: 1, role: "User");

            var dto = new TaskCreateUpdateDto
            {
                Title = "Original task",
                UserId = 2
            };

            var result = await controller.UpdateTask(task.Id, dto);

            Assert.IsType<NoContentResult>(result);
            var updatedTask = await context.Tasks.FindAsync(task.Id);
            Assert.Equal(1, updatedTask!.UserId);
        }
    }
}