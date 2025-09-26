using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using PortalMirage.Data;
using PortalMirage.Data.Abstractions;

namespace PortalMirage.Business;

public class MachineBreakdownService(
    IMachineBreakdownRepository machineBreakdownRepository,
    IAuditLogService auditLogService) : IMachineBreakdownService
{
    public async Task<MachineBreakdown> CreateAsync(MachineBreakdown breakdown)
    {
        return await machineBreakdownRepository.CreateAsync(breakdown);
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

        return await machineBreakdownRepository.MarkAsResolvedAsync(breakdownId, userId, resolutionNotes);
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