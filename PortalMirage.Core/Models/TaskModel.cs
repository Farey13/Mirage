namespace PortalMirage.Core.Models;

// Renamed to TaskModel to avoid conflict with System.Threading.Tasks.Task
public record TaskModel
{
    public int TaskID { get; init; }
    public required string TaskName { get; set; }
    public int? ShiftID { get; set; }
    public required string ScheduleType { get; set; }
    public string? ScheduleValue { get; set; }
    public bool IsActive { get; set; }
}