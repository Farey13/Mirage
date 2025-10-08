using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace PortalMirage.Business;

public class HandoverService(
    IHandoverRepository handoverRepository,
    IAuditLogService auditLogService) : IHandoverService
{
    public async Task<Handover> CreateAsync(Handover handover)
    {
        var newHandover = await handoverRepository.CreateAsync(handover);

        // ADDED: Log the creation event
        await auditLogService.LogAsync(
            userId: newHandover.GivenByUserID,
            actionType: "Create",
            moduleName: "Handover",
            recordId: newHandover.HandoverID.ToString(),
            newValue: newHandover.HandoverNotes
        );

        return newHandover;
    }

    public async Task<IEnumerable<Handover>> GetPendingAsync(DateTime startDate, DateTime endDate)
    {
        return await handoverRepository.GetPendingAsync(startDate, endDate);
    }

    public async Task<IEnumerable<Handover>> GetCompletedAsync(DateTime startDate, DateTime endDate)
    {
        return await handoverRepository.GetCompletedAsync(startDate, endDate);
    }

    public async Task<bool> MarkAsReceivedAsync(int handoverId, int userId)
    {
        // Business Rule: Ensure the handover exists before trying to update it.
        var handover = await handoverRepository.GetByIdAsync(handoverId);
        if (handover is null)
        {
            return false; // Handover not found
        }

        // Business Rule: Don't re-mark an already received handover.
        if (handover.IsReceived)
        {
            return true; // Already received, so the state is correct.
        }

        var success = await handoverRepository.MarkAsReceivedAsync(handoverId, userId);
        if (success)
        {
            // ADDED: Log the receive event
            await auditLogService.LogAsync(
                userId: userId,
                actionType: "Receive",
                moduleName: "Handover",
                recordId: handoverId.ToString()
            );
        }
        return success;
    }

    public async Task<bool> DeactivateAsync(int handoverId, int userId, string reason)
    {
        // First, we check if the handover exists to avoid errors
        var handover = await handoverRepository.GetByIdAsync(handoverId);
        if (handover is null)
        {
            return false; // Handover not found
        }

        var success = await handoverRepository.DeactivateAsync(handoverId, userId, reason);
        if (success)
        {
            // If deactivation was successful, we create an audit log entry
            await auditLogService.LogAsync(userId, "Deactivate", "Handover", handoverId.ToString(), newValue: reason);
        }
        return success;
    }

    public async Task<Handover?> GetByIdAsync(int handoverId)
    {
        return await handoverRepository.GetByIdAsync(handoverId);
    }
}