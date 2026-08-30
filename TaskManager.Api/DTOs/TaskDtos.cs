using TaskManager.Api.Models;

namespace TaskManager.Api.DTOs
{
   public class TaskCreateUpdateDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskManager.Api.Models.TaskStatus Status { get; set; } = TaskManager.Api.Models.TaskStatus.Pending;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public string? Category { get; set; }
    public DateTime? DueDate { get; set; }
    public int? UserId { get; set; }
}

    public class TaskReadDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
       public TaskManager.Api.Models.TaskStatus Status { get; set; }        public TaskPriority Priority { get; set; }
        public string? Category { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
        
    }
}