using Microsoft.AspNetCore.Mvc;
using TaskManagerAPI.Data;
using TaskManagerAPI.Models;
using System.Linq;

namespace TaskManagerAPI.Controllers
{
    [Route("api/tasks")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly TaskDbContext _context;

        public TasksController(TaskDbContext context)
        {
            _context = context;
        }

        // POST: api/tasks
        [HttpPost("")]
        public IActionResult GetAllTasksWithPaginationAndSearch([FromBody] PaginationSearchRequest request)
        {
            // Validate request parameters
            if (request.Page < 1 || request.PageSize < 1)
            {
                return BadRequest(new { error = "Invalid request parameters" });
            }

            // Perform search and pagination using database
            var filteredTasks = string.IsNullOrWhiteSpace(request.Search)
                ? _context.Tasks
                : _context.Tasks.Where(t =>
                    t.Title.Contains(request.Search) ||
                    t.Description.Contains(request.Search));

            var pagedTasks = filteredTasks
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return Ok(new
            {
                tasks = pagedTasks,
                total = filteredTasks.Count()
            });
        }

        // POST: api/tasks/details
        [HttpPost("details")]
        public IActionResult GetTaskById([FromBody] TaskIdRequest request)
        {
            // Find task by ID using database
            var task = _context.Tasks.Find(request.Id);
            if (task == null)
            {
                return NotFound(new { error = "Task not found" });
            }

            return Ok(task);
        }

        // POST: api/tasks/create
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

            _context.Tasks.Add(task);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
        }

        // PUT: api/tasks/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateTask(int id, [FromBody] TaskItem request)
        {
            // Find the task
            var task = _context.Tasks.Find(id);
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

            _context.SaveChanges();

            return Ok(task);
        }

        // DELETE: api/tasks/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteTask(int id)
        {
            var task = _context.Tasks.Find(id);
            if (task == null)
            {
                return NotFound(new { Message = "המשימה לא נמצאה" });
            }

            _context.Tasks.Remove(task);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
