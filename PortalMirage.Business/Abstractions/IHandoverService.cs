using PortalMirage.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortalMirage.Business.Abstractions;

public interface IHandoverService
{
    Task<Handover> CreateAsync(Handover handover);
    Task<IEnumerable<Handover>> GetPendingAsync(DateTime startDate, DateTime endDate); // Add date parameters
    Task<IEnumerable<Handover>> GetCompletedAsync(DateTime startDate, DateTime endDate); // Add this new method
    Task<bool> MarkAsReceivedAsync(int handoverId, int userId);
}