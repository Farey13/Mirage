using System;
namespace PortalMirage.Core.Dtos;

public record CreateMachineBreakdownRequest(string MachineName, string BreakdownReason);
public record MachineBreakdownResponse(int BreakdownID, string MachineName, string BreakdownReason, DateTime ReportedDateTime, int ReportedByUserID, bool IsResolved, DateTime? ResolvedDateTime, int? ResolvedByUserID);