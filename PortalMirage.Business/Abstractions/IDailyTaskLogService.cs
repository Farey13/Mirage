using PortalMirage.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Business.Abstractions;

public interface IDailyTaskLogService
{
    Task<IEnumerable<TaskLogDetailDto>> GetTasksForDateAsync(DateTime date);
    Task<TaskLogDetailDto?> UpdateTaskStatusAsync(long logId, string status, int userId, string? comment);
    Task<DailyTaskLog?> ExtendTaskDeadlineAsync(long logId, DateTime newDeadline, string reason, int adminUserId);
    Task<DailyTaskLog?> MarkAsNotApplicableAsync(long logId, int userId, string reason);
    Task<DailyTaskLog?> OverrideLockAsync(long logId, DateTime overrideUntil, string reason, int adminUserId);
}