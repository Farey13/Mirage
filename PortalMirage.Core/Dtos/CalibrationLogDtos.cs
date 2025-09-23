using System;
namespace PortalMirage.Core.Dtos;

public record CreateCalibrationLogRequest(string TestName, string QcResult, string? Reason);
public record CalibrationLogResponse(int CalibrationID, string TestName, string QcResult, string? Reason, DateTime CalibrationDateTime, int PerformedByUserID, string PerformedByUsername);