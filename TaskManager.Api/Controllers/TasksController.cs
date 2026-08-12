using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.DTOs;
using TaskManager.Api.Models;

namespace TaskManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TasksController> _logger;

        public TasksController(ApplicationDbContext context, ILogger<TasksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private bool IsAdmin =>
            User.FindFirstValue(ClaimTypes.Role) == UserRole.Admin.ToString();

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskReadDto>>> GetTasks()
        {
            var query = _context.Tasks.AsQueryable();

            if (!IsAdmin)
            {
                query = query.Where(t => t.UserId == CurrentUserId);
            }

            var tasks = await query
                .Select(t => new TaskReadDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    Priority = t.Priority,
                    Category = t.Category,
                    DueDate = t.DueDate,
                    CreatedAt = t.CreatedAt,
                    UserId = t.UserId
                })
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskReadDto>> GetTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
                return NotFound();

            if (!IsAdmin && task.UserId != CurrentUserId)
                return Forbid();

            return Ok(new TaskReadDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                Category = task.Category,
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt,
                UserId = task.UserId
            });
        }

        [HttpPost]
        public async Task<ActionResult<TaskReadDto>> CreateTask(TaskCreateUpdateDto dto)
        {
            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                Status = dto.Status,
                Priority = dto.Priority,
                Category = dto.Category,
                DueDate = dto.DueDate,
                UserId = CurrentUserId
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Task {TaskId} created by user {UserId}", task.Id, CurrentUserId);

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskCreateUpdateDto dto)
        {
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
                return NotFound();

            if (!IsAdmin && task.UserId != CurrentUserId)
                return Forbid();

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.Status = dto.Status;
            task.Priority = dto.Priority;
            task.Category = dto.Category;
            task.DueDate = dto.DueDate;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Task {TaskId} updated by user {UserId}", task.Id, CurrentUserId);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
                return NotFound();

            if (!IsAdmin && task.UserId != CurrentUserId)
                return Forbid();

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Task {TaskId} deleted by user {UserId}", task.Id, CurrentUserId);

            return NoContent();
        }

        [HttpGet("summary")]
        public async Task<ActionResult> GetSummary()
        {
            var query = _context.Tasks.AsQueryable();

            if (!IsAdmin)
                query = query.Where(t => t.UserId == CurrentUserId);

            var summary = new
            {
                Pending = await query.CountAsync(t => t.Status == TaskManager.Api.Models.TaskStatus.Pending),
                InProgress = await query.CountAsync(t => t.Status == TaskManager.Api.Models.TaskStatus.InProgress),
                Completed = await query.CountAsync(t => t.Status == TaskManager.Api.Models.TaskStatus.Completed)
            };

            return Ok(summary);
        }
    }
}