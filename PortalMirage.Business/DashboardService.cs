using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Dtos;
using PortalMirage.Data.Abstractions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace PortalMirage.Business;

public class DashboardService : IDashboardService
{
    private readonly IHandoverRepository _handoverRepository;
    private readonly IMachineBreakdownRepository _machineBreakdownRepository;
    private readonly IDailyTaskLogRepository _dailyTaskLogRepository;
    private readonly ISampleStorageRepository _sampleStorageRepository;
    private readonly ITimeProvider _timeProvider;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IHandoverRepository handoverRepository,
        IMachineBreakdownRepository machineBreakdownRepository,
        IDailyTaskLogRepository dailyTaskLogRepository,
        ISampleStorageRepository sampleStorageRepository,
        ITimeProvider timeProvider,
        ILogger<DashboardService> logger)
    {
        _handoverRepository = handoverRepository;
        _machineBreakdownRepository = machineBreakdownRepository;
        _dailyTaskLogRepository = dailyTaskLogRepository;
        _sampleStorageRepository = sampleStorageRepository;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        _logger.LogInformation("Fetching dashboard summary");
        
        var handoversTask = _handoverRepository.GetPendingCountAsync();
        var breakdownsTask = _machineBreakdownRepository.GetPendingCountAsync();
        var tasksTask = _dailyTaskLogRepository.GetPendingCountForDateAsync(_timeProvider.Today);
        var samplesTask = _sampleStorageRepository.GetPendingCountAsync();

        await Task.WhenAll(handoversTask, breakdownsTask, tasksTask, samplesTask);

        _logger.LogInformation("Dashboard summary retrieved - Handovers: {Handovers}, Breakdowns: {Breakdowns}, Tasks: {Tasks}, Samples: {Samples}",
            await handoversTask, await breakdownsTask, await tasksTask, await samplesTask);

        return new DashboardSummaryDto(
            PendingHandoversCount: await handoversTask,
            UnresolvedBreakdownsCount: await breakdownsTask,
            PendingDailyTasksCount: await tasksTask,
            PendingSamplesCount: await samplesTask
        );
    }
}