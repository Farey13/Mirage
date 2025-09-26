using System;
namespace PortalMirage.Core.Dtos;

public record CreateMachineBreakdownRequest(string MachineName, string BreakdownReason);

public record ResolveBreakdownRequest(string ResolutionNotes);

public record MachineBreakdownResponse(
    int BreakdownID,
    string MachineName,
    string BreakdownReason,
    DateTime ReportedDateTime,
    int ReportedByUserID,
    string ReportedByUsername,
    bool IsResolved,
    DateTime? ResolvedDateTime,
    int? ResolvedByUserID,
    string? ResolvedByUsername,
    string? ResolutionNotes,      // Add this line
    int? DowntimeMinutes);        // Add this line