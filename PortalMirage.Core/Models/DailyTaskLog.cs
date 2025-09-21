namespace PortalMirage.Core.Models;

public record DailyTaskLog
{
    public long LogID { get; init; }
    public int TaskID { get; set; }
    public DateTime LogDate { get; set; } // Changed from DateOnly to DateTime
    public required string Status { get; set; }
    public int? CompletedByUserID { get; set; }
    public DateTime? CompletedDateTime { get; set; }
}