namespace PortalMirage.Api.Dtos;

public record MachineBreakdownResponse(
    int BreakdownID,
    string MachineName,
    string BreakdownReason,
    DateTime ReportedDateTime,
    int ReportedByUserID,
    bool IsResolved,
    DateTime? ResolvedDateTime,
    int? ResolvedByUserID);