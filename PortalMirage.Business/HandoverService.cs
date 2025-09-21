using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Models;
using PortalMirage.Data.Abstractions;

namespace PortalMirage.Business;

public class HandoverService(IHandoverRepository handoverRepository) : IHandoverService
{
    public async Task<Handover> CreateAsync(Handover handover)
    {
        return await handoverRepository.CreateAsync(handover);
    }

    public async Task<IEnumerable<Handover>> GetPendingAsync()
    {
        return await handoverRepository.GetPendingAsync();
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

        return await handoverRepository.MarkAsReceivedAsync(handoverId, userId);
    }
}