using TaskManagerAPI.Models;


namespace TaskManagerAPI.Data
{
    public static class TaskSeeder
    {
        public static List<TaskItem> GetMockTasks()
        {
            var tasks = new List<TaskItem>();
            for (int i = 1; i <= 150; i++)
            {
                tasks.Add(new TaskItem
                {
                    Id = i,
                    Title = $"משימה מספר {i}",
                    Description = $"תיאור משימה מדומה עבור משימה מספר {i}",
                    Status = i % 2 == 0 // מחזור בין true/false
                });
            }
            return tasks;
        }
    }
}
