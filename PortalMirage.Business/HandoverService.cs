using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace PortalMirage.Business;

public class HandoverService : IHandoverService
{
    private readonly IHandoverRepository _handoverRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<HandoverService> _logger;

    public HandoverService(
        IHandoverRepository handoverRepository,
        IAuditLogService auditLogService,
        ILogger<HandoverService> logger)
    {
        _handoverRepository = handoverRepository;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<Handover> CreateAsync(Handover handover)
    {
        _logger.LogInformation("Creating handover by user {UserId}", handover.GivenByUserID);
        var newHandover = await _handoverRepository.CreateAsync(handover);

        await _auditLogService.LogAsync(
            userId: newHandover.GivenByUserID,
            actionType: "Create",
            moduleName: "Handover",
            recordId: newHandover.HandoverID.ToString(),
            newValue: newHandover.HandoverNotes
        );

        _logger.LogInformation("Handover created with ID: {HandoverId}", newHandover.HandoverID);
        return newHandover;
    }

    public async Task<IEnumerable<Handover>> GetPendingAsync(DateTime startDate, DateTime endDate)
    {
        _logger.LogDebug("Fetching pending handovers from {StartDate} to {EndDate}", startDate, endDate);
        return await _handoverRepository.GetPendingAsync(startDate, endDate);
    }

    public async Task<IEnumerable<Handover>> GetCompletedAsync(DateTime startDate, DateTime endDate)
    {
        _logger.LogDebug("Fetching completed handovers from {StartDate} to {EndDate}", startDate, endDate);
        return await _handoverRepository.GetCompletedAsync(startDate, endDate);
    }

    public async Task<bool> MarkAsReceivedAsync(int handoverId, int userId)
    {
        _logger.LogInformation("Marking handover {HandoverId} as received by user {UserId}", handoverId, userId);
        
        var handover = await _handoverRepository.GetByIdAsync(handoverId);
        if (handover is null)
        {
            _logger.LogWarning("Handover not found: {HandoverId}", handoverId);
            return false;
        }

        if (handover.IsReceived)
        {
            _logger.LogInformation("Handover already received: {HandoverId}", handoverId);
            return true;
        }

        var success = await _handoverRepository.MarkAsReceivedAsync(handoverId, userId);
        if (success)
        {
            await _auditLogService.LogAsync(
                userId: userId,
                actionType: "Receive",
                moduleName: "Handover",
                recordId: handoverId.ToString()
            );
            _logger.LogInformation("Handover {HandoverId} marked as received", handoverId);
        }
        return success;
    }

    public async Task<bool> DeactivateAsync(int handoverId, int userId, string reason)
    {
        _logger.LogInformation("Deactivating handover {HandoverId} by user {UserId}", handoverId, userId);
        
        var handover = await _handoverRepository.GetByIdAsync(handoverId);
        if (handover is null)
        {
            _logger.LogWarning("Handover not found: {HandoverId}", handoverId);
            return false;
        }

        var success = await _handoverRepository.DeactivateAsync(handoverId, userId, reason);
        if (success)
        {
            await _auditLogService.LogAsync(userId, "Deactivate", "Handover", handoverId.ToString(), newValue: reason);
            _logger.LogInformation("Handover {HandoverId} deactivated successfully", handoverId);
        }
        return success;
    }

    public async Task<Handover?> GetByIdAsync(int handoverId)
    {
        return await _handoverRepository.GetByIdAsync(handoverId);
    }
}