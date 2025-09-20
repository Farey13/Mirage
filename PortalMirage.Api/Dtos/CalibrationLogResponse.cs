namespace PortalMirage.Api.Dtos;

public record CalibrationLogResponse(
    int CalibrationID,
    string TestName,
    string QcResult,
    string? Reason,
    DateTime CalibrationDateTime,
    int PerformedByUserID);