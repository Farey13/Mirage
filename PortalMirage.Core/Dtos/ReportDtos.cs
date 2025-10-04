using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace PortalMirage.Core.Dtos;

public partial class MachineBreakdownReportDto : ObservableObject
{
    public DateTime ReportedDateTime { get; set; }
    public string MachineName { get; set; }
    public string BreakdownReason { get; set; }
    public string ReportedByUsername { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedDateTime { get; set; }
    public string? ResolvedByUsername { get; set; }
    public string? ResolutionNotes { get; set; }
    public int? DowntimeMinutes { get; set; }

    public string ElapsedDowntimeDisplay
    {
        get
        {
            if (IsResolved)
            {
                return $"{DowntimeMinutes} mins";
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

    // This method is now correctly placed INSIDE the class
    public void UpdateCalculatedProperties()
    {
        OnPropertyChanged(nameof(ElapsedDowntimeDisplay));
    }
}

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
public record KitValidationReportDto(
    DateTime ValidationDateTime,
    string KitName,
    string KitLotNumber,
    DateTime KitExpiryDate,
    string ValidationStatus,
    string? Comments,
    string ValidatedByUsername
);