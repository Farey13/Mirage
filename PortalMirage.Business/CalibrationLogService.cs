using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Business;

public class CalibrationLogService(
    ICalibrationLogRepository calibrationLogRepository,
    IAuditLogService auditLogService) : ICalibrationLogService
{
    public async Task<CalibrationLog> CreateAsync(CalibrationLog calibrationLog)
    {
        // In the future, any business rules (like validation) would go here.
        // For now, we pass it directly to the repository.
        var newLog = await calibrationLogRepository.CreateAsync(calibrationLog);

        // ADDED: Log the creation event
        await auditLogService.LogAsync(
            userId: newLog.PerformedByUserID,
            actionType: "Create",
            moduleName: "CalibrationLog",
            recordId: newLog.CalibrationID.ToString(),
            newValue: $"Test: {newLog.TestName}, Result: {newLog.QcResult}"
        );

        return newLog;
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