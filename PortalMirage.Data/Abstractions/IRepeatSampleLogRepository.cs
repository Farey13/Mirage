using PortalMirage.Core.Dtos;
using PortalMirage.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Data.Abstractions;

public interface IRepeatSampleLogRepository
{
    Task<RepeatSampleLog> CreateAsync(RepeatSampleLog repeatSampleLog);
    Task<IEnumerable<RepeatSampleLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<bool> DeactivateAsync(int repeatId, int userId, string reason);

    Task<IEnumerable<RepeatSampleReportDto>> GetReportDataAsync(DateTime startDate, DateTime endDate, string? reason, string? department); // ADD THIS
}