namespace PortalMirage.Core.Models;

public record RepeatSampleLog
{
    public int RepeatID { get; init; }
    public string? PatientIdCardNumber { get; set; }
    public required string PatientName { get; set; }
    public string? InformedPersonOrDept { get; set; }
    public string? ReasonText { get; set; }
    public DateTime LogDateTime { get; init; }
    public int LoggedByUserID { get; set; }
}