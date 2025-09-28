using System;
namespace PortalMirage.Core.Models;
public record DailyTaskLog
{
    public long LogID { get; init; }
    public int TaskID { get; set; }
    public DateTime LogDate { get; set; }
    public required string Status { get; set; }
    public string? Comments { get; set; }
    public int? CompletedByUserID { get; set; }
    public DateTime? CompletedDateTime { get; set; }
    public DateTime? LockOverrideUntil { get; set; }
    public string? LockOverrideReason { get; set; }
    public int? LockOverrideByUserID { get; set; }
}