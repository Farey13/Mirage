namespace PortalMirage.Core.Models;

// This DTO combines data from the Task and DailyTaskLog models
public record TaskLogDetailDto
{
    public long LogID { get; init; }
    public required string TaskName { get; init; }
    public required string TaskCategory { get; init; }
    public required string Status { get; init; }
    public DateTime? CompletedDateTime { get; init; }
    public int? CompletedByUserID { get; init; }
}