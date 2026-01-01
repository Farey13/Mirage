using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;

namespace PortalMirage.Core.Dtos;

public partial class MachineBreakdownReportDto : ObservableObject
{
    public DateTime ReportedDateTime { get; set; }
    public string MachineName { get; set; }
    public string BreakdownReason { get; set; }
    public string ReportedByUserName { get; set; } // Fixed casing
    public bool IsResolved { get; set; }
    public DateTime? ResolvedDateTime { get; set; }
    public string? ResolvedByUserName { get; set; } // Fixed casing
    public string? ResolutionNotes { get; set; }
    public int? DowntimeMinutes { get; set; }

    public string ElapsedDowntimeDisplay
    {
        get
        {
            if (IsResolved)
            {
                if (!DowntimeMinutes.HasValue) return "-";

                var ts = TimeSpan.FromMinutes(DowntimeMinutes.Value);
                if (ts.TotalDays >= 1) return $"{ts.Days}d {ts.Hours}h";
                return $"{ts.Hours}h {ts.Minutes}m";
            }
            else
            {
                var elapsed = DateTime.Now - ReportedDateTime;
                if (elapsed.Days > 0) return $"{elapsed.Days}d {elapsed.Hours}h";
                if (elapsed.Hours > 0) return $"{elapsed.Hours}h {elapsed.Minutes}m";
                return $"{elapsed.Minutes}m";
            }
        }
    }

    public void UpdateCalculatedProperties()
    {
        OnPropertyChanged(nameof(ElapsedDowntimeDisplay));
    }
}

public partial class HandoverReportDto : ObservableObject
{
    public DateTime GivenDateTime { get; set; }
    public string GivenByUserName { get; set; } // Fixed casing
    public string Shift { get; set; }
    public string Priority { get; set; }
    public string HandoverNotes { get; set; }
    public bool IsReceived { get; set; }
    public DateTime? ReceivedDateTime { get; set; }
    public string? ReceivedByUserName { get; set; } // Fixed casing
}

public partial class KitValidationReportDto : ObservableObject
{
    public DateTime ValidationDateTime { get; set; }
    public string KitName { get; set; }
    public string KitLotNumber { get; set; }
    public DateTime KitExpiryDate { get; set; }
    public string ValidationStatus { get; set; }
    public string? Comments { get; set; }
    public string ValidatedByUserName { get; set; } // Fixed casing
}

public partial class RepeatSampleReportDto : ObservableObject
{
    public DateTime LogDateTime { get; set; }
    public string? PatientIdCardNumber { get; set; }
    public string PatientName { get; set; }
    public string? ReasonText { get; set; }
    public string? Department { get; set; }
    public string? InformedPerson { get; set; }
    public string LoggedByUserName { get; set; } // Fixed casing
}

public partial class MediaSterilityReportDto : ObservableObject
{
    public DateTime CheckDateTime { get; set; }
    public string MediaName { get; set; }
    public string MediaLotNumber { get; set; }
    public string? MediaQuantity { get; set; }
    public string Result37C { get; set; }
    public string Result25C { get; set; }
    public string OverallStatus { get; set; }
    public string? Comments { get; set; }
    public string PerformedByUserName { get; set; } // Fixed casing
}

public partial class SampleStorageReportDto : ObservableObject
{
    public DateTime StorageDateTime { get; set; }
    public string PatientSampleID { get; set; }
    public string TestName { get; set; }
    public string StoredByUserName { get; set; } // Fixed casing
    public bool IsTestDone { get; set; }
    public DateTime? TestDoneDateTime { get; set; }
    public string? TestDoneByUserName { get; set; } // Fixed casing
}

public partial class CalibrationReportDto : ObservableObject
{
    public DateTime CalibrationDateTime { get; set; }
    public string TestName { get; set; }
    public string QcResult { get; set; }
    public string? Reason { get; set; }
    public string PerformedByUserName { get; set; } // Fixed casing
}

public partial class DailyTaskLogDto : ObservableObject
{
    // Matches SQL: dtl.LogDate
    public DateTime LogDate { get; set; }

    // Matches SQL: t.TaskName
    public string TaskName { get; set; }

    // Matches SQL: s.ShiftName
    public string? ShiftName { get; set; }

    // Matches SQL: dtl.Status
    public string Status { get; set; }

    // Matches SQL: dtl.Comments
    public string? Comments { get; set; }

    // Matches SQL: u.FullName AS CompletedByUserName
    public string? CompletedByUserName { get; set; }

    // Matches SQL: dtl.CompletedDateTime
    public DateTime? CompletedDateTime { get; set; }

    // --- Helpers for ReportService.cs ---
    public DateTime TaskDate => LogDate;

    // Compatibility property for reports/UI expecting "Shift"
    public string? Shift => ShiftName;
}
// Wrapper for the Compliance Report
public record DailyTaskComplianceReportDto(
    List<DailyTaskLogDto> Items,
    int TotalTasks,
    int CompletedTasks
);