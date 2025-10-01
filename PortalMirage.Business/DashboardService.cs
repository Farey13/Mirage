using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Dtos;
using PortalMirage.Data.Abstractions;
using System.Threading.Tasks;

namespace PortalMirage.Business;

public class DashboardService : IDashboardService
{
    private readonly IHandoverRepository _handoverRepository;
    private readonly IMachineBreakdownRepository _machineBreakdownRepository;
    private readonly IDailyTaskLogRepository _dailyTaskLogRepository;
    private readonly ITimeProvider _timeProvider;

    public DashboardService(
        IHandoverRepository handoverRepository,
        IMachineBreakdownRepository machineBreakdownRepository,
        IDailyTaskLogRepository dailyTaskLogRepository,
        ITimeProvider timeProvider)
    {
        _handoverRepository = handoverRepository;
        _machineBreakdownRepository = machineBreakdownRepository;
        _dailyTaskLogRepository = dailyTaskLogRepository;
        _timeProvider = timeProvider;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        var handoversTask = _handoverRepository.GetPendingCountAsync();
        var breakdownsTask = _machineBreakdownRepository.GetPendingCountAsync();
        var tasksTask = _dailyTaskLogRepository.GetPendingCountForDateAsync(_timeProvider.Today);

        await Task.WhenAll(handoversTask, breakdownsTask, tasksTask);

        return new DashboardSummaryDto(
            PendingHandoversCount: await handoversTask,
            UnresolvedBreakdownsCount: await breakdownsTask,
            PendingDailyTasksCount: await tasksTask
        );
    }
}