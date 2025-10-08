using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Business;

public class MediaSterilityCheckService(
    IMediaSterilityCheckRepository sterilityCheckRepository,
    IAuditLogService auditLogService) : IMediaSterilityCheckService
{
    public async Task<MediaSterilityCheck> CreateAsync(MediaSterilityCheck sterilityCheck)
    {
        // Business Rule: Automatically determine the overall status.
        if (sterilityCheck.Result25C.Equals("Growth Seen", StringComparison.OrdinalIgnoreCase) ||
            sterilityCheck.Result37C.Equals("Growth Seen", StringComparison.OrdinalIgnoreCase))
        {
            sterilityCheck.OverallStatus = "Failed";
        }
        else
        {
            sterilityCheck.OverallStatus = "Passed";
        }

        var newCheck = await sterilityCheckRepository.CreateAsync(sterilityCheck);

        // ADDED: Log the creation event
        await auditLogService.LogAsync(
            userId: newCheck.PerformedByUserID,
            actionType: "Create",
            moduleName: "MediaSterilityCheck",
            recordId: newCheck.SterilityCheckID.ToString(),
            newValue: $"Media: {newCheck.MediaName}, Lot: {newCheck.MediaLotNumber}, Status: {newCheck.OverallStatus}"
        );

        return newCheck;
    }

    public async Task<IEnumerable<MediaSterilityCheck>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await sterilityCheckRepository.GetByDateRangeAsync(startDate, endDate);
    }

    public async Task<bool> DeactivateAsync(int checkId, int userId, string reason)
    {
        var success = await sterilityCheckRepository.DeactivateAsync(checkId, userId, reason);
        if (success)
        {
            await auditLogService.LogAsync(userId, "Deactivate", "MediaSterilityCheck", checkId.ToString(), newValue: reason);
        }
        return success;
    }
}