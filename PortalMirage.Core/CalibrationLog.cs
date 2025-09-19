namespace PortalMirage.Core.Models;

public record CalibrationLog
{
    public int CalibrationID { get; init; }
    public required string TestName { get; set; }
    public required string QcResult { get; set; }
    public string? Reason { get; set; }
    public DateTime CalibrationDateTime { get; init; }
    public int PerformedByUserID { get; set; }
}