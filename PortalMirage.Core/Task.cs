namespace PortalMirage.Core.Models;

public record Task
{
    public int TaskID { get; init; }
    public required string TaskName { get; set; }
    public required string TaskCategory { get; set; }
    public required string ScheduleType { get; set; }
    public string? ScheduleValue { get; set; }
    public bool IsActive { get; set; }
}