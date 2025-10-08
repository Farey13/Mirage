using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using PortalMirage.Data;
using PortalMirage.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Business;

public class MachineBreakdownService(
    IMachineBreakdownRepository machineBreakdownRepository,
    IAuditLogService auditLogService) : IMachineBreakdownService
{
    public async Task<MachineBreakdown> CreateAsync(MachineBreakdown breakdown)
    {
        var newBreakdown = await machineBreakdownRepository.CreateAsync(breakdown);

        // ADDED: Log the creation event
        await auditLogService.LogAsync(
            userId: newBreakdown.ReportedByUserID,
            actionType: "Create",
            moduleName: "MachineBreakdown",
            recordId: newBreakdown.BreakdownID.ToString(),
            newValue: $"Machine: {newBreakdown.MachineName}, Reason: {newBreakdown.BreakdownReason}"
        );

        return newBreakdown;
    }

    public async Task<IEnumerable<MachineBreakdown>> GetPendingByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await machineBreakdownRepository.GetPendingByDateRangeAsync(startDate, endDate);
    }

    public async Task<bool> MarkAsResolvedAsync(int breakdownId, int userId, string resolutionNotes)
    {
        // Business Rule: Check if the breakdown record exists.
        var breakdown = await machineBreakdownRepository.GetByIdAsync(breakdownId);
        if (breakdown is null)
        {
            return false; // Record not found.
        }

        // Business Rule: Don't re-mark an already resolved issue.
        if (breakdown.IsResolved)
        {
            return true; // Already resolved, so the state is correct.
        }

        var success = await machineBreakdownRepository.MarkAsResolvedAsync(breakdownId, userId, resolutionNotes);
        if (success)
        {
            // ADDED: Log the resolve event
            await auditLogService.LogAsync(
                userId: userId,
                actionType: "Resolve",
                moduleName: "MachineBreakdown",
                recordId: breakdownId.ToString(),
                newValue: resolutionNotes
            );
        }
        return success;
    }

    public async Task<IEnumerable<MachineBreakdown>> GetResolvedByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await machineBreakdownRepository.GetResolvedByDateRangeAsync(startDate, endDate);
    }

    public async Task<MachineBreakdown?> GetByIdAsync(int breakdownId)
    {
        return await machineBreakdownRepository.GetByIdAsync(breakdownId);
    }

    public async Task<bool> DeactivateAsync(int breakdownId, int userId, string reason)
    {
        var success = await machineBreakdownRepository.DeactivateAsync(breakdownId, userId, reason);
        if (success)
        {
            await auditLogService.LogAsync(userId, "Deactivate", "MachineBreakdown", breakdownId.ToString(), newValue: reason);
        }
        return success;
    }
}