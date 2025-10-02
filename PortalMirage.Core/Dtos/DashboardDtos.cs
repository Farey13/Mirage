namespace PortalMirage.Core.Dtos;

public record DashboardSummaryDto(
    int PendingHandoversCount,
    int UnresolvedBreakdownsCount,
    int PendingDailyTasksCount,  
    int PendingSamplesCount // ADD THIS LINE
);