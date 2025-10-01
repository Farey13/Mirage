namespace PortalMirage.Core.Dtos;

public record DashboardSummaryDto(
    int PendingHandoversCount,
    int UnresolvedBreakdownsCount,
    int PendingDailyTasksCount
);