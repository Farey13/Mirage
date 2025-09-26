namespace PortalMirage.Core.Models;

public record MachineBreakdown
{
    public int BreakdownID { get; init; }
    public required string MachineName { get; set; }
    public required string BreakdownReason { get; set; }
    public DateTime ReportedDateTime { get; init; }
    public int ReportedByUserID { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedDateTime { get; set; }
    public int? ResolvedByUserID { get; set; }

    public string? ResolutionNotes { get; set; } // Add this line
    public int? DowntimeMinutes { get; set; }   // Add this line
}