using PortalMirage.Core.Models;

namespace PortalMirage.Business.Abstractions;

public interface IHandoverService
{
    Task<Handover> CreateAsync(Handover handover);
    Task<IEnumerable<Handover>> GetPendingAsync();
    Task<bool> MarkAsReceivedAsync(int handoverId, int userId);
}