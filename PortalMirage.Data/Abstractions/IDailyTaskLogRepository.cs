using PortalMirage.Core.Dtos;
using PortalMirage.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Data.Abstractions;

public interface IDailyTaskLogRepository
{
    Task<DailyTaskLog> CreateAsync(DailyTaskLog log);
    Task<IEnumerable<DailyTaskLog>> GetForDateAsync(DateTime date);
    Task<DailyTaskLog?> UpdateStatusAsync(long logId, string status, int? userId, string? comment);
    Task<DailyTaskLog?> ExtendDeadlineAsync(long logId, DateTime newDeadline, string reason, int adminUserId);
    Task<DailyTaskLog?> OverrideLockAsync(long logId, DateTime overrideUntil, string reason, int adminUserId);
    Task<IEnumerable<DailyTaskComplianceReportItemDto>> GetComplianceReportDataAsync(DateTime startDate, DateTime endDate, int? shiftId, string? status); // ADD THIS
    Task<int> GetPendingCountForDateAsync(DateTime date); // ADD THIS LINE
}