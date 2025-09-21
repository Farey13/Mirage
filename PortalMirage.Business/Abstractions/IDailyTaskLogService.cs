using PortalMirage.Core.Models;

namespace PortalMirage.Business.Abstractions;

public interface IDailyTaskLogService
{
    Task<IEnumerable<TaskLogDetailDto>> GetTasksForDateAsync(DateTime date); // Changed from DateOnly
    Task<TaskLogDetailDto?> UpdateTaskStatusAsync(long logId, string status, int userId);
}