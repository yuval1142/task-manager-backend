
namespace TaskManagerAPI.Models
{

    public class TaskUpdateRequest
    {
        public int Id { get; set; } // מזהה המשימה
        public string? Title { get; set; } // כותרת המשימה
        public string? Description { get; set; } // תיאור המשימה
        public bool? Status { get; set; } // סטטוס המשימה
    }
}