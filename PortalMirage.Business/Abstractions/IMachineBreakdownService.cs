using PortalMirage.Core.Models;

namespace PortalMirage.Business.Abstractions;

public interface IMachineBreakdownService
{
    Task<MachineBreakdown> CreateAsync(MachineBreakdown breakdown);
    Task<IEnumerable<MachineBreakdown>> GetPendingByDateRangeAsync(DateTime startDate, DateTime endDate);
    // Change this method signature
    Task<bool> MarkAsResolvedAsync(int breakdownId, int userId, string resolutionNotes);

    Task<IEnumerable<MachineBreakdown>> GetResolvedByDateRangeAsync(DateTime startDate, DateTime endDate);
}

