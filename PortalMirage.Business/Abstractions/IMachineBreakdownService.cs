using PortalMirage.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Business.Abstractions;

public interface IMachineBreakdownService
{
    Task<MachineBreakdown> CreateAsync(MachineBreakdown breakdown);
    Task<IEnumerable<MachineBreakdown>> GetPendingByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<MachineBreakdown>> GetResolvedByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<MachineBreakdown?> GetByIdAsync(int breakdownId); // This was missing
    Task<bool> MarkAsResolvedAsync(int breakdownId, int userId, string resolutionNotes);
    Task<bool> DeactivateAsync(int breakdownId, int userId, string reason); // This is new
}