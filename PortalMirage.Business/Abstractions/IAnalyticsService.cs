using System;
using System.Threading.Tasks;
using PortalMirage.Core.Dtos;

namespace PortalMirage.Business.Abstractions;

public interface IAnalyticsService
{
    Task<AnalyticsReportDto> GetDailyTaskCompletionAsync(DateTime start, DateTime end);
    Task<AnalyticsReportDto> GetMachineDowntimeAsync(DateTime start, DateTime end);
}