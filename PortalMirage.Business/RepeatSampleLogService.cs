using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Business;

public class RepeatSampleLogService(
    IRepeatSampleLogRepository repeatSampleLogRepository,
    IAuditLogService auditLogService) : IRepeatSampleLogService
{
    public async Task<RepeatSampleLog> CreateAsync(RepeatSampleLog repeatSampleLog)
    {
        var newLog = await repeatSampleLogRepository.CreateAsync(repeatSampleLog);

        // ADDED: Log the creation event
        await auditLogService.LogAsync(
            userId: newLog.LoggedByUserID,
            actionType: "Create",
            moduleName: "RepeatSampleLog",
            recordId: newLog.RepeatID.ToString(),
            newValue: $"Patient: {newLog.PatientName}, Reason: {newLog.ReasonText}"
        );

        return newLog;
    }

    public async Task<IEnumerable<RepeatSampleLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await repeatSampleLogRepository.GetByDateRangeAsync(startDate, endDate);
    }

    public async Task<bool> DeactivateAsync(int repeatId, int userId, string reason)
    {
        var success = await repeatSampleLogRepository.DeactivateAsync(repeatId, userId, reason);
        if (success)
        {
            await auditLogService.LogAsync(userId, "Deactivate", "RepeatSampleLog", repeatId.ToString(), newValue: reason);
        }
        return success;
    }
}