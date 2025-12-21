namespace PortalMirage.Core.Dtos
{
    public class UpdateTaskStatusRequest
    {
        // 1. Add UserId so we can pass it to the strict backend
        public int UserId { get; set; }

        public string Status { get; set; } = string.Empty;
        public string? Comment { get; set; }

        // 2. Add an Empty Constructor (Required for 'new Request { ... }' syntax)
        public UpdateTaskStatusRequest() { }

        // 3. Keep the old constructor to prevent breaking other code (optional but safe)
        public UpdateTaskStatusRequest(string status, string? comment)
        {
            Status = status;
            Comment = comment;
        }
    }
}