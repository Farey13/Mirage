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
public record HandoverReportDto(
    DateTime GivenDateTime,
    string GivenByUsername,
    string Shift,
    string Priority,
    string HandoverNotes,
    bool IsReceived,
    DateTime? ReceivedDateTime,
    string? ReceivedByUsername
);