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
    public string? ResolutionNotes { get; set; }
    public int? DowntimeMinutes { get; set; }

    // New properties for deactivation
    public bool IsActive { get; set; }
    public string? DeactivationReason { get; set; }
    public int? DeactivatedByUserID { get; set; }
    public DateTime? DeactivationDateTime { get; set; }
}