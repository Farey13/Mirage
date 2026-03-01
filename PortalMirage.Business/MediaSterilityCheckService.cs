using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Business;

public class MediaSterilityCheckService : IMediaSterilityCheckService
{
    private readonly IMediaSterilityCheckRepository _sterilityCheckRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<MediaSterilityCheckService> _logger;

    public MediaSterilityCheckService(
        IMediaSterilityCheckRepository sterilityCheckRepository,
        IAuditLogService auditLogService,
        ILogger<MediaSterilityCheckService> logger)
    {
        _sterilityCheckRepository = sterilityCheckRepository;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<MediaSterilityCheck> CreateAsync(MediaSterilityCheck sterilityCheck)
    {
        _logger.LogInformation("Creating media sterility check for media: {MediaName}", sterilityCheck.MediaName);
        
        if (sterilityCheck.Result25C.Equals("Growth Seen", StringComparison.OrdinalIgnoreCase) ||
            sterilityCheck.Result37C.Equals("Growth Seen", StringComparison.OrdinalIgnoreCase))
        {
            sterilityCheck.OverallStatus = "Failed";
        }
        else
        {
            sterilityCheck.OverallStatus = "Passed";
        }

        var newCheck = await _sterilityCheckRepository.CreateAsync(sterilityCheck);

        await _auditLogService.LogAsync(
            userId: newCheck.PerformedByUserID,
            actionType: "Create",
            moduleName: "MediaSterilityCheck",
            recordId: newCheck.SterilityCheckID.ToString(),
            newValue: $"Media: {newCheck.MediaName}, Lot: {newCheck.MediaLotNumber}, Status: {newCheck.OverallStatus}"
        );

        _logger.LogInformation("Media sterility check created with ID: {SterilityCheckId}, Status: {Status}", 
            newCheck.SterilityCheckID, newCheck.OverallStatus);
        return newCheck;
    }

    public async Task<IEnumerable<MediaSterilityCheck>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        _logger.LogDebug("Fetching media sterility checks from {StartDate} to {EndDate}", startDate, endDate);
        return await _sterilityCheckRepository.GetByDateRangeAsync(startDate, endDate);
    }

    public async Task<bool> DeactivateAsync(int checkId, int userId, string reason)
    {
        _logger.LogInformation("Deactivating media sterility check {CheckId} by user {UserId}", checkId, userId);
        var success = await _sterilityCheckRepository.DeactivateAsync(checkId, userId, reason);
        if (success)
        {
            await _auditLogService.LogAsync(userId, "Deactivate", "MediaSterilityCheck", checkId.ToString(), newValue: reason);
            _logger.LogInformation("Media sterility check {CheckId} deactivated successfully", checkId);
        }
        return success;
    }
}