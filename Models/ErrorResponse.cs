namespace TaskManagerAPI.Models
{
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string? ErrorID { get; set; }
        public string? ErrorDescription { get; set; }
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; }
    }
}