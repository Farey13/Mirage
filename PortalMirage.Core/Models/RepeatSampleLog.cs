using System;

namespace PortalMirage.Core.Models;

public record RepeatSampleLog
{
    public int RepeatID { get; init; }
    public string? PatientIdCardNumber { get; set; }
    public required string PatientName { get; set; }
    public string? ReasonText { get; set; }
    public string? InformedPerson { get; set; }
    public string? Department { get; set; }
    public DateTime LogDateTime { get; init; }
    public int LoggedByUserID { get; set; }
    public bool IsActive { get; set; }
    public string? DeactivationReason { get; set; }
    public int? DeactivatedByUserID { get; set; }
    public DateTime? DeactivationDateTime { get; set; }
}