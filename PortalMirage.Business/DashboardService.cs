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
    private readonly ISampleStorageRepository _sampleStorageRepository; // 1. ADD THIS FIELD
    private readonly ITimeProvider _timeProvider;

    public DashboardService(
        IHandoverRepository handoverRepository,
        IMachineBreakdownRepository machineBreakdownRepository,
        IDailyTaskLogRepository dailyTaskLogRepository,
        ISampleStorageRepository sampleStorageRepository, // 2. ADD THIS PARAMETER
        ITimeProvider timeProvider)
    {
        _handoverRepository = handoverRepository;
        _machineBreakdownRepository = machineBreakdownRepository;
        _dailyTaskLogRepository = dailyTaskLogRepository;
        _sampleStorageRepository = sampleStorageRepository; // 3. ADD THIS ASSIGNMENT
        _timeProvider = timeProvider;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        var handoversTask = _handoverRepository.GetPendingCountAsync();
        var breakdownsTask = _machineBreakdownRepository.GetPendingCountAsync();
        var tasksTask = _dailyTaskLogRepository.GetPendingCountForDateAsync(_timeProvider.Today);
        var samplesTask = _sampleStorageRepository.GetPendingCountAsync(); // 4. ADD THIS TASK

        await Task.WhenAll(handoversTask, breakdownsTask, tasksTask, samplesTask); // 5. AWAIT THE NEW TASK

        return new DashboardSummaryDto(
            PendingHandoversCount: await handoversTask,
            UnresolvedBreakdownsCount: await breakdownsTask,
            PendingDailyTasksCount: await tasksTask,
            PendingSamplesCount: await samplesTask // 6. ADD THE NEW COUNT TO THE DTO
        );
    }
}