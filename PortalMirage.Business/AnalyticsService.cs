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
    IShiftHandoverRepository handoverRepo,      // Added
    ISampleStorageRepository storageRepo) : IAnalyticsService  // Added
{
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

        // Updated to calculate completion percentage instead of raw count
        var chartData = logs
            .GroupBy(l => l.LogDate.ToString("dd MMM"))
            .Select(g => new ChartDataPoint(
                g.Key,
                Math.Round(((double)g.Count(x => x.Status == "Complete" || x.Status == "Completed") / g.Count()) * 100, 0)
            ))
            .ToList();

        return new AnalyticsReportDto(kpis, chartData, "Daily Task Completion %");
    }

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

        var kpis = new List<AnalyticsSummaryDto>
        {
            new("Total Breakdowns", data.Count().ToString(), "Orange"),
            new("Total Downtime", $"{totalDowntimeHours} h", "Red"),
            new("Avg. Repair Time", $"{avgDowntimeHours} h", "Blue")
        };

        // Refined Machine Breakdown mapping with null handling
        var chartData = data
            .GroupBy(d => d.MachineName)
            .Select(g => new ChartDataPoint(
                g.Key,
                // Convert to double instead of decimal to match ChartDataPoint constructor
                Math.Round((double)g.Sum(x => x.DowntimeMinutes ?? 0) / 60.0, 1)
            ))
            .OrderByDescending(x => x.Value)
            .ToList();

        return new AnalyticsReportDto(kpis, chartData, "Downtime Hours by Machine (Pareto)");
    }

    public async Task<AnalyticsReportDto> GetShiftHandoverAnalyticsAsync(DateTime start, DateTime end)
    {
        var data = await handoverRepo.GetReportDataAsync(start, end);

        var kpis = new List<AnalyticsSummaryDto>
        {
            new("Total Handovers", data.Count().ToString(), "Blue"),
            new("Critical Flags", data.Count(h => h.IsCritical).ToString(), "Red"),
            new("Avg Duration", $"{data.Average(h => h.DurationMinutes ?? 0):F1} m", "Orange")
        };

        var chartData = data
            .GroupBy(h => h.ShiftName) // e.g., "Shift A", "Shift B"
            .Select(g => new ChartDataPoint(g.Key, (double)g.Count()))
            .OrderByDescending(x => x.Value)
            .ToList();

        return new AnalyticsReportDto(kpis, chartData, "Handovers Completed by Shift");
    }

    public async Task<AnalyticsReportDto> GetSampleStorageAnalyticsAsync(DateTime start, DateTime end)
    {
        var data = await storageRepo.GetReportDataAsync(start, end);

        var activeSamples = data.Count(s => !s.IsDisposed);
        var overdueSamples = data.Count(s => s.ExpiryDate < DateTime.Now && !s.IsDisposed);

        // Calculate actual utilization percentage
        var totalCapacity = 100; // This should come from configuration or repository
        var utilizationPercent = totalCapacity > 0 ? (activeSamples / (double)totalCapacity) * 100 : 0;

        var kpis = new List<AnalyticsSummaryDto>
        {
            new("Total Samples", activeSamples.ToString(), "Blue"),
            new("Overdue Disposal", overdueSamples.ToString(), "Red"),
            new("Utilization", $"{utilizationPercent:F0}%", "Orange") // Dynamic calculation
        };

        var chartData = data
            .Where(s => !s.IsDisposed)
            .GroupBy(s => s.StorageLocation)
            .Select(g => new ChartDataPoint(g.Key, (double)g.Count()))
            .OrderByDescending(x => x.Value)
            .ToList();

        return new AnalyticsReportDto(kpis, chartData, "Current Inventory by Storage Location");
    }
}