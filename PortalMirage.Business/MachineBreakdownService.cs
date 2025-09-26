using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using PortalMirage.Data;
using PortalMirage.Data.Abstractions;

namespace PortalMirage.Business;

public class MachineBreakdownService(IMachineBreakdownRepository machineBreakdownRepository) : IMachineBreakdownService
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
}