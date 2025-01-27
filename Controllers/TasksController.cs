using Microsoft.AspNetCore.Mvc;
using TaskManagerAPI.Models;
using System.Collections.Generic;
using System.Linq;

namespace TaskManagerAPI.Controllers
{
    [Route("api/tasks")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private static List<TaskItem> tasks = new List<TaskItem>
        {
            new TaskItem { Id = 1, Title = "משימה ראשונה", Description = "תיאור משימה", Status = false },
            new TaskItem { Id = 2, Title = "משימה שנייה", Description = "תיאור נוסף", Status = true },
            new TaskItem { Id = 2, Title = "", Description = "", Status = true }

        };

       // POST: api/tasks
        [HttpPost("")]
        public IActionResult GetAllTasksWithPaginationAndSearch([FromBody] PaginationSearchRequest request)
        {
            // Validate request parameters
            if (request.Page < 1 || request.PageSize < 1)
            {
                return BadRequest(new { error = "Invalid request parameters" });
            }
 
            // Perform search and pagination
            var filteredTasks = string.IsNullOrWhiteSpace(request.SearchQuery)
                ? tasks
                : tasks.Where(t =>
                    t.Title.Contains(request.SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                    t.Description.Contains(request.SearchQuery, StringComparison.OrdinalIgnoreCase)).ToList();
 
            var pagedTasks = filteredTasks
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();
 
            return Ok(new
            {
                tasks = pagedTasks,
                total = filteredTasks.Count
            });
        }

        // POST: api/tasks/details
        [HttpPost("details")]
        public IActionResult GetTaskById([FromBody] TaskIdRequest request)
        {
            // Find task by ID
            var task = tasks.FirstOrDefault(t => t.Id == request.Id);
            if (task == null)
            {
                return NotFound(new { error = "Task not found" });
            }
 
            return Ok(task);
        }

        // POST: api/tasks
        [HttpPost("create")]
        public IActionResult CreateTask([FromBody] TaskItem task)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (string.IsNullOrEmpty(task.Title) || task.Title.Length < 3 || task.Title.Length > 50)
            {
                return BadRequest(new { Message = "כותרת המשימה חייבת להיות בין 3 ל-50 תווים." });
            }

            task.Id = tasks.Count > 0 ? tasks.Max(t => t.Id) + 1 : 1;
            tasks.Add(task);
            return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
        }

      
        // PUT: api/tasks/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateTask(int id, [FromBody] TaskItem request)
        {
            // Find the task
            var task = tasks.FirstOrDefault(t => t.Id == id);
            if (task == null)
            {
                return NotFound(new { error = "Task not found" });
            }
 
            // Update task fields
            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                task.Title = request.Title;
            }
            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                task.Description = request.Description;
            }
            task.Status = request.Status;
 
            return Ok(task);
        }
 

        // DELETE: api/tasks/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteTask(int id)
        {
            var task = tasks.FirstOrDefault(t => t.Id == id);
            if (task == null)
            {
                return NotFound(new { Message = "המשימה לא נמצאה" });
            }

            tasks.Remove(task);
            return NoContent();
        }

    }
}
