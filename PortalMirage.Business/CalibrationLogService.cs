using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Business;

public class CalibrationLogService : ICalibrationLogService
{
    private readonly ICalibrationLogRepository _calibrationLogRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<CalibrationLogService> _logger;

    public CalibrationLogService(
        ICalibrationLogRepository calibrationLogRepository,
        IAuditLogService auditLogService,
        ILogger<CalibrationLogService> logger)
    {
        _calibrationLogRepository = calibrationLogRepository;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<CalibrationLog> CreateAsync(CalibrationLog calibrationLog)
    {
        _logger.LogInformation("Creating calibration log for test: {TestName}", calibrationLog.TestName);
        var newLog = await _calibrationLogRepository.CreateAsync(calibrationLog);

        await _auditLogService.LogAsync(
            userId: newLog.PerformedByUserID,
            actionType: "Create",
            moduleName: "CalibrationLog",
            recordId: newLog.CalibrationID.ToString(),
            newValue: $"Test: {newLog.TestName}, Result: {newLog.QcResult}"
        );

        _logger.LogInformation("Calibration log created with ID: {CalibrationId}", newLog.CalibrationID);
        return newLog;
    }

    public async Task<IEnumerable<CalibrationLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        _logger.LogDebug("Fetching calibration logs from {StartDate} to {EndDate}", startDate, endDate);
        return await _calibrationLogRepository.GetByDateRangeAsync(startDate, endDate);
    }

    public async Task<bool> DeactivateAsync(int logId, int userId, string reason)
    {
        _logger.LogInformation("Deactivating calibration log {LogId} by user {UserId}", logId, userId);
        var success = await _calibrationLogRepository.DeactivateAsync(logId, userId, reason);
        if (success)
        {
            await _auditLogService.LogAsync(userId, "Deactivate", "CalibrationLog", logId.ToString(), newValue: reason);
            _logger.LogInformation("Calibration log {LogId} deactivated successfully", logId);
        }
        return success;
    }
}