using System;

namespace PortalMirage.Core.Models;

public record Shift
{
    public int ShiftID { get; init; }
    public required string ShiftName { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int GracePeriodHours { get; set; }
    public bool IsActive { get; set; }
}