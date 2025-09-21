using PortalMirage.Core.Models;

namespace PortalMirage.Data.Abstractions;

public interface IMachineBreakdownRepository
{
    Task<MachineBreakdown> CreateAsync(MachineBreakdown breakdown);
    Task<IEnumerable<MachineBreakdown>> GetPendingByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<MachineBreakdown?> GetByIdAsync(int breakdownId);
    Task<bool> MarkAsResolvedAsync(int breakdownId, int userId);
}