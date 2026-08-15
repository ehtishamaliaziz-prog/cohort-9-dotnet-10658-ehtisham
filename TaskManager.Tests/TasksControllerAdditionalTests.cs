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
    public class TasksControllerAdditionalTests
    {
        private ApplicationDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private TasksController GetController(ApplicationDbContext context, int userId, string role)
        {
            var logger = new LoggerFactory().CreateLogger<TasksController>();
            var controller = new TasksController(context, logger);
            controller.ControllerContext = TestHelpers.CreateControllerContextForUser(userId, role);
            return controller;
        }

        // --- GetTask ---

        [Fact]
        public async Task GetTask_ReturnsTask_WhenOwnedByRequester()
        {
            var context = GetInMemoryContext();
            var task = new TaskItem { Title = "My task", UserId = 1 };
            context.Tasks.Add(task);
            await context.SaveChangesAsync();

            var controller = GetController(context, userId: 1, role: "User");

            var result = await controller.GetTask(task.Id);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<TaskReadDto>(okResult.Value);
            Assert.Equal("My task", dto.Title);
        }

        [Fact]
        public async Task GetTask_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            var context = GetInMemoryContext();
            var controller = GetController(context, userId: 1, role: "User");

            var result = await controller.GetTask(999);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetTask_ReturnsForbid_WhenNotOwnerAndNotAdmin()
        {
            var context = GetInMemoryContext();
            var task = new TaskItem { Title = "Someone else's task", UserId = 2 };
            context.Tasks.Add(task);
            await context.SaveChangesAsync();

            var controller = GetController(context, userId: 1, role: "User");

            var result = await controller.GetTask(task.Id);

            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task GetTask_ReturnsTask_WhenNotOwnerButAdmin()
        {
            var context = GetInMemoryContext();
            var task = new TaskItem { Title = "Someone else's task", UserId = 2 };
            context.Tasks.Add(task);
            await context.SaveChangesAsync();

            var controller = GetController(context, userId: 1, role: "Admin");

            var result = await controller.GetTask(task.Id);

            Assert.IsType<OkObjectResult>(result.Result);
        }

        // --- CreateTask ---

        [Fact]
        public async Task CreateTask_AssignsTaskToCurrentUser()
        {
            var context = GetInMemoryContext();
            var controller = GetController(context, userId: 42, role: "User");

            var dto = new TaskCreateUpdateDto { Title = "New task" };

            await controller.CreateTask(dto);

            var savedTask = await context.Tasks.FirstAsync();
            Assert.Equal(42, savedTask.UserId);
        }

        // --- UpdateTask ---

        [Fact]
        public async Task UpdateTask_UpdatesFields_WhenOwner()
        {
            var context = GetInMemoryContext();
            var task = new TaskItem { Title = "Old title", UserId = 1, Status = TaskManager.Api.Models.TaskStatus.Pending };
            context.Tasks.Add(task);
            await context.SaveChangesAsync();

            var controller = GetController(context, userId: 1, role: "User");

            var dto = new TaskCreateUpdateDto
            {
                Title = "New title",
                Status = TaskManager.Api.Models.TaskStatus.Completed,
                Priority = TaskPriority.High
            };

            var result = await controller.UpdateTask(task.Id, dto);

            Assert.IsType<NoContentResult>(result);
            var updated = await context.Tasks.FindAsync(task.Id);
            Assert.Equal("New title", updated!.Title);
            Assert.Equal(TaskManager.Api.Models.TaskStatus.Completed, updated.Status);
        }

        [Fact]
        public async Task UpdateTask_ReturnsNotFound_WhenTaskDoesNotExist()
        {
            var context = GetInMemoryContext();
            var controller = GetController(context, userId: 1, role: "User");

            var dto = new TaskCreateUpdateDto { Title = "Doesn't matter" };

            var result = await controller.UpdateTask(999, dto);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateTask_ReturnsForbid_WhenNotOwnerAndNotAdmin()
        {
            var context = GetInMemoryContext();
            var task = new TaskItem { Title = "Someone else's task", UserId = 2 };
            context.Tasks.Add(task);
            await context.SaveChangesAsync();

            var controller = GetController(context, userId: 1, role: "User");

            var dto = new TaskCreateUpdateDto { Title = "Hijacked title" };

            var result = await controller.UpdateTask(task.Id, dto);

            Assert.IsType<ForbidResult>(result);
        }

        // --- DeleteTask ---

        [Fact]
        public async Task DeleteTask_RemovesTask_WhenOwner()
        {
            var context = GetInMemoryContext();
            var task = new TaskItem { Title = "To be deleted", UserId = 1 };
            context.Tasks.Add(task);
            await context.SaveChangesAsync();

            var controller = GetController(context, userId: 1, role: "User");

            var result = await controller.DeleteTask(task.Id);

            Assert.IsType<NoContentResult>(result);
            Assert.Equal(0, await context.Tasks.CountAsync());
        }

        [Fact]
        public async Task DeleteTask_ReturnsForbid_WhenNotOwnerAndNotAdmin()
        {
            var context = GetInMemoryContext();
            var task = new TaskItem { Title = "Someone else's task", UserId = 2 };
            context.Tasks.Add(task);
            await context.SaveChangesAsync();

            var controller = GetController(context, userId: 1, role: "User");

            var result = await controller.DeleteTask(task.Id);

            Assert.IsType<ForbidResult>(result);
            Assert.Equal(1, await context.Tasks.CountAsync());
        }

        // --- GetSummary ---

        [Fact]
        public async Task GetSummary_CountsOnlyCurrentUsersTasks_WhenNotAdmin()
        {
            var context = GetInMemoryContext();
            context.Tasks.Add(new TaskItem { Title = "Mine - pending", UserId = 1, Status = TaskManager.Api.Models.TaskStatus.Pending });
            context.Tasks.Add(new TaskItem { Title = "Mine - completed", UserId = 1, Status = TaskManager.Api.Models.TaskStatus.Completed });
            context.Tasks.Add(new TaskItem { Title = "Someone else's - pending", UserId = 2, Status = TaskManager.Api.Models.TaskStatus.Pending });
            await context.SaveChangesAsync();

            var controller = GetController(context, userId: 1, role: "User");

            var result = await controller.GetSummary();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value!;
            var pendingProp = value.GetType().GetProperty("Pending")!;
            var pendingCount = (int)pendingProp.GetValue(value)!;

            Assert.Equal(1, pendingCount);
        }

        [Fact]
        public async Task GetSummary_CountsAllUsersTasks_WhenAdmin()
        {
            var context = GetInMemoryContext();
            context.Tasks.Add(new TaskItem { Title = "User 1 - pending", UserId = 1, Status = TaskManager.Api.Models.TaskStatus.Pending });
            context.Tasks.Add(new TaskItem { Title = "User 2 - pending", UserId = 2, Status = TaskManager.Api.Models.TaskStatus.Pending });
            await context.SaveChangesAsync();

            var controller = GetController(context, userId: 1, role: "Admin");

            var result = await controller.GetSummary();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value!;
            var pendingProp = value.GetType().GetProperty("Pending")!;
            var pendingCount = (int)pendingProp.GetValue(value)!;

            Assert.Equal(2, pendingCount);
        }
    }
}