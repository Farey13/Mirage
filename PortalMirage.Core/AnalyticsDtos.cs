using System;
using System.Collections.Generic;

namespace PortalMirage.Core.Dtos;

// Individual point for charts (X-axis Label, Y-axis Value)
public record ChartDataPoint(string Label, double Value);

// Top-row KPI cards
public record AnalyticsSummaryDto(string Title, string Value, string Color);

// The full package sent to the UI
public record AnalyticsReportDto(
    List<AnalyticsSummaryDto> Kpis,
    List<ChartDataPoint> ChartData,
    string ChartTitle);