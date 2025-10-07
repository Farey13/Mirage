using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Dtos;
using PortalMirage.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortalMirage.Business;

public class ReportService : IReportService
{
    private readonly IMachineBreakdownRepository _machineBreakdownRepository;
    private readonly IHandoverRepository _handoverRepository;
    private readonly IKitValidationRepository _kitValidationRepository;
    private readonly IRepeatSampleLogRepository _repeatSampleLogRepository;
    private readonly IDailyTaskLogRepository _dailyTaskLogRepository;
    private readonly IMediaSterilityCheckRepository _mediaSterilityCheckRepository;

    public ReportService(
        IMachineBreakdownRepository machineBreakdownRepository,
        IHandoverRepository handoverRepository,
        IKitValidationRepository kitValidationRepository,
        IRepeatSampleLogRepository repeatSampleLogRepository,
        IDailyTaskLogRepository dailyTaskLogRepository,
        IMediaSterilityCheckRepository mediaSterilityCheckRepository) // 1. INJECT REPO
    {
        _machineBreakdownRepository = machineBreakdownRepository;
        _handoverRepository = handoverRepository;
        _kitValidationRepository = kitValidationRepository;
        _repeatSampleLogRepository = repeatSampleLogRepository;
        _dailyTaskLogRepository = dailyTaskLogRepository;
        _mediaSterilityCheckRepository = mediaSterilityCheckRepository;
    }

    public async Task<DailyTaskComplianceReportDto> GetDailyTaskComplianceReportAsync(DateTime startDate, DateTime endDate, int? shiftId, string? status)
    {
        var items = (await _dailyTaskLogRepository.GetComplianceReportDataAsync(startDate, endDate, shiftId, status)).ToList();
        var completedCount = items.Count(i => i.Status == "Completed");
        return new DailyTaskComplianceReportDto(items, items.Count, completedCount);
    }

    public async Task<IEnumerable<HandoverReportDto>> GetHandoverReportAsync(DateTime startDate, DateTime endDate, string? shift, string? priority, string? status)
    {
        return await _handoverRepository.GetReportDataAsync(startDate, endDate, shift, priority, status);
    }

    public async Task<IEnumerable<KitValidationReportDto>> GetKitValidationReportAsync(DateTime startDate, DateTime endDate, string? kitName, string? status)
    {
        return await _kitValidationRepository.GetReportDataAsync(startDate, endDate, kitName, status);
    }

    public async Task<IEnumerable<MachineBreakdownReportDto>> GetMachineBreakdownReportAsync(DateTime startDate, DateTime endDate, string? machineName, string? status)
    {
        return await _machineBreakdownRepository.GetReportDataAsync(startDate, endDate, machineName, status);
    }

    public async Task<IEnumerable<RepeatSampleReportDto>> GetRepeatSampleReportAsync(DateTime startDate, DateTime endDate, string? reason, string? department)
    {
        return await _repeatSampleLogRepository.GetReportDataAsync(startDate, endDate, reason, department);
    }

    // 2. ADD NEW METHOD
    public async Task<IEnumerable<MediaSterilityReportDto>> GetMediaSterilityReportAsync(DateTime startDate, DateTime endDate, string? mediaName, string? status)
    {
        return await _mediaSterilityCheckRepository.GetReportDataAsync(startDate, endDate, mediaName, status);
    }
}