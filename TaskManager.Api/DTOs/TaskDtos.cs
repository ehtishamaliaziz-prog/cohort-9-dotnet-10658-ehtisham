using System.ComponentModel.DataAnnotations;
using TaskManager.Api.Models;

namespace TaskManager.Api.DTOs
{
    public class TaskCreateUpdateDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        [EnumDataType(typeof(TaskManager.Api.Models.TaskStatus))]
        public TaskManager.Api.Models.TaskStatus Status { get; set; } = TaskManager.Api.Models.TaskStatus.Pending;

        [EnumDataType(typeof(TaskPriority))]
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        [MaxLength(100)]
        public string? Category { get; set; }

        public DateTime? DueDate { get; set; }

        // Admin-only: allows reassigning a task to a different user.
        // Regular users cannot set this — the controller enforces that.
        public int? UserId { get; set; }
    }

    public class TaskReadDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TaskManager.Api.Models.TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public string? Category { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
    }
}