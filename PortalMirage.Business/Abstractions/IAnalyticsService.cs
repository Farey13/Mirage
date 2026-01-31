using System;
using System.Threading.Tasks;
using PortalMirage.Core.Dtos;

namespace PortalMirage.Business.Abstractions;

public interface IAnalyticsService
{
    Task<AnalyticsReportDto> GetDailyTaskCompletionAsync(DateTime start, DateTime end);
    Task<AnalyticsReportDto> GetMachineDowntimeAsync(DateTime start, DateTime end);

    // New Methods - Added from new snippet
    Task<AnalyticsReportDto> GetShiftHandoverAnalyticsAsync(DateTime start, DateTime end);
    Task<AnalyticsReportDto> GetSampleStorageAnalyticsAsync(DateTime start, DateTime end);
    Task<AnalyticsReportDto> GetCalibrationAnalyticsAsync(DateTime start, DateTime end);
    Task<AnalyticsReportDto> GetKitValidationAnalyticsAsync(DateTime start, DateTime end);
    Task<AnalyticsReportDto> GetMediaSterilityAnalyticsAsync(DateTime start, DateTime end);
    Task<AnalyticsReportDto> GetRepeatSampleAnalyticsAsync(DateTime start, DateTime end);
}