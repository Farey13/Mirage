using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Business;

public class RepeatSampleLogService : IRepeatSampleLogService
{
    private readonly IRepeatSampleLogRepository _repeatSampleLogRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<RepeatSampleLogService> _logger;

    public RepeatSampleLogService(
        IRepeatSampleLogRepository repeatSampleLogRepository,
        IAuditLogService auditLogService,
        ILogger<RepeatSampleLogService> logger)
    {
        _repeatSampleLogRepository = repeatSampleLogRepository;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<RepeatSampleLog> CreateAsync(RepeatSampleLog repeatSampleLog)
    {
        _logger.LogInformation("Creating repeat sample log for patient: {PatientName}", repeatSampleLog.PatientName);
        var newLog = await _repeatSampleLogRepository.CreateAsync(repeatSampleLog);

        await _auditLogService.LogAsync(
            userId: newLog.LoggedByUserID,
            actionType: "Create",
            moduleName: "RepeatSampleLog",
            recordId: newLog.RepeatID.ToString(),
            newValue: $"Patient: {newLog.PatientName}, Reason: {newLog.ReasonText}"
        );

        _logger.LogInformation("Repeat sample log created with ID: {RepeatId}", newLog.RepeatID);
        return newLog;
    }

    public async Task<IEnumerable<RepeatSampleLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        _logger.LogDebug("Fetching repeat samples from {StartDate} to {EndDate}", startDate, endDate);
        return await _repeatSampleLogRepository.GetByDateRangeAsync(startDate, endDate);
    }

    public async Task<bool> DeactivateAsync(int repeatId, int userId, string reason)
    {
        _logger.LogInformation("Deactivating repeat sample {RepeatId} by user {UserId}", repeatId, userId);
        var success = await _repeatSampleLogRepository.DeactivateAsync(repeatId, userId, reason);
        if (success)
        {
            await _auditLogService.LogAsync(userId, "Deactivate", "RepeatSampleLog", repeatId.ToString(), newValue: reason);
            _logger.LogInformation("Repeat sample {RepeatId} deactivated successfully", repeatId);
        }
        return success;
    }
}