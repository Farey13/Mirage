namespace PortalMirage.Api.Dtos;

public record CreateCalibrationLogRequest(string TestName, string QcResult, string? Reason);