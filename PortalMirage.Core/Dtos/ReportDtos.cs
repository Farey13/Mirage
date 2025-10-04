using System;

namespace PortalMirage.Core.Dtos;

public record MachineBreakdownReportDto(
    DateTime ReportedDateTime,
    string MachineName,
    string BreakdownReason,
    string ReportedByUsername,
    bool IsResolved,
    DateTime? ResolvedDateTime,
    string? ResolvedByUsername,
    string? ResolutionNotes,
    int? DowntimeMinutes
);