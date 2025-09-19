namespace PortalMirage.Core.Models;

public record DailyTaskLog
{
    public long LogID { get; init; }
    public int TaskID { get; set; }
    public DateOnly LogDate { get; set; } // Using DateOnly for just the date part
    public required string Status { get; set; }
    public int? CompletedByUserID { get; set; }
    public DateTime? CompletedDateTime { get; set; }
}