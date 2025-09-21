using PortalMirage.Core.Models;

namespace PortalMirage.Data.Abstractions;

public interface IDailyTaskLogRepository
{
    Task<DailyTaskLog> CreateAsync(DailyTaskLog log);
    Task<IEnumerable<DailyTaskLog>> GetForDateAsync(DateTime date); // Changed from DateOnly
    Task<DailyTaskLog?> UpdateStatusAsync(long logId, string status, int? userId);
}