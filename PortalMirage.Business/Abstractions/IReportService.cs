using PortalMirage.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Business.Abstractions;

public interface IReportService
{
    Task<IEnumerable<MachineBreakdownReportDto>> GetMachineBreakdownReportAsync(DateTime startDate, DateTime endDate, string? machineName, string? status);
    Task<IEnumerable<HandoverReportDto>> GetHandoverReportAsync(DateTime startDate, DateTime endDate, string? shift, string? priority, string? status);
    Task<IEnumerable<KitValidationReportDto>> GetKitValidationReportAsync(DateTime startDate, DateTime endDate, string? kitName, string? status);
    Task<IEnumerable<RepeatSampleReportDto>> GetRepeatSampleReportAsync(DateTime startDate, DateTime endDate, string? reason, string? department);
    Task<DailyTaskComplianceReportDto> GetDailyTaskComplianceReportAsync(DateTime startDate, DateTime endDate, int? shiftId, string? status);

    Task<IEnumerable<MediaSterilityReportDto>> GetMediaSterilityReportAsync(DateTime startDate, DateTime endDate, string? mediaName, string? status); // ADD THIS
}