using System;

namespace PortalMirage.Core.Dtos;

public record ShiftResponse(
    int ShiftID,
    string ShiftName,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int GracePeriodHours,
    bool IsActive);

public record CreateShiftRequest(
    string ShiftName,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int GracePeriodHours);

public record UpdateShiftRequest(
    int ShiftID, // This was the missing property
    string ShiftName,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int GracePeriodHours,
    bool IsActive);