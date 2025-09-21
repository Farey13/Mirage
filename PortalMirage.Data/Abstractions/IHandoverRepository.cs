using PortalMirage.Core.Models;

namespace PortalMirage.Data.Abstractions;

public interface IHandoverRepository
{
    Task<Handover> CreateAsync(Handover handover);
    Task<IEnumerable<Handover>> GetPendingAsync();
    Task<Handover?> GetByIdAsync(int handoverId);
    Task<bool> MarkAsReceivedAsync(int handoverId, int userId);
}