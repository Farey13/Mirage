using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;

namespace PortalMirage.Business;

public class CalibrationLogService(
    ICalibrationLogRepository calibrationLogRepository,
    IAuditLogService auditLogService) : ICalibrationLogService
{
    public async Task<CalibrationLog> CreateAsync(CalibrationLog calibrationLog)
    {
        // In the future, any business rules (like validation) would go here.
        // For now, we pass it directly to the repository.
        return await calibrationLogRepository.CreateAsync(calibrationLog);
    }

    public async Task<IEnumerable<CalibrationLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await calibrationLogRepository.GetByDateRangeAsync(startDate, endDate);
    }

    public async Task<bool> DeactivateAsync(int logId, int userId, string reason)
    {
        var success = await calibrationLogRepository.DeactivateAsync(logId, userId, reason);
        if (success)
        {
            await auditLogService.LogAsync(userId, "Deactivate", "CalibrationLog", logId.ToString(), newValue: reason);
        }
        return success;
    }
}