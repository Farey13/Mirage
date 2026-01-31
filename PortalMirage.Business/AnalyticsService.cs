using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Dtos;
using PortalMirage.Data.Abstractions;

namespace PortalMirage.Business;

public class AnalyticsService(
    IDailyTaskLogRepository taskRepo,
    IMachineBreakdownRepository breakdownRepo,
    IHandoverRepository handoverRepo,          // FIXED: Using IHandoverRepository from new snippet
    ISampleStorageRepository storageRepo,
    ICalibrationLogRepository calibrationRepo,
    IKitValidationRepository kitRepo,
    IMediaSterilityCheckRepository mediaRepo,
    IRepeatSampleLogRepository repeatRepo) : IAnalyticsService
{
    // 1. DAILY TASK LOG (Combined best of both)
    public async Task<AnalyticsReportDto> GetDailyTaskCompletionAsync(DateTime start, DateTime end)
    {
        var logs = await taskRepo.GetComplianceReportDataAsync(start, end, null, "All");

        int total = logs.Count();
        int completed = logs.Count(l => l.Status == "Complete" || l.Status == "Completed");
        double percent = total > 0 ? (double)completed / total * 100 : 0;

        var kpis = new List<AnalyticsSummaryDto>
        {
            new("Total Tasks", total.ToString(), "Blue"),
            new("Completion %", $"{percent:F1}%", percent > 90 ? "Green" : "Orange"),
            new("Missed", (total - completed).ToString(), "Red")
        };

        var chartData = logs
            .GroupBy(l => l.LogDate.ToString("dd MMM"))
            .Select(g => new ChartDataPoint(
                g.Key,
                Math.Round(((double)g.Count(x => x.Status == "Complete" || x.Status == "Completed") / g.Count()) * 100, 0)
            ))
            .ToList();

        return new AnalyticsReportDto(kpis, chartData, "Daily Task Completion %");
    }

    // 2. MACHINE BREAKDOWN (Preserved improved calculations from current code)
    public async Task<AnalyticsReportDto> GetMachineDowntimeAsync(DateTime start, DateTime end)
    {
        var data = await breakdownRepo.GetReportDataAsync(start, end, null, "All");

        // Handle nullable DowntimeMinutes in KPI calculations
        var totalDowntimeMinutes = data.Sum(d => d.DowntimeMinutes ?? 0);
        var totalDowntimeHours = Math.Round(totalDowntimeMinutes / 60.0, 1);

        var avgDowntimeMinutes = data.Any(d => d.DowntimeMinutes.HasValue)
            ? data.Where(d => d.DowntimeMinutes.HasValue).Average(d => d.DowntimeMinutes.Value)
            : 0;
        var avgDowntimeHours = Math.Round(avgDowntimeMinutes / 60.0, 1);

        // Using better KPIs from current code
        var kpis = new List<AnalyticsSummaryDto>
        {
            new("Total Breakdowns", data.Count().ToString(), "Orange"),
            new("Total Downtime", $"{totalDowntimeHours} h", "Red"),
            new("Avg. Repair Time", $"{avgDowntimeHours} h", "Blue") // Better than "Active Issues"
        };

        var chartData = data
            .GroupBy(d => d.MachineName)
            .Select(g => new ChartDataPoint(
                g.Key,
                Math.Round((double)g.Sum(x => x.DowntimeMinutes ?? 0) / 60.0, 1)
            ))
            .OrderByDescending(x => x.Value)
            .ToList();

        return new AnalyticsReportDto(kpis, chartData, "Downtime Hours by Machine");
    }

    // 3. SHIFT HANDOVER (Using new snippet's method signature)
    public async Task<AnalyticsReportDto> GetShiftHandoverAnalyticsAsync(DateTime start, DateTime end)
    {
        // Pass nulls for filters we don't use (from new snippet)
        var data = await handoverRepo.GetReportDataAsync(start, end, null, null, null);

        int total = data.Count();
        int critical = data.Count(x => x.Priority == "Urgent");

        // Using better KPIs from current code but adapted
        var kpis = new List<AnalyticsSummaryDto>
        {
            new("Total Handovers", total.ToString(), "Blue"),
            new("Urgent Flags", critical.ToString(), critical > 0 ? "Red" : "Green"),
            new("Pending Receive", data.Count(x => !x.IsReceived).ToString(), "Orange")
        };

        // Chart data based on available properties
        var chartData = data
            .GroupBy(x => x.Shift)
            .Select(g => new ChartDataPoint(g.Key, (double)g.Count()))
            .ToList();

        return new AnalyticsReportDto(kpis, chartData, "Total Handovers by Shift");
    }

    // 4. SAMPLE STORAGE (Corrected: Removed Expiry/Disposal logic)
    public async Task<AnalyticsReportDto> GetSampleStorageAnalyticsAsync(DateTime start, DateTime end)
    {
        // Fetch data with null filters for testName and status
        var data = await storageRepo.GetReportDataAsync(start, end, null, null);

        int total = data.Count();

        // Logic: "Pending" means the test is not done yet.
        int pending = data.Count(x => !x.IsTestDone);
        int completed = data.Count(x => x.IsTestDone);

        // Calculate a completion percentage for the KPI card
        double completionRate = total > 0 ? (double)completed / total * 100 : 0;

        var kpis = new List<AnalyticsSummaryDto>
        {
            new("Total Samples", total.ToString(), "Blue"),
            new("Pending Tests", pending.ToString(), pending > 0 ? "Orange" : "Green"),
            new("Completion %", $"{completionRate:F0}%", completionRate > 90 ? "Green" : "Blue")
        };

        // Chart: Total samples grouped by Test Name (e.g., how many "Blood Count" vs "Lipid Profile")
        var chartData = data
            .GroupBy(x => x.TestName ?? "Unknown")
            .Select(g => new ChartDataPoint(g.Key, (double)g.Count()))
            .OrderByDescending(x => x.Value)
            .ToList();

        return new AnalyticsReportDto(kpis, chartData, "Sample Volume by Test Type");
    }

    // 5. CALIBRATION LOG (From new snippet)
    public async Task<AnalyticsReportDto> GetCalibrationAnalyticsAsync(DateTime start, DateTime end)
    {
        var data = await calibrationRepo.GetReportDataAsync(start, end, null, null);

        int total = data.Count();
        int failed = data.Count(x => x.QcResult == "Failed");

        var kpis = new List<AnalyticsSummaryDto>
        {
            new("Total QC Runs", total.ToString(), "Blue"),
            new("Passed", (total - failed).ToString(), "Green"),
            new("Failed", failed.ToString(), failed > 0 ? "Red" : "Green")
        };

        var chartData = data
            .Where(x => x.QcResult == "Failed")
            .GroupBy(x => x.TestName)
            .Select(g => new ChartDataPoint(g.Key, (double)g.Count()))
            .ToList();

        return new AnalyticsReportDto(kpis, chartData, "QC Failures by Instrument");
    }

    // 6. KIT VALIDATION (From new snippet)
    public async Task<AnalyticsReportDto> GetKitValidationAnalyticsAsync(DateTime start, DateTime end)
    {
        var data = await kitRepo.GetReportDataAsync(start, end, null, null);

        int total = data.Count();
        int rejected = data.Count(x => x.ValidationStatus == "Rejected");

        var kpis = new List<AnalyticsSummaryDto>
        {
            new("Kits Validated", total.ToString(), "Blue"),
            new("Accepted", (total - rejected).ToString(), "Green"),
            new("Rejected", rejected.ToString(), "Red")
        };

        var chartData = data
            .GroupBy(x => x.KitName)
            .Select(g => new ChartDataPoint(g.Key, (double)g.Count()))
            .ToList();

        return new AnalyticsReportDto(kpis, chartData, "Validation Volume by Kit");
    }

    // 7. MEDIA STERILITY (From new snippet)
    public async Task<AnalyticsReportDto> GetMediaSterilityAnalyticsAsync(DateTime start, DateTime end)
    {
        var data = await mediaRepo.GetReportDataAsync(start, end, null, null);

        int total = data.Count();
        int contaminated = data.Count(x => x.OverallStatus == "Failed");

        var kpis = new List<AnalyticsSummaryDto>
        {
            new("Batches Checked", total.ToString(), "Blue"),
            new("Sterile", (total - contaminated).ToString(), "Green"),
            new("Contaminated", contaminated.ToString(), "Red")
        };

        var chartData = data
            .Where(x => x.OverallStatus == "Failed")
            .GroupBy(x => x.MediaName)
            .Select(g => new ChartDataPoint(g.Key, (double)g.Count()))
            .ToList();

        return new AnalyticsReportDto(kpis, chartData, "Contamination Events by Media");
    }

    // 8. REPEAT SAMPLE (From new snippet)
    public async Task<AnalyticsReportDto> GetRepeatSampleAnalyticsAsync(DateTime start, DateTime end)
    {
        var data = await repeatRepo.GetReportDataAsync(start, end, null, null);

        int total = data.Count();
        var topDept = data.GroupBy(x => x.Department)
                          .OrderByDescending(g => g.Count())
                          .FirstOrDefault()?.Key ?? "-";

        var kpis = new List<AnalyticsSummaryDto>
        {
            new("Total Repeats", total.ToString(), "Red"),
            new("Top Dept", topDept, "Orange"),
            new("Avg/Day", $"{(total / Math.Max(1, (end - start).TotalDays)):F1}", "Blue")
        };

        var chartData = data
            .GroupBy(x => x.ReasonText ?? "Unknown")
            .Select(g => new ChartDataPoint(g.Key, (double)g.Count()))
            .OrderByDescending(x => x.Value)
            .ToList();

        return new AnalyticsReportDto(kpis, chartData, "Repeat Reasons (Pareto)");
    }
}